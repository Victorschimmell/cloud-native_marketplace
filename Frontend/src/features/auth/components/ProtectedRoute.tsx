import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { canUseCapability } from '../../../routes/navigation';
import type { AuthCapability } from '../types';
import { useAuth } from '../useAuth';

interface ProtectedRouteProps {
  capability?: AuthCapability;
}

export function ProtectedRoute({ capability = 'authenticated' }: ProtectedRouteProps) {
  const { capabilities, isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    const returnTo = `${location.pathname}${location.search}${location.hash}`;
    return <Navigate replace to={`/login?returnTo=${encodeURIComponent(returnTo)}`} />;
  }

  if (!canUseCapability(capabilities, capability)) {
    return <Navigate replace to="/not-found" />;
  }

  return <Outlet />;
}
