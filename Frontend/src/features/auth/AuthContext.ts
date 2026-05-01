import { createContext } from 'react';
import type { LoginRequest, RegisterRequest, RegistrationResponse, UserAccount } from './types';

export interface AuthContextValue {
  user: UserAccount | null;
  isAuthenticated: boolean;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => void;
  register: (request: RegisterRequest) => Promise<RegistrationResponse>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
