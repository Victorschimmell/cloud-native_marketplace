import { useMemo, useState, type ReactNode } from 'react';
import { authApi } from './api/authApi';
import { AuthContext, type AuthContextValue } from './AuthContext';
import { authStorageVersion, clearStoredAuth, getStoredAuth, setStoredAuth } from './authStorage';
import type { AuthCapabilities, AuthProfile, RegistrationResponse, UserAccount } from './types';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [storedAuth, setAuthState] = useState(() => getStoredAuth());

  const value = useMemo<AuthContextValue>(
    () => {
      const capabilities = getCapabilities(storedAuth?.user ?? null, storedAuth?.profile ?? null);

      return {
        user: storedAuth?.user ?? null,
        profile: storedAuth?.profile ?? null,
        capabilities,
        isAuthenticated: capabilities.isAuthenticated,
        login: async (loginRequest) => {
          const response = await authApi.login(loginRequest);
          const nextAuth = {
            version: authStorageVersion,
            token: response.token,
            user: response.user,
            profile: response.profile,
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
              version: authStorageVersion,
              token: response.token,
              user: response.user,
              profile: getRegistrationAuthProfile(response),
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

function getCapabilities(user: UserAccount | null, profile: AuthProfile | null): AuthCapabilities {
  const isAuthenticated = Boolean(user);
  const isAdmin = user?.isAdmin === true;
  const isCustomer = isAuthenticated && Boolean(profile?.customerId) && !isAdmin;
  const isSeller = Boolean(profile?.sellerId);
  const isVerifiedSeller = isSeller && profile?.sellerVerificationStatus === 'Verified';

  return {
    isAuthenticated,
    isCustomer,
    isSeller,
    needsSellerVerification: isSeller && !isVerifiedSeller,
    isVerifiedSeller,
    isAdmin,
  };
}

function getRegistrationAuthProfile(response: RegistrationResponse): AuthProfile {
  return {
    customerId: response.customer?.id ?? null,
    sellerId: response.seller?.id ?? null,
    sellerVerificationStatus: response.seller?.verificationStatus ?? null,
  };
}
