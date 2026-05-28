import Modal from '../../../shared/components/Modal';
import type { AdminIssue } from '../types';
import './AdminIssueDetailsDialog.css';

interface AdminIssueDetailsDialogProps {
  issue: AdminIssue;
  isPending?: boolean;
  onAssign?: (issueId: string) => void;
  onClose: () => void;
  onResolve?: (issueId: string) => void;
}

export default function AdminIssueDetailsDialog({
  issue,
  isPending = false,
  onAssign,
  onClose,
  onResolve,
}: AdminIssueDetailsDialogProps) {
  const canAssign = Boolean(onAssign) && issue.status === 'open';
  const canResolve = Boolean(onResolve) && issue.status === 'in progress';

  return (
    <Modal
      title={issue.title}
      subtitle={<span className="admin-issue-details__subtitle">{issue.type}</span>}
      onClose={onClose}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" onClick={onClose}>
            Close
          </button>
          {canAssign ? (
            <button
              type="button"
              className="modal__button modal__button--primary"
              disabled={isPending}
              onClick={() => onAssign?.(issue.id)}
            >
              {isPending ? 'Working...' : 'Assign me'}
            </button>
          ) : null}
          {canResolve ? (
            <button
              type="button"
              className="modal__button modal__button--success"
              disabled={isPending}
              onClick={() => {
                onClose();
                onResolve?.(issue.id);
              }}
            >
              {isPending ? 'Working...' : 'Resolve'}
            </button>
          ) : null}
        </>
      )}
    >
      <div className="admin-issue-details__badges">
        <span className={`admin-issue-details__badge admin-issue-details__badge--${issue.priority}`}>
          {issue.priority}
        </span>
        <span className={`admin-issue-details__badge admin-issue-details__badge--${badgeKey(issue.status)}`}>
          {issue.status}
        </span>
      </div>

      <p className="admin-issue-details__description">{issue.description}</p>

      <dl className="admin-issue-details__grid">
        <div>
          <dt>Reported by</dt>
          <dd>{issue.reportedBy}</dd>
        </div>
        <div>
          <dt>Assignee</dt>
          <dd>{issue.assignee ?? 'Unassigned'}</dd>
        </div>
        {issue.resolvedBy ? (
          <div>
            <dt>Resolved by</dt>
            <dd>{issue.resolvedBy}</dd>
          </div>
        ) : null}
        {issue.resolvedAt ? (
          <div>
            <dt>Resolved on</dt>
            <dd>{formatDate(issue.resolvedAt)}</dd>
          </div>
        ) : null}
        <div>
          <dt>Date</dt>
          <dd>{formatDate(issue.date)}</dd>
        </div>
        <div>
          <dt>Issue ID</dt>
          <dd>{issue.id}</dd>
        </div>
      </dl>

      {issue.resolution ? (
        <div className="admin-issue-details__resolution">
          <h4>Resolution</h4>
          <p>{issue.resolution}</p>
        </div>
      ) : null}
    </Modal>
  );
}

function badgeKey(status: string): string {
  return status.replace(/\s+/g, '-');
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-GB');
}
