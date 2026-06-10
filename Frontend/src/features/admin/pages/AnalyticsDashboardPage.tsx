import { useEffect, useMemo, useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import type { ReactNode } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import { useCurrency } from '../../../shared/currency/useCurrency';
import {
  adminApi,
  type AdminPaymentResponse,
  type DashboardStatsResponse,
} from '../api/adminApi';
import AdminIssueDetailsDialog from '../components/AdminIssueDetailsDialog';
import AdminIssueResolveDialog from '../components/AdminIssueResolveDialog';
import { toAdminIssue } from '../api/issueMapping';
import type { AdminIssue } from '../types';
import './AnalyticsDashboardPage.css';

// Admin dashboard. Stats row on top, then the Recent Payments and Unresolved Issues panels.
export default function AnalyticsDashboardPage() {
  const { currency } = useCurrency();
  const priceFormatter = useMemo(
    () => new Intl.NumberFormat(getCurrencyLocale(currency), { style: 'currency', currency }),
    [currency],
  );

  const [stats, setStats] = useState<DashboardStatsResponse | null>(null);
  const [payments, setPayments] = useState<AdminPaymentResponse[]>([]);
  const [unresolvedIssues, setUnresolvedIssues] = useState<AdminIssue[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [pendingIssueId, setPendingIssueId] = useState<string | null>(null);
  const [selectedIssueId, setSelectedIssueId] = useState<string | null>(null);
  const [resolvingIssueId, setResolvingIssueId] = useState<string | null>(null);
  const [resolutionText, setResolutionText] = useState('');

  const selectedIssue = selectedIssueId ? unresolvedIssues.find((issue) => issue.id === selectedIssueId) : null;
  const resolvingIssue = resolvingIssueId ? unresolvedIssues.find((issue) => issue.id === resolvingIssueId) : null;

  useEffect(() => {
    const abortController = new AbortController();

    async function load() {
      setError(null);
      try {
        const [statsResponse, paymentsResponse, issuesResponse] = await Promise.all([
          adminApi.getDashboardStats(currency, abortController.signal),
          adminApi.listPayments(currency, 1, 5, { signal: abortController.signal }),
          adminApi.listIssues(1, 5, { unresolvedOnly: true, signal: abortController.signal }),
        ]);
        setStats(statsResponse);
        setPayments(paymentsResponse.items);
        setUnresolvedIssues(issuesResponse.items.map(toAdminIssue));
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }
        setError('Could not load dashboard data.');
      }
    }

    void load();
    return () => abortController.abort();
  }, [currency]);

  async function handleAssign(issueId: string) {
    setError(null);
    setPendingIssueId(issueId);
    try {
      const updated = await adminApi.assignIssue(issueId);
      setUnresolvedIssues((current) => current.map((issue) => (issue.id === issueId ? toAdminIssue(updated) : issue)));
    } catch {
      setError('Could not assign issue.');
    } finally {
      setPendingIssueId(null);
    }
  }

  function openResolveDialog(issueId: string) {
    setSelectedIssueId(null);
    setResolutionText('');
    setResolvingIssueId(issueId);
  }

  function closeResolveDialog() {
    if (pendingIssueId === resolvingIssueId) {
      return;
    }

    setResolvingIssueId(null);
    setResolutionText('');
  }

  async function submitResolution(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!resolvingIssueId) {
      return;
    }

    setError(null);
    setPendingIssueId(resolvingIssueId);
    try {
      await adminApi.resolveIssue(resolvingIssueId, resolutionText.trim() || undefined);
      setUnresolvedIssues((current) => current.filter((issue) => issue.id !== resolvingIssueId));
      setStats((current) => current ? { ...current, unresolvedIssues: Math.max(0, current.unresolvedIssues - 1) } : current);
      setResolvingIssueId(null);
      setResolutionText('');
    } catch {
      setError('Could not resolve issue.');
    } finally {
      setPendingIssueId(null);
    }
  }

  return (
    <PageSkeleton title="Admin Dashboard" summary="Analytics overview, recent payments and unresolved issues at a glance.">
      <div className="admin-dashboard-page">
        <StatsRow stats={stats} priceFormatter={priceFormatter} />

        {error ? <p className="admin-dashboard-page__panel-empty">{error}</p> : null}

        <div className="admin-dashboard-page__columns">
          <RecentPaymentsPanel payments={payments} priceFormatter={priceFormatter} />
          <UnresolvedIssuesPanel issues={unresolvedIssues} onOpenIssue={setSelectedIssueId} />
        </div>

        {selectedIssue ? (
          <AdminIssueDetailsDialog
            issue={selectedIssue}
            isPending={pendingIssueId === selectedIssue.id}
            onAssign={handleAssign}
            onClose={() => setSelectedIssueId(null)}
            onResolve={openResolveDialog}
          />
        ) : null}
        {resolvingIssue ? (
          <AdminIssueResolveDialog
            issue={resolvingIssue}
            isPending={pendingIssueId === resolvingIssue.id}
            resolutionText={resolutionText}
            onChangeResolution={setResolutionText}
            onClose={closeResolveDialog}
            onSubmit={submitResolution}
          />
        ) : null}
      </div>
    </PageSkeleton>
  );
}

