// authConfig.ts — B2C version

import type { Configuration } from "@azure/msal-browser";

const {
  VITE_AZURE_CLIENT_ID,            // SPA app id (B2C tenant)
  VITE_REDIRECT_URI,
  VITE_POST_LOGOUT_REDIRECT_URI,
  VITE_API_BASE_URL,
  VITE_API_SCOPE,                   // e.g., https://.../access_as_user
  VITE_B2C_DOMAIN,                  // e.g., gearhawkadb2c.b2clogin.com
  VITE_B2C_AUTHORITY,               // e.g., https://.../B2C_1_gh_susi/v2.0
} = import.meta.env as Record<string, string | undefined>;

export const msalConfig: Configuration = {
  auth: {
    clientId: VITE_AZURE_CLIENT_ID!,
    authority: VITE_B2C_AUTHORITY!,
    knownAuthorities: [VITE_B2C_DOMAIN!], // host only
    redirectUri: VITE_REDIRECT_URI || "http://localhost:5173",
    postLogoutRedirectUri: VITE_POST_LOGOUT_REDIRECT_URI || "http://localhost:5173",
  },
  cache: { cacheLocation: "localStorage", storeAuthStateInCookie: true },
};

export const loginRequest = {
  scopes: [
    VITE_API_SCOPE!,   // your API scope in B2C
    "openid",
    "offline_access",
    "profile", // optional
    "email",   // optional
  ],
};

export const protectedResources = {
  gearHawkApi: {
    endpoint: VITE_API_BASE_URL || "https://localhost:7060/api", // use http if your dev API is http
    scopes: [VITE_API_SCOPE!],
  },
};
