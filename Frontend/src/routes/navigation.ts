import type { AuthCapabilities, AuthCapability } from '../features/auth/types';

export type NavigationAudience = 'all' | 'public' | AuthCapability;

export interface NavigationItem {
  label: string;
  to: string;
  audience: NavigationAudience;
  activePathPrefixes?: string[];
  end?: boolean;
  showForAdmin?: boolean;
}

export const primaryNavigationItems: NavigationItem[] = [
  { label: 'Home', to: '/', audience: 'all', end: true },
  { label: 'Browse Products', to: '/products', audience: 'public', showForAdmin: true },
  { label: 'Categories', to: '/categories', audience: 'public' },
  { label: 'Orders', to: '/orders', audience: 'customer' },
  { label: 'Seller Verification', to: '/seller/verification', audience: 'sellerVerification' },
  {
    label: 'Seller Dashboard',
    to: '/seller/products',
    audience: 'verifiedSeller',
    activePathPrefixes: ['/seller/products', '/seller/orders'],
  },
  { label: 'Admin Dashboard', to: '/admin/dashboard', audience: 'admin' },
  { label: 'User Management', to: '/admin/users', audience: 'admin' },
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
  if (item.audience === 'all') {
    return true;
  }

  if (capabilities.isSeller && item.audience === 'public') {
    return true;
  }

  // Admins don't need most shopping links, only their own pages.
  if ((capabilities.isSeller || capabilities.isAdmin) && (item.audience === 'public' || item.audience === 'customer')) {
    return capabilities.isAdmin && item.showForAdmin === true;
  }

  return item.audience === 'public' || canUseCapability(capabilities, item.audience);
}
