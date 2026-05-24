import { useState } from 'react';
import type { ReactNode } from 'react';
import Modal from '../../../shared/components/Modal';
import type { AdminIssue } from '../types';

interface IssuesListProps {
  issues: AdminIssue[];
  isLoading?: boolean;
  pendingId?: string | null;
  pagination?: ReactNode;
  onAssign?: (issueId: string) => void;
  onResolve?: (issueId: string) => void;
}

// Full list of issues on the Report Issue page with status-driven actions.
export default function IssuesList({
  issues,
  isLoading = false,
  pendingId = null,
  pagination = null,
  onAssign,
  onResolve,
}: IssuesListProps) {
  const [selectedIssueId, setSelectedIssueId] = useState<string | null>(null);
  const selectedIssue = selectedIssueId ? issues.find((issue) => issue.id === selectedIssueId) : null;

  return (
    <div className="admin-issues-page__panel">
      <div className="admin-issues-page__panel-header">
        <h2 className="admin-issues-page__panel-title">All Issues</h2>
        <p className="admin-issues-page__panel-subtitle">Visible to all administrators</p>
      </div>

      {isLoading ? (
        <p className="admin-issues-page__empty">Loading issues...</p>
      ) : issues.length === 0 ? (
        <p className="admin-issues-page__empty">No issues reported yet.</p>
      ) : (
        <div className="admin-issues-page__table-wrap">
          <table className="admin-issues-page__table">
            <thead>
              <tr>
                <th>Issue</th>
                <th>Type</th>
                <th>Priority</th>
                <th>Status</th>
                <th>Reported by</th>
                <th>Assignee</th>
                <th>Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {issues.map((issue) => (
                <IssueRow
                  key={issue.id}
                  issue={issue}
                  isPending={pendingId === issue.id}
                  onOpen={setSelectedIssueId}
                  onAssign={onAssign}
                  onResolve={onResolve}
                />
              ))}
            </tbody>
          </table>
        </div>
      )}

      {pagination}
      {selectedIssue ? (
        <IssueDetailsDialog
          issue={selectedIssue}
          isPending={pendingId === selectedIssue.id}
          onAssign={onAssign}
          onClose={() => setSelectedIssueId(null)}
          onResolve={onResolve}
        />
      ) : null}
    </div>
  );
}

interface IssueRowProps {
  issue: AdminIssue;
  isPending: boolean;
  onOpen: (issueId: string) => void;
  onAssign?: (issueId: string) => void;
  onResolve?: (issueId: string) => void;
}

function IssueRow({ issue, isPending, onOpen, onAssign, onResolve }: IssueRowProps) {
  const isOpen = issue.status === 'open';
  const isAssigned = issue.status === 'in progress';

  return (
    <tr>
      <td>
        <div className="admin-issues-page__issue-cell">
          <button
            type="button"
            className="admin-issues-page__issue-title-button"
            onClick={() => onOpen(issue.id)}
          >
            {issue.title}
          </button>
          <p className="admin-issues-page__issue-description">{issue.description}</p>
        </div>
      </td>
      <td>{issue.type}</td>
      <td>
        <span className={`admin-issues-page__badge admin-issues-page__badge--${issue.priority}`}>
          {issue.priority}
        </span>
      </td>
      <td>
        <span className={`admin-issues-page__badge admin-issues-page__badge--${badgeKey(issue.status)}`}>
          {issue.status}
        </span>
      </td>
      <td>{issue.reportedBy}</td>
      <td>
        {issue.assignee ? (
          <span title={issue.assignee}>{shortenId(issue.assignee)}</span>
        ) : (
          <span className="admin-issues-page__muted">Unassigned</span>
        )}
      </td>
      <td>{formatDate(issue.date)}</td>
      <td>
        <div className="admin-issues-page__actions">
          {isOpen ? (
            <button
              type="button"
              className="admin-issues-page__action admin-issues-page__action--assign"
              onClick={() => onAssign?.(issue.id)}
              disabled={isPending}
            >
              {isPending ? 'Working...' : 'Assign me'}
            </button>
          ) : null}
          {isAssigned ? (
            <button
              type="button"
              className="admin-issues-page__action admin-issues-page__action--resolve"
              onClick={() => onResolve?.(issue.id)}
              disabled={isPending}
            >
              {isPending ? 'Working...' : 'Resolve'}
            </button>
          ) : null}
          {!isOpen && !isAssigned ? (
            <span className="admin-issues-page__no-action">No actions</span>
          ) : null}
        </div>
      </td>
    </tr>
  );
}

interface IssueDetailsDialogProps {
  issue: AdminIssue;
  isPending: boolean;
  onAssign?: (issueId: string) => void;
  onClose: () => void;
  onResolve?: (issueId: string) => void;
}

function IssueDetailsDialog({ issue, isPending, onAssign, onClose, onResolve }: IssueDetailsDialogProps) {
  const canAssign = issue.status === 'open';
  const canResolve = issue.status === 'in progress';

  return (
    <Modal
      title={issue.title}
      subtitle={<span className="admin-issues-page__modal-subtitle">{issue.type}</span>}
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
      <div className="admin-issues-page__modal-badges">
          <span className={`admin-issues-page__badge admin-issues-page__badge--${issue.priority}`}>
            {issue.priority}
          </span>
          <span className={`admin-issues-page__badge admin-issues-page__badge--${badgeKey(issue.status)}`}>
            {issue.status}
          </span>
      </div>

      <p className="admin-issues-page__modal-description">{issue.description}</p>

      <dl className="admin-issues-page__modal-details">
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
        <div className="admin-issues-page__modal-resolution">
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

function shortenId(id: string): string {
  return id.length <= 8 ? id : id.slice(0, 8);
}
