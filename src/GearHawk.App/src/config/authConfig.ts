// Microsoft Entra (Azure AD) Configuration
const {
  VITE_AZURE_CLIENT_ID,
  VITE_AZURE_TENANT_ID,
  VITE_REDIRECT_URI,
  VITE_POST_LOGOUT_REDIRECT_URI,
  VITE_API_BASE_URL,
  VITE_API_SCOPE,
} = import.meta.env as Record<string, string | undefined>;

export const msalConfig = {
  auth: {
    clientId: VITE_AZURE_CLIENT_ID || 'YOUR_SPA_APP_ID',
    authority: `https://login.microsoftonline.com/${VITE_AZURE_TENANT_ID || 'YOUR_TENANT_ID'}`,
    redirectUri: VITE_REDIRECT_URI || 'http://localhost:5173',
    postLogoutRedirectUri: VITE_POST_LOGOUT_REDIRECT_URI || 'http://localhost:5173',
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
};

// Scopes for sign-in / user info
// Scopes for sign-in / user info
export const loginRequest = {
  scopes: [
    'openid',
    'profile',
    'email',
    VITE_API_SCOPE || 'https://gearhawkadb2c.onmicrosoft.com/d6dae277-1dc7-43e7-8b90-aae1468ca252/EntraGearHawkApiScope'
  ]
};

// API configuration and scopes
export const protectedResources = {
  gearHawkApi: {
    endpoint: VITE_API_BASE_URL || 'https://localhost:7001/api',
    scopes: [VITE_API_SCOPE || 'api://YOUR_API_APP_ID/access_as_user']
  }
};
