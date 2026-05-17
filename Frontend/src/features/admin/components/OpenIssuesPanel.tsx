import { Link } from 'react-router-dom';
import type { AdminIssue } from '../data/placeholderData';

interface OpenIssuesPanelProps {
  issues: AdminIssue[];
}

// Open Issues panel on the admin dashboard
export default function OpenIssuesPanel({ issues }: OpenIssuesPanelProps) {
  const openIssues = issues.filter((issue) => issue.status !== 'resolved');

  return (
    <div className="admin-dashboard__panel">
      <div className="admin-dashboard__panel-header">
        <h2 className="admin-dashboard__panel-title">Open Issues</h2>
        <Link to="/admin/issues" className="admin-dashboard__panel-link">
          View All
        </Link>
      </div>

      {openIssues.length === 0 ? (
        <p className="admin-dashboard__panel-empty">No open issues.</p>
      ) : (
        <ul className="admin-dashboard__list">
          {openIssues.map((issue) => (
            <IssueRow key={issue.id} issue={issue} />
          ))}
        </ul>
      )}
    </div>
  );
}

function IssueRow({ issue }: { issue: AdminIssue }) {
  return (
    <li className="admin-dashboard__list-item">
      <div className="admin-dashboard__list-row">
        <p className="admin-dashboard__list-primary">{issue.title}</p>
        <span className={`admin-dashboard__badge admin-dashboard__badge--${issue.priority}`}>
          {issue.priority}
        </span>
      </div>
      <p className="admin-dashboard__list-secondary">{issue.description}</p>
      <p className="admin-dashboard__list-meta">
        {formatDate(issue.date)} • {issue.type}
      </p>
    </li>
  );
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-GB');
}
