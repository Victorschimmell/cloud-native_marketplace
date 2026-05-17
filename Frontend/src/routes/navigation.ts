import type { AuthCapabilities, AuthCapability } from '../features/auth/types';

export type NavigationAudience = 'public' | AuthCapability;

export interface NavigationItem {
  label: string;
  to: string;
  audience: NavigationAudience;
}

export const primaryNavigationItems: NavigationItem[] = [
  { label: 'Browse Products', to: '/products', audience: 'public' },
  { label: 'Categories', to: '/categories', audience: 'public' },
  { label: 'Orders', to: '/orders', audience: 'customer' },
  { label: 'Seller Verification', to: '/seller/verification', audience: 'sellerVerification' },
  { label: 'Seller Products', to: '/seller/products', audience: 'verifiedSeller' },
  { label: 'Seller Orders', to: '/seller/orders', audience: 'verifiedSeller' },
  { label: 'Admin Dashboard', to: '/analytics', audience: 'admin' },
  { label: 'Admin Users', to: '/admin/users', audience: 'admin' },
  { label: 'Audit Logs', to: '/admin/audit', audience: 'admin' },
];

export function canUseCapability(capabilities: AuthCapabilities, capability: AuthCapability) {
  switch (capability) {
    case 'authenticated':
      return capabilities.isAuthenticated;
    case 'customer':
      return capabilities.isCustomer;
    case 'seller':
      return capabilities.isSeller;
    case 'sellerVerification':
      return capabilities.needsSellerVerification;
    case 'verifiedSeller':
      return capabilities.isVerifiedSeller;
    case 'admin':
      return capabilities.isAdmin;
  }
}

export function canShowNavigationItem(capabilities: AuthCapabilities, item: NavigationItem) {
  // Sellers and Admins don't need the shopping links, only their own pages.
  if ((capabilities.isSeller || capabilities.isAdmin) && (item.audience === 'public' || item.audience === 'customer')) {
    return false;
  }

  return item.audience === 'public' || canUseCapability(capabilities, item.audience);
}
