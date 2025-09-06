import { configureStore } from '@reduxjs/toolkit'
import { setupListeners } from '@reduxjs/toolkit/query'
import { gearHawkApi } from './api/gearHawkApi'
import authReducer from './slices/authSlice'
import counterReducer from './slices/counterSlice'

export const store = configureStore({
  reducer: {
    auth: authReducer,
    counter: counterReducer,
    [gearHawkApi.reducerPath]: gearHawkApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(gearHawkApi.middleware),
})

setupListeners(store.dispatch)

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
