import { useEffect, useMemo, useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import UsersTable from '../components/UsersTable';
import UserDetailsPanel from '../components/UserDetailsPanel';
import {
  adminApi,
  type AdminUserResponse,
  type AdminUserRoleApi,
  type AdminUserStatusApi,
} from '../api/adminApi';
import type {
  AdminUser,
  AdminUserRole,
  AdminUserStatus,
} from '../data/placeholderData';
import './AdminUsersPage.css';

type RoleFilter = AdminUserRole | 'All';
type StatusFilter = 'active' | 'pending' | 'blocked' | 'All';

// User Management page. Filter users by role + status, click "Manage" to see details.
export default function AdminUsersPage() {
  const [roleFilter, setRoleFilter] = useState<RoleFilter>('All');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null);
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const abortController = new AbortController();

    async function load() {
      setIsLoading(true);
      setError(null);
      try {
        const response = await adminApi.listUsers(1, 100, {
          role: roleToApi(roleFilter),
          status: statusToApi(statusFilter),
          signal: abortController.signal,
        });
        setUsers(response.items.map(toAdminUser));
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }
        setError('Could not load users.');
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void load();
    return () => abortController.abort();
  }, [roleFilter, statusFilter]);

  const filteredUsers = useMemo(() => users, [users]);

  return (
    <PageSkeleton title="User Management" summary="Browse, filter and inspect every account on the platform.">
      <div className="admin-users-page">
        <div className="admin-users-page__panel">
          <div className="admin-users-page__panel-header">
            <h2 className="admin-users-page__panel-title">All Users</h2>
            <div className="admin-users-page__filters">
              <label className="admin-users-page__filter">
                Role:
                <select
                  value={roleFilter}
                  onChange={(event) => setRoleFilter(event.target.value as RoleFilter)}
                >
                  <option value="All">All</option>
                  <option value="Customer">Customer</option>
                  <option value="Seller">Seller</option>
                  <option value="Admin">Admin</option>
                </select>
              </label>

              <label className="admin-users-page__filter">
                Status:
                <select
                  value={statusFilter}
                  onChange={(event) => setStatusFilter(event.target.value as StatusFilter)}
                >
                  <option value="All">All</option>
                  <option value="active">Active</option>
                  <option value="pending">Pending</option>
                  <option value="blocked">Blocked</option>
                </select>
              </label>
            </div>
          </div>

          {error ? <p className="admin-users-page__empty">{error}</p> : null}
          {isLoading ? (
            <p className="admin-users-page__empty">Loading users...</p>
          ) : (
            <UsersTable users={filteredUsers} onManage={setSelectedUser} />
          )}
        </div>

        <UserDetailsPanel user={selectedUser} />
      </div>
    </PageSkeleton>
  );
}

function roleToApi(filter: RoleFilter): AdminUserRoleApi {
  switch (filter) {
    case 'Customer':
      return 'Customer';
    case 'Seller':
      return 'Seller';
    case 'Admin':
      return 'Admin';
    default:
      return 'Any';
  }
}

function statusToApi(filter: StatusFilter): AdminUserStatusApi {
  switch (filter) {
    case 'active':
      return 'Active';
    case 'pending':
      return 'PendingVerification';
    case 'blocked':
      return 'Blocked';
    default:
      return 'Any';
  }
}

function toAdminUser(response: AdminUserResponse): AdminUser {
  return {
    id: response.id,
    name: response.name,
    email: response.email,
    company: response.company ?? undefined,
    role: response.role as AdminUserRole,
    status: response.status as AdminUserStatus,
    registeredOn: response.registeredOn,
  };
}
