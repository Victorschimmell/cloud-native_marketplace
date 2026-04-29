import { request } from '../../../shared/api/request';
import type { LoginRequest, LoginResponse, RegisterCustomerRequest, RegisterSellerRequest, RegistrationResponse } from '../types';

export const authApi = {
  login: (loginRequest: LoginRequest) =>
    request<LoginResponse>('/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(loginRequest),
    }),

  registerCustomer: (registerRequest: RegisterCustomerRequest) =>
    request<RegistrationResponse>('/api/registration/customer', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(registerRequest),
    }),

  registerSeller: (registerRequest: RegisterSellerRequest) =>
    request<RegistrationResponse>('/api/registration/seller', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(registerRequest),
    }),
};
