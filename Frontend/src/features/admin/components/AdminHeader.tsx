import { Link } from 'react-router-dom';
import type { ReactNode } from 'react';

interface AdminHeaderProps {
  title: string;
  actions?: ReactNode;
}

// Adds Red banner used at the top of every admin page.
export default function AdminHeader({ title, actions }: AdminHeaderProps) {
  return (
    <header className="admin-dashboard__header">
      <h1 className="admin-dashboard__title">
        <ShieldIcon />
        {title}
      </h1>
      {actions ? <div className="admin-dashboard__header-actions">{actions}</div> : null}
    </header>
  );
}

// "Report Issue" link with an alert icon in Admin page
export function ReportIssueLink({ to = '/admin/issues' }: { to?: string }) {
  return (
    <Link to={to} className="admin-dashboard__header-link">
      <AlertIcon />
      Report Issue
    </Link>
  );
}

function ShieldIcon() {
  return (
    <svg
      className="admin-dashboard__title-icon"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M12 2l8 4v6c0 5-3.5 9-8 10-4.5-1-8-5-8-10V6l8-4z" />
    </svg>
  );
}

function AlertIcon() {
  return (
    <svg
      width="18"
      height="18"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <circle cx="12" cy="12" r="10" />
      <line x1="12" y1="8" x2="12" y2="12" />
      <line x1="12" y1="16" x2="12.01" y2="16" />
    </svg>
  );
}
