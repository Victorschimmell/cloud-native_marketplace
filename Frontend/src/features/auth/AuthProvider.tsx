import { useMemo, useState, type ReactNode } from 'react';
import { authApi } from './api/authApi';
import { AuthContext, type AuthContextValue } from './AuthContext';
import { clearStoredAuth, getStoredAuth, setStoredAuth } from './authStorage';
import type { RegistrationResponse } from './types';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [storedAuth, setAuthState] = useState(() => getStoredAuth());

  const value = useMemo<AuthContextValue>(
    () => ({
      user: storedAuth?.user ?? null,
      isAuthenticated: Boolean(storedAuth),
      login: async (loginRequest) => {
        const response = await authApi.login(loginRequest);
        setStoredAuth({ token: response.token, user: response.user });
        setAuthState({ token: response.token, user: response.user });
      },
      logout: () => {
        clearStoredAuth();
        setAuthState(null);
      },
      register: async (registerRequest) => {
        let response: RegistrationResponse;

        if (registerRequest.accountType === 'customer') {
          const { accountType, ...customerRequest } = registerRequest;
          void accountType;
          response = await authApi.registerCustomer(customerRequest);
        } else {
          const { accountType, ...sellerRequest } = registerRequest;
          void accountType;
          response = await authApi.registerSeller(sellerRequest);
        }

        if (response.token) {
          setStoredAuth({ token: response.token, user: response.user });
          setAuthState({ token: response.token, user: response.user });
        }

        return response;
      },
    }),
    [storedAuth],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
