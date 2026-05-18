import { useMemo } from 'react';
import { Link } from 'react-router-dom';
import type { ReactNode } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import { useCurrency } from '../../../shared/currency/useCurrency';
import {
  placeholderIssues,
  placeholderPayments,
  placeholderStats,
  type AdminIssue,
  type RecentPayment,
} from '../data/placeholderData';
import './AnalyticsDashboardPage.css';

// Admin dashboard. Stats row on top, then the Recent Payments and Open Issues panels.
// Placeholder data for now - swap for API calls once the back-end story is ready.
export default function AnalyticsDashboardPage() {
  const { currency } = useCurrency();
  const priceFormatter = useMemo(
    () => new Intl.NumberFormat(getCurrencyLocale(currency), { style: 'currency', currency }),
    [currency],
  );

  return (
    <PageSkeleton title="Admin Dashboard" summary="Analytics overview, recent payments and open issues at a glance.">
      <div className="admin-dashboard-page">
        <StatsRow priceFormatter={priceFormatter} />

        <div className="admin-dashboard-page__columns">
          <RecentPaymentsPanel payments={placeholderPayments} />
          <OpenIssuesPanel issues={placeholderIssues} />
        </div>
      </div>
    </PageSkeleton>
  );
}

function StatsRow({ priceFormatter }: { priceFormatter: Intl.NumberFormat }) {
  const stats = placeholderStats;

  return (
    <div className="admin-dashboard-page__stats">
      <StatCard label="Active Users" value={stats.activeUsers.toLocaleString()} meta={`+${stats.activeUsersChange}%`} />
      <StatCard label="Requests/Day" value={stats.requestsPerDay.toLocaleString()} meta={`+${stats.requestsPerDayChange}%`} />
      <StatCard label="Total Revenue" value={priceFormatter.format(stats.totalRevenue)} meta={`+${stats.totalRevenueChange}%`} />
      <StatCard label="Open Issues" value={stats.openIssues.toString()} meta="Needs attention" isWarning />
    </div>
  );
}

function StatCard({ label, value, meta, isWarning = false }: { label: string; value: ReactNode; meta?: string; isWarning?: boolean; }) {
  const metaClassName = `admin-dashboard-page__stat-meta${isWarning ? ' admin-dashboard-page__stat-meta--warning' : ''}`;

  return (
    <div className="admin-dashboard-page__stat-card">
      <p className="admin-dashboard-page__stat-label">{label}</p>
      <p className="admin-dashboard-page__stat-value">{value}</p>
      {meta ? <p className={metaClassName}>{meta}</p> : null}
    </div>
  );
}

function RecentPaymentsPanel({ payments }: { payments: RecentPayment[] }) {
  return (
    <div className="admin-dashboard-page__panel">
      <div className="admin-dashboard-page__panel-header">
        <h2 className="admin-dashboard-page__panel-title">Recent Payments</h2>
        <Link to="/admin/payments" className="admin-dashboard-page__panel-link">View All</Link>
      </div>

      {payments.length === 0 ? (
        <p className="admin-dashboard-page__panel-empty">No recent payments.</p>
      ) : (
        <ul className="admin-dashboard-page__list">
          {payments.map((payment) => (
            <li key={payment.id} className="admin-dashboard-page__list-item">
              <div className="admin-dashboard-page__list-row">
                <p className="admin-dashboard-page__list-primary">{payment.id}</p>
                <span className={`admin-dashboard-page__badge admin-dashboard-page__badge--${payment.status}`}>
                  {payment.status}
                </span>
              </div>
              <p className="admin-dashboard-page__list-secondary">{payment.customerName}</p>
              <div className="admin-dashboard-page__list-row">
                <p className="admin-dashboard-page__list-meta">{formatDate(payment.date)}</p>
                <span className="admin-dashboard-page__list-amount">{payment.amount.toFixed(2)}</span>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

function OpenIssuesPanel({ issues }: { issues: AdminIssue[] }) {
  const openIssues = issues.filter((issue) => issue.status !== 'resolved');

  return (
    <div className="admin-dashboard-page__panel">
      <div className="admin-dashboard-page__panel-header">
        <h2 className="admin-dashboard-page__panel-title">Open Issues</h2>
        <Link to="/admin/issues" className="admin-dashboard-page__panel-link">View All</Link>
      </div>

      {openIssues.length === 0 ? (
        <p className="admin-dashboard-page__panel-empty">No open issues.</p>
      ) : (
        <ul className="admin-dashboard-page__list">
          {openIssues.map((issue) => (
            <li key={issue.id} className="admin-dashboard-page__list-item">
              <div className="admin-dashboard-page__list-row">
                <p className="admin-dashboard-page__list-primary">{issue.title}</p>
                <span className={`admin-dashboard-page__badge admin-dashboard-page__badge--${issue.priority}`}>
                  {issue.priority}
                </span>
              </div>
              <p className="admin-dashboard-page__list-secondary">{issue.description}</p>
              <p className="admin-dashboard-page__list-meta">{formatDate(issue.date)} - {issue.type}</p>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('en-GB');
}
