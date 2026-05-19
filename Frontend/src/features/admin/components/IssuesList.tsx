import type { AdminIssue } from '../data/placeholderData';

interface IssuesListProps {
  issues: AdminIssue[];
  isLoading?: boolean;
  pendingId?: string | null;
  onView?: (issueId: string) => void;
  onAssign?: (issueId: string) => void;
  onResolve?: (issueId: string) => void;
}

// Full list of issues on the Report Issue page with View/Assign/Resolve buttons.
export default function IssuesList({
  issues,
  isLoading = false,
  pendingId = null,
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
        <ul className="admin-issues-page__list">
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
        </ul>
      )}
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
    <li className="admin-issues-page__list-item">
      <div className="admin-issues-page__list-row">
        <p className="admin-issues-page__list-primary">{issue.title}</p>
        <div className="admin-issues-page__actions">
          <span className={`admin-issues-page__badge admin-issues-page__badge--${issue.priority}`}>
            {issue.priority}
          </span>
          <span className={`admin-issues-page__badge admin-issues-page__badge--${badgeKey(issue.status)}`}>
            {issue.status}
          </span>
        </div>
      </div>

      <p className="admin-issues-page__list-secondary">{issue.description}</p>
      <p className="admin-issues-page__list-meta">
        {issue.type} - {issue.reportedBy} - {formatDate(issue.date)}
      </p>

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
    </li>
  );
}

function badgeKey(status: string): string {
  return status.replace(/\s+/g, '-');
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-GB');
}
