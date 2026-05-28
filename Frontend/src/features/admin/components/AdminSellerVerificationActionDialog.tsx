import type { FormEvent } from 'react';
import Modal from '../../../shared/components/Modal';
import type { AdminUser } from '../types';
import './AdminSellerVerificationActionDialog.css';

interface AdminSellerVerificationActionDialogProps {
  action: 'approve' | 'reject';
  isPending: boolean;
  text: string;
  user: AdminUser;
  onChangeText: (value: string) => void;
  onClose: () => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
}

export default function AdminSellerVerificationActionDialog({
  action,
  isPending,
  text,
  user,
  onChangeText,
  onClose,
  onSubmit,
}: AdminSellerVerificationActionDialogProps) {
  const isApprove = action === 'approve';
  const title = isApprove ? 'Approve seller' : 'Reject seller';
  const submitLabel = isApprove ? 'Approve' : 'Reject';
  const buttonTone = isApprove ? 'modal__button--success' : 'modal__button--danger';
  const fieldLabel = isApprove ? 'Review notes' : 'Rejection reason';

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
          <button type="submit" form="admin-seller-verification-action-form" className={`modal__button ${buttonTone}`} disabled={isPending}>
            {isPending ? 'Working...' : submitLabel}
          </button>
        </>
      )}
    >
      <form id="admin-seller-verification-action-form" className="admin-seller-verification-action__form" onSubmit={onSubmit}>
        <label htmlFor="admin-seller-verification-action-text">{fieldLabel}</label>
        <textarea
          id="admin-seller-verification-action-text"
          value={text}
          onChange={(event) => onChangeText(event.target.value)}
          placeholder={isApprove ? 'Add optional notes for this approval.' : 'Describe why this seller is being rejected.'}
          disabled={isPending}
          required={!isApprove}
          rows={5}
        />
      </form>
    </Modal>
  );
}
