import { createContext } from 'react';
import type {
  AuthProfile,
  AuthCapabilities,
  LoginRequest,
  RegisterRequest,
  RegistrationResponse,
  UserAccount,
} from './types';

export interface AuthContextValue {
  user: UserAccount | null;
  profile: AuthProfile | null;
  capabilities: AuthCapabilities;
  isAuthenticated: boolean;
  // login resolves with the capabilities so there is distiction in the role
  // for example (e.g. sellers → dashboard, customers → /products).
  login: (request: LoginRequest) => Promise<AuthCapabilities>;
  logout: () => void;
  register: (request: RegisterRequest) => Promise<RegistrationResponse>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
