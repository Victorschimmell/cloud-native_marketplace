import type { AdminIssue } from '../data/placeholderData';

interface IssuesListProps {
  issues: AdminIssue[];
}

// Full list of issues on the Report Issue page with View/Assign/Resolve buttons.
export default function IssuesList({ issues }: IssuesListProps) {
  return (
    <div className="admin-dashboard__panel">
      <div className="admin-dashboard__panel-header">
        <div>
          <h2 className="admin-dashboard__panel-title">All Issues</h2>
          <p className="admin-dashboard__panel-subtitle">Visible to all administrators</p>
        </div>
      </div>

      {issues.length === 0 ? (
        <p className="admin-dashboard__panel-empty">No issues reported yet.</p>
      ) : (
        <ul className="admin-dashboard__list">
          {issues.map((issue) => (
            <IssueRow key={issue.id} issue={issue} />
          ))}
        </ul>
      )}
    </div>
  );
}

function IssueRow({ issue }: { issue: AdminIssue }) {
  // Logging stubs for now - must add real API calls here later.
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
    <li className="admin-dashboard__list-item">
      <div className="admin-dashboard__list-row">
        <p className="admin-dashboard__list-primary">{issue.title}</p>
        <div className="admin-dashboard__issue-actions">
          <span className={`admin-dashboard__badge admin-dashboard__badge--${issue.priority}`}>
            {issue.priority}
          </span>
          <span
            className={`admin-dashboard__badge admin-dashboard__badge--${badgeKey(issue.status)}`}
          >
            {issue.status}
          </span>
        </div>
      </div>

      <p className="admin-dashboard__list-secondary">{issue.description}</p>
      <p className="admin-dashboard__list-meta">
        {issue.type} • {issue.reportedBy} • {formatDate(issue.date)}
      </p>

      <div className="admin-dashboard__issue-actions">
        <button
          type="button"
          className="admin-dashboard__action admin-dashboard__action--view"
          onClick={handleView}
        >
          View
        </button>
        {!isResolved && (
          <>
            <button
              type="button"
              className="admin-dashboard__action admin-dashboard__action--assign"
              onClick={handleAssign}
            >
              Assign
            </button>
            <button
              type="button"
              className="admin-dashboard__action admin-dashboard__action--resolve"
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
