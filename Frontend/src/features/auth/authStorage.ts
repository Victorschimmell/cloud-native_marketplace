import type { AuthToken, UserAccount } from './types';

const authStorageKey = 'marketplace.auth';
let currentAuth: StoredAuth | null = null;

export interface StoredAuth {
  token: AuthToken;
  user: UserAccount;
}

export function getStoredAuth(): StoredAuth | null {
  if (!currentAuth?.token.accessToken || !currentAuth.user.id) {
    clearStoredAuth();
    return null;
  }

  if (new Date(currentAuth.token.expiresAtUtc).getTime() <= Date.now()) {
    clearStoredAuth();
    return null;
  }

  return currentAuth;
}

export function getStoredAccessToken(): string | null {
  return getStoredAuth()?.token.accessToken ?? null;
}

export function setStoredAuth(auth: StoredAuth) {
  currentAuth = auth;
  window.localStorage.removeItem(authStorageKey);
}

export function clearStoredAuth() {
  currentAuth = null;
  window.localStorage.removeItem(authStorageKey);
}
