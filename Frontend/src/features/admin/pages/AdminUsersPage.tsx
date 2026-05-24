import { useCallback, useEffect, useMemo, useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import Pagination from '../../../shared/components/Pagination';
import AdminUserDetailsDialog from '../components/AdminUserDetailsDialog';
import UsersTable from '../components/UsersTable';
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
} from '../types';
import './AdminUsersPage.css';

type RoleFilter = AdminUserRole | 'All';
type StatusFilter = 'active' | 'pending' | 'blocked' | 'All';
type PendingAction = {
  userId: string;
  action: 'block' | 'unblock' | 'approve' | 'reject';
} | null;

const usersPageSize = 10;

// User Management page. Filter users by role/status and run admin account actions.
export default function AdminUsersPage() {
  const [roleFilter, setRoleFilter] = useState<RoleFilter>('All');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [error, setError] = useState<string | null>(null);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [pendingAction, setPendingAction] = useState<PendingAction>(null);
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const totalPages = useMemo(() => Math.max(1, Math.ceil(totalCount / usersPageSize)), [totalCount]);

  const loadUsers = useCallback(async (targetPage: number, signal?: AbortSignal) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await adminApi.listUsers(targetPage, usersPageSize, {
        role: roleToApi(roleFilter),
        status: statusToApi(statusFilter),
        signal,
      });
      const mappedUsers = response.items.map(toAdminUser);
      setUsers(mappedUsers);
      setSelectedUser((current) => {
        if (!current) {
          return null;
        }

        return mappedUsers.find((user) => user.id === current.id) ?? null;
      });
      setTotalCount(response.totalCount);
    } catch (requestError) {
      if (requestError instanceof DOMException && requestError.name === 'AbortError') {
        return;
      }
      setError(`Could not load users: ${getErrorMessage(requestError)}`);
      setUsers([]);
      setTotalCount(0);
    } finally {
      if (!signal?.aborted) {
        setIsLoading(false);
      }
    }
  }, [roleFilter, statusFilter]);

  useEffect(() => {
    const abortController = new AbortController();
    void loadUsers(page, abortController.signal);
    return () => abortController.abort();
  }, [loadUsers, page]);

  async function runUserAction(user: AdminUser, action: Exclude<PendingAction, null>['action']) {
    setError(null);
    setActionMessage(null);
    setPendingAction({ userId: user.id, action });

    try {
      if (action === 'block') {
        const reason = window.prompt('Block reason (optional):') ?? undefined;
        await adminApi.blockUser(user.id, reason || undefined);
        setActionMessage(`${user.name} has been blocked.`);
      } else if (action === 'unblock') {
        const reason = window.prompt('Unblock reason (optional):') ?? undefined;
        await adminApi.unblockUser(user.id, reason || undefined);
        setActionMessage(`${user.name} has been unblocked.`);
      } else if (action === 'approve') {
        if (!user.sellerId || !user.pendingVerificationRequestId) {
          throw new Error('This seller does not have a pending verification request.');
        }
        const reviewNotes = window.prompt('Review notes (optional):') ?? undefined;
        await adminApi.verifySeller(user.sellerId, user.pendingVerificationRequestId, {
          approve: true,
          reviewNotes: reviewNotes || undefined,
        });
        setActionMessage(`${user.name} has been approved as a seller.`);
      } else {
        if (!user.sellerId || !user.pendingVerificationRequestId) {
          throw new Error('This seller does not have a pending verification request.');
        }
        const rejectionReason = window.prompt('Rejection reason:');
        if (!rejectionReason?.trim()) {
          return;
        }
        await adminApi.verifySeller(user.sellerId, user.pendingVerificationRequestId, {
          approve: false,
          rejectionReason: rejectionReason.trim(),
        });
        setActionMessage(`${user.name} has been rejected as a seller.`);
      }

      await loadUsers(page);
    } catch (requestError) {
      setError(`Could not ${action} user: ${getErrorMessage(requestError)}`);
    } finally {
      setPendingAction(null);
    }
  }

  function handleRoleChange(value: RoleFilter) {
    setRoleFilter(value);
    setPage(1);
  }

  function handleStatusChange(value: StatusFilter) {
    setStatusFilter(value);
    setPage(1);
  }

  function goToPreviousPage() {
    setPage((currentPage) => Math.max(1, currentPage - 1));
  }

  function goToNextPage() {
    setPage((currentPage) => Math.min(totalPages, currentPage + 1));
  }

  return (
    <PageSkeleton title="User Management" summary="Browse, filter and inspect every account on the platform.">
      <div className="admin-users-page">
        <div className="admin-users-page__panel">
          <div className="admin-users-page__panel-header">
            <div>
              <h2 className="admin-users-page__panel-title">All Users</h2>
              <p className="admin-users-page__panel-subtitle">Visible to all administrators</p>
            </div>
            <div className="admin-users-page__filters">
              <label className="admin-users-page__filter">
                Role:
                <select
                  value={roleFilter}
                  onChange={(event) => handleRoleChange(event.target.value as RoleFilter)}
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
                  onChange={(event) => handleStatusChange(event.target.value as StatusFilter)}
                >
                  <option value="All">All</option>
                  <option value="active">Active</option>
                  <option value="pending">Pending</option>
                  <option value="blocked">Blocked</option>
                </select>
              </label>
            </div>
          </div>

          {actionMessage ? <p className="admin-users-page__notice">{actionMessage}</p> : null}
          {error ? (
            <p className="admin-users-page__empty admin-users-page__empty--error">{error}</p>
          ) : isLoading ? (
            <p className="admin-users-page__empty">Loading users...</p>
          ) : users.length === 0 ? (
            <p className="admin-users-page__empty">No users match the current filters.</p>
          ) : (
            <>
              <UsersTable
                users={users}
                pendingAction={pendingAction}
                onApproveVerification={(user) => void runUserAction(user, 'approve')}
                onBlock={(user) => void runUserAction(user, 'block')}
                onOpenUser={setSelectedUser}
                onRejectVerification={(user) => void runUserAction(user, 'reject')}
                onUnblock={(user) => void runUserAction(user, 'unblock')}
              />
              <Pagination
                currentPage={page}
                disabled={isLoading || pendingAction !== null}
                label="Users pagination"
                onNext={goToNextPage}
                onPrevious={goToPreviousPage}
                totalPages={totalPages}
              />
            </>
          )}
        </div>
        {selectedUser ? (
          <AdminUserDetailsDialog
            user={selectedUser}
            pendingAction={pendingAction?.userId === selectedUser.id ? pendingAction.action : null}
            onApproveVerification={(user) => void runUserAction(user, 'approve')}
            onBlock={(user) => void runUserAction(user, 'block')}
            onClose={() => setSelectedUser(null)}
            onRejectVerification={(user) => void runUserAction(user, 'reject')}
            onUnblock={(user) => void runUserAction(user, 'unblock')}
          />
        ) : null}
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
    sellerId: response.sellerId ?? undefined,
    pendingVerificationRequestId: response.pendingVerificationRequestId ?? undefined,
    registeredOn: response.registeredOn,
  };
}

function getErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const payload = err.payload as Record<string, unknown> | null;
    if (payload && typeof payload === 'object' && 'error' in payload) {
      return String(payload.error);
    }
    return err.message;
  }
  if (err instanceof Error) {
    return err.message;
  }
  return 'Unknown error';
}
