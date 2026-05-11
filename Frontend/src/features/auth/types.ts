export type AccountStatus = 'PendingActivation' | 'Active' | 'Suspended' | 'Locked' | 'Disabled';

export type AccountType = 'customer' | 'seller';

export interface UserAccount {
  id: string;
  email: string;
  isAdmin: boolean;
  isBlocked: boolean;
  accountStatus: AccountStatus;
  lastLoginAtUtc: string | null;
}

export interface AuthToken {
  accessToken: string;
  expiresAtUtc: string;
  refreshToken?: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  user: UserAccount;
  token: AuthToken;
  customer?: CustomerProfile | null;
  seller?: SellerProfile | null;
}

export interface RegisterCustomerRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone: string;
  defaultAddressId?: string | null;
}

export interface RegisterSellerRequest {
  email: string;
  password: string;
  businessName: string;
  registrationNumber: string;
  payoutInformation: string;
  defaultAddressId?: string | null;
}

export type RegisterRequest =
  | ({ accountType: Extract<AccountType, 'customer'> } & RegisterCustomerRequest)
  | ({ accountType: Extract<AccountType, 'seller'> } & RegisterSellerRequest);

export interface CustomerProfile {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  phone: string;
  defaultAddressId?: string | null;
}

export interface SellerProfile {
  id: string;
  userId: string;
  businessName: string;
  registrationNumber: string;
  payoutInformation: string;
  defaultAddressId?: string | null;
  verificationStatus: string;
  verifiedAtUtc?: string | null;
}

export interface RegistrationResponse {
  user: UserAccount;
  customer?: CustomerProfile | null;
  seller?: SellerProfile | null;
  token?: AuthToken | null;
}

export interface AuthCapabilities {
  isAuthenticated: boolean;
  isCustomer: boolean;
  isSeller: boolean;
  isVerifiedSeller: boolean;
  isAdmin: boolean;
}

export type AuthCapability = 'authenticated' | 'customer' | 'seller' | 'verifiedSeller' | 'admin';
