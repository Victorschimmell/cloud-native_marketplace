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
  { label: 'Admin Users', to: '/admin/users', audience: 'admin' },
  { label: 'Seller Reviews', to: '/admin/verifications', audience: 'admin' },
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
  // Hide the marketplace-browsing items for Sellers and show only seller related items
  if (capabilities.isSeller && (item.audience === 'public' || item.audience === 'customer')) {
    return false;
  }

  return item.audience === 'public' || canUseCapability(capabilities, item.audience);
}
