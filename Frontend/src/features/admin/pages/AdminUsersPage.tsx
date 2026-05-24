import { useCallback, useEffect, useMemo, useState } from 'react';
import type { FormEvent } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import Pagination from '../../../shared/components/Pagination';
import AdminUserAccountActionDialog from '../components/AdminUserAccountActionDialog';
import AdminUserDetailsDialog from '../components/AdminUserDetailsDialog';
import AdminSellerVerificationActionDialog from '../components/AdminSellerVerificationActionDialog';
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
type AccountActionDialogState = {
  user: AdminUser;
  action: 'block' | 'unblock';
} | null;
type VerificationActionDialogState = {
  user: AdminUser;
  action: 'approve' | 'reject';
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
  const [accountActionDialog, setAccountActionDialog] = useState<AccountActionDialogState>(null);
  const [accountActionReason, setAccountActionReason] = useState('');
  const [verificationActionDialog, setVerificationActionDialog] = useState<VerificationActionDialogState>(null);
  const [verificationActionText, setVerificationActionText] = useState('');
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

  function openAccountActionDialog(user: AdminUser, action: 'block' | 'unblock') {
    setError(null);
    setActionMessage(null);
    setSelectedUser(null);
    setAccountActionReason('');
    setAccountActionDialog({ user, action });
  }

  function closeAccountActionDialog() {
    if (pendingAction) {
      return;
    }

    setAccountActionDialog(null);
    setAccountActionReason('');
  }

  async function submitAccountAction(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!accountActionDialog) {
      return;
    }

    const { user, action } = accountActionDialog;
    const reason = accountActionReason.trim() || undefined;

    setError(null);
    setActionMessage(null);
    setPendingAction({ userId: user.id, action });

    try {
      if (action === 'block') {
        await adminApi.blockUser(user.id, reason);
        setActionMessage(`${user.name} has been blocked.`);
      } else {
        await adminApi.unblockUser(user.id, reason);
        setActionMessage(`${user.name} has been unblocked.`);
      }

      setAccountActionDialog(null);
      setAccountActionReason('');
      await loadUsers(page);
    } catch (requestError) {
      setError(`Could not ${action} user: ${getErrorMessage(requestError)}`);
    } finally {
      setPendingAction(null);
    }
  }

  function openVerificationActionDialog(user: AdminUser, action: 'approve' | 'reject') {
    setError(null);
    setActionMessage(null);
    setSelectedUser(null);
    setVerificationActionText('');
    setVerificationActionDialog({ user, action });
  }

  function closeVerificationActionDialog() {
    if (pendingAction) {
      return;
    }

    setVerificationActionDialog(null);
    setVerificationActionText('');
  }

  async function submitVerificationAction(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!verificationActionDialog) {
      return;
    }

    const { user, action } = verificationActionDialog;
    setError(null);
    setActionMessage(null);
    setPendingAction({ userId: user.id, action });

    try {
      if (!user.sellerId || !user.pendingVerificationRequestId) {
        throw new Error('This seller does not have a pending verification request.');
      }

      if (action === 'approve') {
        await adminApi.verifySeller(user.sellerId, user.pendingVerificationRequestId, {
          approve: true,
          reviewNotes: verificationActionText.trim() || undefined,
        });
        setActionMessage(`${user.name} has been approved as a seller.`);
      } else {
        const rejectionReason = verificationActionText.trim();
        if (!rejectionReason) {
          return;
        }

        await adminApi.verifySeller(user.sellerId, user.pendingVerificationRequestId, {
          approve: false,
          rejectionReason,
        });
        setActionMessage(`${user.name} has been rejected as a seller.`);
      }

      setVerificationActionDialog(null);
      setVerificationActionText('');
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
                onApproveVerification={(user) => openVerificationActionDialog(user, 'approve')}
                onBlock={(user) => openAccountActionDialog(user, 'block')}
                onOpenUser={setSelectedUser}
                onRejectVerification={(user) => openVerificationActionDialog(user, 'reject')}
                onUnblock={(user) => openAccountActionDialog(user, 'unblock')}
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
            onApproveVerification={(user) => openVerificationActionDialog(user, 'approve')}
            onBlock={(user) => openAccountActionDialog(user, 'block')}
            onClose={() => setSelectedUser(null)}
            onRejectVerification={(user) => openVerificationActionDialog(user, 'reject')}
            onUnblock={(user) => openAccountActionDialog(user, 'unblock')}
          />
        ) : null}
        {accountActionDialog ? (
          <AdminUserAccountActionDialog
            action={accountActionDialog.action}
            isPending={pendingAction?.userId === accountActionDialog.user.id && pendingAction.action === accountActionDialog.action}
            reasonText={accountActionReason}
            user={accountActionDialog.user}
            onChangeReason={setAccountActionReason}
            onClose={closeAccountActionDialog}
            onSubmit={submitAccountAction}
          />
        ) : null}
        {verificationActionDialog ? (
          <AdminSellerVerificationActionDialog
            action={verificationActionDialog.action}
            isPending={pendingAction?.userId === verificationActionDialog.user.id && pendingAction.action === verificationActionDialog.action}
            text={verificationActionText}
            user={verificationActionDialog.user}
            onChangeText={setVerificationActionText}
            onClose={closeVerificationActionDialog}
            onSubmit={submitVerificationAction}
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
