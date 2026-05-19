import type { ReactNode } from 'react';
import type { AdminIssue } from '../data/placeholderData';

interface IssuesListProps {
  issues: AdminIssue[];
  isLoading?: boolean;
  pendingId?: string | null;
  pagination?: ReactNode;
  onView?: (issueId: string) => void;
  onAssign?: (issueId: string) => void;
  onResolve?: (issueId: string) => void;
}

// Full list of issues on the Report Issue page with View/Assign/Resolve buttons.
export default function IssuesList({
  issues,
  isLoading = false,
  pendingId = null,
  pagination = null,
  onView,
  onAssign,
  onResolve,
}: IssuesListProps) {
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
        <table className="admin-issues-page__table">
          <thead>
            <tr>
              <th>Issue</th>
              <th>Type</th>
              <th>Priority</th>
              <th>Status</th>
              <th>Reported by</th>
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
                onView={onView}
                onAssign={onAssign}
                onResolve={onResolve}
              />
            ))}
          </tbody>
        </table>
      )}

      {pagination}
    </div>
  );
}

interface IssueRowProps {
  issue: AdminIssue;
  isPending: boolean;
  onView?: (issueId: string) => void;
  onAssign?: (issueId: string) => void;
  onResolve?: (issueId: string) => void;
}

function IssueRow({ issue, isPending, onView, onAssign, onResolve }: IssueRowProps) {
  const isResolved = issue.status === 'resolved';

  return (
    <tr>
      <td>
        <div className="admin-issues-page__issue-cell">
          <p className="admin-issues-page__issue-title">{issue.title}</p>
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
      <td>{formatDate(issue.date)}</td>
      <td>
        <div className="admin-issues-page__actions">
          <button
            type="button"
            className="admin-issues-page__action admin-issues-page__action--view"
            onClick={() => onView?.(issue.id)}
            disabled={isPending}
          >
            View
          </button>
          {!isResolved && (
            <>
              <button
                type="button"
                className="admin-issues-page__action admin-issues-page__action--assign"
                onClick={() => onAssign?.(issue.id)}
                disabled={isPending}
              >
                Assign
              </button>
              <button
                type="button"
                className="admin-issues-page__action admin-issues-page__action--resolve"
                onClick={() => onResolve?.(issue.id)}
                disabled={isPending}
              >
                {isPending ? 'Working...' : 'Resolve'}
              </button>
            </>
          )}
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
