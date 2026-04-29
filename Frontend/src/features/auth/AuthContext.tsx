import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { authApi } from './api/authApi';
import { clearStoredAuth, getStoredAuth, setStoredAuth } from './authStorage';
import type { LoginRequest, RegisterRequest, RegistrationResponse, UserAccount } from './types';

interface AuthContextValue {
  user: UserAccount | null;
  isAuthenticated: boolean;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => void;
  register: (request: RegisterRequest) => Promise<RegistrationResponse>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

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
        if (registerRequest.accountType === 'customer') {
          const { accountType, ...customerRequest } = registerRequest;
          void accountType;
          return authApi.registerCustomer(customerRequest);
        }

        const { accountType, ...sellerRequest } = registerRequest;
        void accountType;
        return authApi.registerSeller(sellerRequest);
      },
    }),
    [storedAuth],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.');
  }

  return context;
}
