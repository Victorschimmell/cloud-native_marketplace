import type { AuthToken, UserAccount } from './types';

const authStorageKey = 'marketplace.auth';

export interface StoredAuth {
  token: AuthToken;
  user: UserAccount;
}

export function getStoredAuth(): StoredAuth | null {
  if (typeof window === 'undefined') {
    return null;
  }

  const rawValue = window.localStorage.getItem(authStorageKey);
  if (!rawValue) {
    return null;
  }

  try {
    const storedAuth = JSON.parse(rawValue) as StoredAuth;
    if (!storedAuth.token?.accessToken || !storedAuth.user?.id) {
      clearStoredAuth();
      return null;
    }

    if (new Date(storedAuth.token.expiresAtUtc).getTime() <= Date.now()) {
      clearStoredAuth();
      return null;
    }

    return storedAuth;
  } catch {
    clearStoredAuth();
    return null;
  }
}

export function getStoredAccessToken(): string | null {
  return getStoredAuth()?.token.accessToken ?? null;
}

export function setStoredAuth(auth: StoredAuth) {
  window.localStorage.setItem(authStorageKey, JSON.stringify(auth));
}

export function clearStoredAuth() {
  window.localStorage.removeItem(authStorageKey);
}
