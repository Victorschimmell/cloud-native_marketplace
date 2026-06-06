import type { AuthProfile, AuthToken, UserAccount } from './types';

const authStorageKey = 'marketplace.auth';
export const authStorageVersion = 2;
let currentAuth: StoredAuth | null = null;

export interface StoredAuth {
  version: number;
  token: AuthToken;
  user: UserAccount;
  profile: AuthProfile;
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
  window.sessionStorage.setItem(authStorageKey, JSON.stringify(auth));
  window.localStorage.removeItem(authStorageKey);
}

export function clearStoredAuth() {
  currentAuth = null;
  window.sessionStorage.removeItem(authStorageKey);
  window.localStorage.removeItem(authStorageKey);
}

function readStoredAuth(): StoredAuth | null {
  window.localStorage.removeItem(authStorageKey);
  const rawAuth = window.sessionStorage.getItem(authStorageKey);

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
  return Boolean(auth?.version === authStorageVersion && auth.token.accessToken && auth.token.expiresAtUtc && auth.user.id && auth.profile);
}
