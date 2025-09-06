import { PublicClientApplication, EventType, AccountInfo } from '@azure/msal-browser'
import { msalConfig } from '../config/authConfig'

export const msalInstance = new PublicClientApplication(msalConfig as any)

msalInstance.initialize().catch((err) => {
  console.error('MSAL initialization failed:', err)
})

msalInstance.addEventCallback((event) => {
  if (event.eventType === EventType.LOGIN_SUCCESS && (event as any).payload) {
    const account = (event as any).payload.account as AccountInfo
    msalInstance.setActiveAccount(account)
  }
})


