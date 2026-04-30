import type { AuthToken, UserAccount } from './types';

const authStorageKey = 'marketplace.auth';
let currentAuth: StoredAuth | null = null;

export interface StoredAuth {
  token: AuthToken;
  user: UserAccount;
}

export function getStoredAuth(): StoredAuth | null {
  currentAuth ??= readStoredAuth();

  if (!isValidStoredAuth(currentAuth)) {
    clearStoredAuth();
    return null;
  }

  const expiresAt = new Date(currentAuth.token.expiresAtUtc).getTime();
  if (!Number.isFinite(expiresAt) || expiresAt <= Date.now()) {
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
  window.localStorage.setItem(authStorageKey, JSON.stringify(auth));
}

export function clearStoredAuth() {
  currentAuth = null;
  window.localStorage.removeItem(authStorageKey);
}

function readStoredAuth(): StoredAuth | null {
  const rawAuth = window.localStorage.getItem(authStorageKey);

  if (!rawAuth) {
    return null;
  }

  try {
    return JSON.parse(rawAuth) as StoredAuth;
  } catch {
    return null;
  }
}

function isValidStoredAuth(auth: StoredAuth | null): auth is StoredAuth {
  return Boolean(auth?.token.accessToken && auth.token.expiresAtUtc && auth.user.id);
}
