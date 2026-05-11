import { useMemo, useState, type ReactNode } from 'react';
import { authApi } from './api/authApi';
import { AuthContext, type AuthContextValue } from './AuthContext';
import { clearStoredAuth, getStoredAuth, setStoredAuth } from './authStorage';
import type { AuthCapabilities, CustomerProfile, RegistrationResponse, SellerProfile, UserAccount } from './types';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [storedAuth, setAuthState] = useState(() => getStoredAuth());

  const value = useMemo<AuthContextValue>(
    () => {
      const capabilities = getCapabilities(storedAuth?.user ?? null, storedAuth?.customer ?? null, storedAuth?.seller ?? null);

      return {
        user: storedAuth?.user ?? null,
        customer: storedAuth?.customer ?? null,
        seller: storedAuth?.seller ?? null,
        capabilities,
        isAuthenticated: capabilities.isAuthenticated,
        login: async (loginRequest) => {
          const response = await authApi.login(loginRequest);
          const nextAuth = {
            version: 1,
            token: response.token,
            user: response.user,
            customer: response.customer ?? null,
            seller: response.seller ?? null,
          };
          setStoredAuth(nextAuth);
          setAuthState(nextAuth);
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
            const nextAuth = {
              version: 1,
              token: response.token,
              user: response.user,
              customer: response.customer ?? null,
              seller: response.seller ?? null,
            };
            setStoredAuth(nextAuth);
            setAuthState(nextAuth);
          }

          return response;
        },
      };
    },
    [storedAuth],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

function getCapabilities(user: UserAccount | null, customer: CustomerProfile | null, seller: SellerProfile | null): AuthCapabilities {
  const isAuthenticated = Boolean(user);
  const isAdmin = user?.isAdmin === true;
  const isCustomer = isAuthenticated && Boolean(customer) && !isAdmin;
  const isSeller = Boolean(seller);

  return {
    isAuthenticated,
    isCustomer,
    isSeller,
    isVerifiedSeller: seller?.verificationStatus === 'Verified',
    isAdmin,
  };
}