function StatsRow({ stats, priceFormatter }: { stats: DashboardStatsResponse | null; priceFormatter: Intl.NumberFormat }) {
  const activeUsers = stats?.activeUsers ?? 0;
  const ordersPerDay = stats?.ordersInLast24Hours ?? 0;
  const revenue = stats?.totalRevenue ?? 0;
  const unresolvedIssues = stats?.unresolvedIssues ?? 0;

  return (
    <div className="admin-dashboard-page__stats">
      <StatCard label="Active Users" value={activeUsers.toLocaleString()} />
      <StatCard label="Orders Last 24h" value={ordersPerDay.toLocaleString()} />
      <StatCard label="Total Revenue" value={priceFormatter.format(revenue)} />
      <StatCard
        label="Unresolved Issues"
        value={unresolvedIssues.toString()}
        meta={unresolvedIssues > 0 ? 'Needs attention' : 'All clear'}
        isWarning={unresolvedIssues > 0}
      />
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

function RecentPaymentsPanel({ payments, priceFormatter }: { payments: AdminPaymentResponse[]; priceFormatter: Intl.NumberFormat }) {
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
            <li key={`${payment.orderId}-${payment.paymentSequential}`} className="admin-dashboard-page__list-item">
              <div className="admin-dashboard-page__list-row">
                <p className="admin-dashboard-page__list-primary">{payment.id}</p>
                <span className={`admin-dashboard-page__badge admin-dashboard-page__badge--${payment.status}`}>
                  {payment.status}
                </span>
              </div>
              <p className="admin-dashboard-page__list-secondary">{payment.customerName}</p>
              <div className="admin-dashboard-page__list-row">
                <p className="admin-dashboard-page__list-meta">{formatDate(payment.date)}</p>
                <span className="admin-dashboard-page__list-amount">{priceFormatter.format(payment.amount)}</span>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

function UnresolvedIssuesPanel({
  issues,
  onOpenIssue,
}: {
  issues: AdminIssue[];
  onOpenIssue: (issueId: string) => void;
}) {
  return (
    <div className="admin-dashboard-page__panel">
      <div className="admin-dashboard-page__panel-header">
        <h2 className="admin-dashboard-page__panel-title">Unresolved Issues</h2>
        <Link to="/admin/issues" className="admin-dashboard-page__panel-link">View All</Link>
      </div>

      {issues.length === 0 ? (
        <p className="admin-dashboard-page__panel-empty">No unresolved issues.</p>
      ) : (
        <ul className="admin-dashboard-page__list">
          {issues.map((issue) => (
            <li key={issue.id} className="admin-dashboard-page__list-item">
              <div className="admin-dashboard-page__list-row">
                <button
                  type="button"
                  className="admin-dashboard-page__list-primary-link"
                  onClick={() => onOpenIssue(issue.id)}
                >
                  {issue.title}
                </button>
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

