import { createContext } from 'react';
import type {
  AuthCapabilities,
  CustomerProfile,
  LoginRequest,
  RegisterRequest,
  RegistrationResponse,
  SellerProfile,
  UserAccount,
} from './types';

export interface AuthContextValue {
  user: UserAccount | null;
  customer: CustomerProfile | null;
  seller: SellerProfile | null;
  capabilities: AuthCapabilities;
  isAuthenticated: boolean;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => void;
  register: (request: RegisterRequest) => Promise<RegistrationResponse>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
