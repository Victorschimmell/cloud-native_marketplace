import type { AdminIssue } from '../data/placeholderData';

interface IssuesListProps {
  issues: AdminIssue[];
}

// Full list of issues on the Report Issue page with View/Assign/Resolve buttons.
export default function IssuesList({ issues }: IssuesListProps) {
  return (
    <div className="admin-issues-page__panel">
      <div className="admin-issues-page__panel-header">
        <h2 className="admin-issues-page__panel-title">All Issues</h2>
        <p className="admin-issues-page__panel-subtitle">Visible to all administrators</p>
      </div>

      {issues.length === 0 ? (
        <p className="admin-issues-page__empty">No issues reported yet.</p>
      ) : (
        <ul className="admin-issues-page__list">
          {issues.map((issue) => (
            <IssueRow key={issue.id} issue={issue} />
          ))}
        </ul>
      )}
    </div>
  );
}

function IssueRow({ issue }: { issue: AdminIssue }) {
  // Logging stubs for now - real API calls go here later.
  function handleView() {
    console.log('View issue', issue.id);
  }

  function handleAssign() {
    console.log('Assign issue', issue.id);
  }

  function handleResolve() {
    console.log('Resolve issue', issue.id);
  }

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
          onClick={handleView}
        >
          View
        </button>
        {!isResolved && (
          <>
            <button
              type="button"
              className="admin-issues-page__action admin-issues-page__action--assign"
              onClick={handleAssign}
            >
              Assign
            </button>
            <button
              type="button"
              className="admin-issues-page__action admin-issues-page__action--resolve"
              onClick={handleResolve}
            >
              Resolve
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
