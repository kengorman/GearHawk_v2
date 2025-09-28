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

// Inventory types (server uses mixed casing: id is lower-case, others PascalCase)
export interface InventoryNodeDto {
  id: number
  Name: string
  Description: string
  IsInventory: boolean
  CustomerCode: string
  OutOfService: boolean
  NeedsRepair: boolean
  NeedsResupply: boolean
  Quantity: string
  MinimumQuantity: string
  ChildNodesExt: InventoryNodeDto[]
}

export interface InventoryHomeResult {
  root: InventoryNodeDto
  userId?: string | null
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

    // Inventory Home (node + direct children)
    getInventoryHome: builder.query<InventoryHomeResult, number | void>({
      query: (id) => `/InventoryHome?id=${id ?? 1}`,
      transformResponse: (response: any): InventoryHomeResult => {
        // Response shape: { message: string, userId }
        // message is a JSON string of string array; first element is the node JSON
        try {
          const outerArray: string[] = JSON.parse(response?.message ?? '[]')
          const nodeJson = outerArray?.[0] ?? '{}'
          const root: InventoryNodeDto = JSON.parse(nodeJson)
          return { root, userId: response?.userId ?? null }
        } catch (e) {
          return { root: { id: -1, Name: '', Description: '', IsInventory: false, CustomerCode: '', OutOfService: false, NeedsRepair: false, NeedsResupply: false, Quantity: '0', MinimumQuantity: '0', ChildNodesExt: [] }, userId: response?.userId ?? null }
        }
      },
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
  useGetInventoryHomeQuery,
  useGetInventoryByIdQuery,
  useGetReportsQuery,
  useCreateInventoryItemMutation,
  useUpdateInventoryItemMutation,
  useDeleteInventoryItemMutation,
} = gearHawkApi
