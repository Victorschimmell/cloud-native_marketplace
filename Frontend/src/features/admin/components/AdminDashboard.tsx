import { Link } from 'react-router-dom';
import type { ReactNode } from 'react';
import AdminHeader, { ReportIssueLink } from './AdminHeader';
import RecentPaymentsPanel from './RecentPaymentsPanel';
import OpenIssuesPanel from './OpenIssuesPanel';
import {
  placeholderIssues,
  placeholderPayments,
  placeholderStats,
} from '../data/placeholderData';
import './AdminDashboard.css';

// Important : Reads from placeholder arrays for now - must be replaced with API calls later.
export default function AdminDashboard() {
  return (
    <section className="admin-dashboard">
      <AdminHeader
        title="Admin Dashboard"
        actions={
          <>
            <ReportIssueLink />
            <Link to="/admin/users" className="admin-dashboard__header-button">
              User Management
            </Link>
          </>
        }
      />

      <h2 className="admin-dashboard__section-title">Analytics Overview</h2>
      <StatsRow />

      <div className="admin-dashboard__columns">
        <RecentPaymentsPanel payments={placeholderPayments} />
        <OpenIssuesPanel issues={placeholderIssues} />
      </div>
    </section>
  );
}

// Stats related code

function StatsRow() {
  const stats = placeholderStats;

  return (
    <div className="admin-dashboard__stats">
      <StatCard
        label="Active Users"
        value={stats.activeUsers.toLocaleString()}
        meta={`↑ ${stats.activeUsersChange}%`}
      />
      <StatCard
        label="Requests/Day"
        value={stats.requestsPerDay.toLocaleString()}
        meta={`↑ ${stats.requestsPerDayChange}%`}
      />
      <StatCard
        label="Total Revenue"
        value={`$${stats.totalRevenue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`}
        meta={`↑ ${stats.totalRevenueChange}%`}
      />
      <StatCard
        label="Open Issues"
        value={stats.openIssues.toString()}
        meta="Needs attention"
        isWarning
      />
    </div>
  );
}

function StatCard({
  label,
  value,
  meta,
  isWarning = false,
}: {
  label: string;
  value: ReactNode;
  meta?: string;
  isWarning?: boolean;
}) {
  const metaClassName = `admin-dashboard__stat-meta${isWarning ? ' admin-dashboard__stat-meta--warning' : ''}`;

  return (
    <div className="admin-dashboard__stat-card">
      <p className="admin-dashboard__stat-label">{label}</p>
      <p className="admin-dashboard__stat-value">{value}</p>
      {meta ? <p className={metaClassName}>{meta}</p> : null}
    </div>
  );
}
