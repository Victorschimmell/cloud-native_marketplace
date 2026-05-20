import type { AdminUser } from '../types';

type UserTableAction = 'block' | 'unblock' | 'approve' | 'reject';

interface UsersTableProps {
  users: AdminUser[];
  pendingAction?: {
    userId: string;
    action: UserTableAction;
  } | null;
  onApproveVerification: (user: AdminUser) => void;
  onBlock: (user: AdminUser) => void;
  onRejectVerification: (user: AdminUser) => void;
  onUnblock: (user: AdminUser) => void;
}

// Users table on the User Management page. Filters, pagination and mutations live in the parent.
export default function UsersTable({
  users,
  pendingAction = null,
  onApproveVerification,
  onBlock,
  onRejectVerification,
  onUnblock,
}: UsersTableProps) {
  return (
    <div className="admin-users-page__table-wrap">
      <table className="admin-users-page__table">
        <thead>
          <tr>
            <th>User</th>
            <th>Role</th>
            <th>Status</th>
            <th>Registered</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <UserRow
              key={user.id}
              user={user}
              pendingAction={pendingAction?.userId === user.id ? pendingAction.action : null}
              onApproveVerification={onApproveVerification}
              onBlock={onBlock}
              onRejectVerification={onRejectVerification}
              onUnblock={onUnblock}
            />
          ))}
        </tbody>
      </table>
    </div>
  );
}

interface UserRowProps {
  user: AdminUser;
  pendingAction: UserTableAction | null;
  onApproveVerification: (user: AdminUser) => void;
  onBlock: (user: AdminUser) => void;
  onRejectVerification: (user: AdminUser) => void;
  onUnblock: (user: AdminUser) => void;
}

function UserRow({
  user,
  pendingAction,
  onApproveVerification,
  onBlock,
  onRejectVerification,
  onUnblock,
}: UserRowProps) {
  const isBlocked = user.status === 'blocked';
  const canReviewSeller = user.status === 'pending verification'
    && Boolean(user.sellerId)
    && Boolean(user.pendingVerificationRequestId);

  return (
    <tr>
      <td>
        <div className="admin-users-page__user-cell">
          <p className="admin-users-page__user-name">{user.name}</p>
          <p className="admin-users-page__user-email">{user.email}</p>
          {user.company ? <p className="admin-users-page__user-company">{user.company}</p> : null}
        </div>
      </td>
      <td>{user.role}</td>
      <td>
        <span className={`admin-users-page__badge admin-users-page__badge--${badgeKey(user.status)}`}>
          {user.status}
        </span>
      </td>
      <td>{formatDate(user.registeredOn)}</td>
      <td>
        <div className="admin-users-page__actions">
          {canReviewSeller ? (
            <>
              <button
                type="button"
                className="admin-users-page__action admin-users-page__action--approve"
                disabled={pendingAction !== null}
                onClick={() => onApproveVerification(user)}
              >
                {pendingAction === 'approve' ? 'Working...' : 'Approve'}
              </button>
              <button
                type="button"
                className="admin-users-page__action admin-users-page__action--reject"
                disabled={pendingAction !== null}
                onClick={() => onRejectVerification(user)}
              >
                {pendingAction === 'reject' ? 'Working...' : 'Reject'}
              </button>
            </>
          ) : null}

          {isBlocked ? (
            <button
              type="button"
              className="admin-users-page__action admin-users-page__action--unblock"
              disabled={pendingAction !== null}
              onClick={() => onUnblock(user)}
            >
              {pendingAction === 'unblock' ? 'Working...' : 'Unblock'}
            </button>
          ) : user.role !== 'Admin' ? (
            <button
              type="button"
              className="admin-users-page__action admin-users-page__action--block"
              disabled={pendingAction !== null}
              onClick={() => onBlock(user)}
            >
              {pendingAction === 'block' ? 'Working...' : 'Block'}
            </button>
          ) : null}

          {!canReviewSeller && !isBlocked && user.role === 'Admin' ? (
            <span className="admin-users-page__no-action">No actions</span>
          ) : null}
        </div>
      </td>
    </tr>
  );
}

function badgeKey(status: string): string {
  return status.replace(/\s+/g, '-');
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-GB');
}
