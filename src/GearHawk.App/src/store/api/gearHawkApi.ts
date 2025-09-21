import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import { InteractionRequiredAuthError } from '@azure/msal-browser'
import type { BaseQueryFn, FetchArgs, FetchBaseQueryError } from '@reduxjs/toolkit/query'
import { msalInstance } from '../../auth/msalInstance'
import { protectedResources } from '../../config/authConfig'

// Define types for your API responses
export interface User {
  id: string
  name: string
  email: string
}

export interface LoginRequest {
  uid: string
  email: string
  displayName: string
}

export interface LoginResponse {
  success: boolean
  data: { uid: string }
  message?: string
}

// Create the API slice
const rawBaseQuery = fetchBaseQuery({
  baseUrl: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7060/api',
})

const baseQueryWithMsal: BaseQueryFn<string | FetchArgs, unknown, FetchBaseQueryError> = async (
  args,
  api,
  extraOptions
) => {
  let adjustedArgs: FetchArgs | string = args
  try {
    const account = msalInstance.getActiveAccount() || msalInstance.getAllAccounts()[0]
    if (account) {
      const tokenResponse = await msalInstance.acquireTokenSilent({
        account,
        scopes: protectedResources.gearHawkApi.scopes as string[],
      })
      const accessToken = tokenResponse?.accessToken
      if (accessToken) {
        const mergeHeaders = (
          input?: Headers | string[][] | Record<string, string | undefined>
        ): Record<string, string> => {
          const out: Record<string, string> = {}
          if (!input) return out
          if (input instanceof Headers) {
            input.forEach((v, k) => {
              if (v) out[k] = v
            })
          } else if (Array.isArray(input)) {
            input.forEach(([k, v]) => {
              if (v) out[k] = v
            })
          } else {
            Object.entries(input).forEach(([k, v]) => {
              if (v) out[k] = v
            })
          }
          return out
        }

        if (typeof args === 'string') {
          adjustedArgs = {
            url: args,
            headers: { authorization: `Bearer ${accessToken}` },
          }
        } else {
          const existing = mergeHeaders((args as FetchArgs).headers)
          adjustedArgs = {
            ...(args as FetchArgs),
            headers: { ...existing, authorization: `Bearer ${accessToken}` },
          }
        }
      }
    }
  } catch (err: any) {
    // If silent token acquisition requires interaction, prompt once and attach token
    if (err instanceof InteractionRequiredAuthError) {
      try {
        const account = msalInstance.getActiveAccount() || msalInstance.getAllAccounts()[0]
        if (account) {
          await msalInstance.acquireTokenRedirect({
            account,
            scopes: protectedResources.gearHawkApi.scopes as string[],
          })
          // After redirect returns, the original request can be retried by the caller if needed
          // Here we simply proceed without token; consumers should re-trigger the request.
        }
      } catch (popupErr) {
        console.warn('MSAL interactive acquisition failed', popupErr)
      }
    } else {
      console.warn('Failed to acquire MSAL token silently', err)
    }
  }
  return rawBaseQuery(adjustedArgs, api, extraOptions)
}

export const gearHawkApi = createApi({
  reducerPath: 'gearHawkApi',
  baseQuery: baseQueryWithMsal,
  tagTypes: ['User', 'Inventory', 'Report'],
  endpoints: (builder) => ({
    // Authentication endpoints
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (credentials) => ({
        url: '/auth/login',
        method: 'POST',
        body: credentials,
      }),
      invalidatesTags: ['User'],
    }),

    // User profile endpoint
    getUserProfile: builder.query<any, void>({
      query: () => '/auth/profile',
      providesTags: ['User'],
    }),

    // Example inventory endpoints (you can expand these)
    getInventory: builder.query<any[], void>({
      query: () => '/inventory',
      providesTags: ['Inventory'],
    }),

    getInventoryById: builder.query<any, string>({
      query: (id) => `/inventory/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Inventory', id }],
    }),

    // Example report endpoints
    getReports: builder.query<any[], void>({
      query: () => '/reports',
      providesTags: ['Report'],
    }),

    // Mutations for creating/updating data
    createInventoryItem: builder.mutation<any, any>({
      query: (item) => ({
        url: '/inventory',
        method: 'POST',
        body: item,
      }),
      invalidatesTags: ['Inventory'],
    }),

    updateInventoryItem: builder.mutation<any, { id: string; data: any }>({
      query: ({ id, data }) => ({
        url: `/inventory/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Inventory', id },
        'Inventory'
      ],
    }),

    deleteInventoryItem: builder.mutation<void, string>({
      query: (id) => ({
        url: `/inventory/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Inventory'],
    }),
  }),
})

// Export hooks for use in components
export const {
  useLoginMutation,
  useGetUserProfileQuery,
  useGetInventoryQuery,
  useGetInventoryByIdQuery,
  useGetReportsQuery,
  useCreateInventoryItemMutation,
  useUpdateInventoryItemMutation,
  useDeleteInventoryItemMutation,
} = gearHawkApi
