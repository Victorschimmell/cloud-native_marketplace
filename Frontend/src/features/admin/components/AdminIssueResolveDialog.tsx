import type { FormEvent } from 'react';
import Modal from '../../../shared/components/Modal';
import type { AdminIssue } from '../types';
import './AdminIssueResolveDialog.css';

interface AdminIssueResolveDialogProps {
  issue: AdminIssue;
  isPending: boolean;
  resolutionText: string;
  onChangeResolution: (value: string) => void;
  onClose: () => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
}

export default function AdminIssueResolveDialog({
  issue,
  isPending,
  resolutionText,
  onChangeResolution,
  onClose,
  onSubmit,
}: AdminIssueResolveDialogProps) {
  return (
    <Modal
      title="Resolve issue"
      subtitle={issue.title}
      onClose={onClose}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" disabled={isPending} onClick={onClose}>
            Cancel
          </button>
          <button type="submit" form="admin-issue-resolution-form" className="modal__button modal__button--success" disabled={isPending}>
            {isPending ? 'Working...' : 'Resolve'}
          </button>
        </>
      )}
    >
      <form id="admin-issue-resolution-form" className="admin-issue-resolve__form" onSubmit={onSubmit}>
        <label htmlFor="admin-issue-resolution">Resolution description</label>
        <textarea
          id="admin-issue-resolution"
          value={resolutionText}
          onChange={(event) => onChangeResolution(event.target.value)}
          placeholder="Describe what was done or why this issue can be closed."
          disabled={isPending}
          rows={5}
        />
      </form>
    </Modal>
  );
}
