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
  // Login resolves with capabilities so callers can choose a role-appropriate page.
  login: (request: LoginRequest) => Promise<AuthCapabilities>;
  logout: () => void;
  register: (request: RegisterRequest) => Promise<RegistrationResponse>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
