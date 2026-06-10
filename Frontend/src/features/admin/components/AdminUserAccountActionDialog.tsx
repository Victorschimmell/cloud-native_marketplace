import type { FormEvent } from 'react';
import Modal from '../../../shared/components/Modal';
import type { AdminUser } from '../types';
import './AdminUserAccountActionDialog.css';

interface AdminUserAccountActionDialogProps {
  action: 'block' | 'unblock';
  isPending: boolean;
  reasonText: string;
  user: AdminUser;
  onChangeReason: (value: string) => void;
  onClose: () => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
}

export default function AdminUserAccountActionDialog({
  action,
  isPending,
  reasonText,
  user,
  onChangeReason,
  onClose,
  onSubmit,
}: AdminUserAccountActionDialogProps) {
  const isBlock = action === 'block';
  const title = isBlock ? 'Block user' : 'Unblock user';
  const submitLabel = isBlock ? 'Block' : 'Unblock';
  const buttonTone = isBlock ? 'modal__button--danger' : 'modal__button--success';

  return (
    <Modal
      title={title}
      subtitle={`${user.name} - ${user.email}`}
      onClose={onClose}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" disabled={isPending} onClick={onClose}>
            Cancel
          </button>
          <button type="submit" form="admin-user-account-action-form" className={`modal__button ${buttonTone}`} disabled={isPending}>
            {isPending ? 'Working...' : submitLabel}
          </button>
        </>
      )}
    >
      <form id="admin-user-account-action-form" className="admin-user-account-action__form" onSubmit={onSubmit}>
        <label htmlFor="admin-user-account-action-reason">Reason</label>
        <textarea
          id="admin-user-account-action-reason"
          value={reasonText}
          onChange={(event) => onChangeReason(event.target.value)}
          placeholder={isBlock ? 'Describe why this account is being blocked.' : 'Describe why this account is being unblocked.'}
          disabled={isPending}
          rows={5}
        />
      </form>
    </Modal>
  );
}
