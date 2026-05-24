import { Outlet } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import SellerDashboardLockedPage from '../pages/SellerDashboardLockedPage';

export default function SellerVerificationGuard() {
  const { capabilities } = useAuth();

  if (!capabilities.isVerifiedSeller) {
    return <SellerDashboardLockedPage />;
  }

  return <Outlet />;
}
