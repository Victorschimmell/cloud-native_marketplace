import Modal from '../../../shared/components/Modal';
import type { AdminUser } from '../types';
import './AdminUserDetailsDialog.css';

type UserDialogAction = 'block' | 'unblock' | 'approve' | 'reject';

interface AdminUserDetailsDialogProps {
  pendingAction?: UserDialogAction | null;
  user: AdminUser;
  onApproveVerification: (user: AdminUser) => void;
  onBlock: (user: AdminUser) => void;
  onClose: () => void;
  onRejectVerification: (user: AdminUser) => void;
  onUnblock: (user: AdminUser) => void;
}

export default function AdminUserDetailsDialog({
  pendingAction = null,
  user,
  onApproveVerification,
  onBlock,
  onClose,
  onRejectVerification,
  onUnblock,
}: AdminUserDetailsDialogProps) {
  const isBlocked = user.status === 'blocked';
  const isPendingVerification = user.status === 'pending verification';
  const canReviewSeller = isPendingVerification
    && Boolean(user.sellerId)
    && Boolean(user.pendingVerificationRequestId);
  const hasAccountAction = canReviewSeller || (!isPendingVerification && (isBlocked || user.role !== 'Admin'));

  return (
    <Modal
      title={user.name}
      subtitle={user.email}
      onClose={onClose}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" onClick={onClose}>
            Close
          </button>
          {canReviewSeller ? (
            <>
              <button
                type="button"
                className="modal__button modal__button--success"
                disabled={pendingAction !== null}
                onClick={() => onApproveVerification(user)}
              >
                {pendingAction === 'approve' ? 'Working...' : 'Approve'}
              </button>
              <button
                type="button"
                className="modal__button modal__button--danger"
                disabled={pendingAction !== null}
                onClick={() => onRejectVerification(user)}
              >
                {pendingAction === 'reject' ? 'Working...' : 'Reject'}
              </button>
            </>
          ) : null}
          {!isPendingVerification && isBlocked ? (
            <button
              type="button"
              className="modal__button modal__button--success"
              disabled={pendingAction !== null}
              onClick={() => onUnblock(user)}
            >
              {pendingAction === 'unblock' ? 'Working...' : 'Unblock'}
            </button>
          ) : !isPendingVerification && user.role !== 'Admin' ? (
            <button
              type="button"
              className="modal__button modal__button--danger"
              disabled={pendingAction !== null}
              onClick={() => onBlock(user)}
            >
              {pendingAction === 'block' ? 'Working...' : 'Block'}
            </button>
          ) : null}
        </>
      )}
    >
      <div className="admin-user-details__badges">
        <span className="admin-user-details__badge">{user.role}</span>
        <span className={`admin-user-details__badge admin-user-details__badge--${badgeKey(user.status)}`}>
          {user.status}
        </span>
      </div>

      <dl className="admin-user-details__grid">
        <div>
          <dt>Email</dt>
          <dd>{user.email}</dd>
        </div>
        <div>
          <dt>Registered</dt>
          <dd>{formatDate(user.registeredOn)}</dd>
        </div>
        <div>
          <dt>User ID</dt>
          <dd>{user.id}</dd>
        </div>
        <div>
          <dt>Role</dt>
          <dd>{user.role}</dd>
        </div>
        {user.company ? (
          <div>
            <dt>Company</dt>
            <dd>{user.company}</dd>
          </div>
        ) : null}
        {user.sellerId ? (
          <div>
            <dt>Seller ID</dt>
            <dd>{user.sellerId}</dd>
          </div>
        ) : null}
      </dl>

      {!hasAccountAction ? (
        <p className="admin-user-details__note">No account actions are available for this user.</p>
      ) : null}
    </Modal>
  );
}

function badgeKey(status: string): string {
  return status.replace(/\s+/g, '-');
}

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('en-GB');
}
