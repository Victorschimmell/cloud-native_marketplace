import { useCallback, useEffect, useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import IssueForm from '../components/IssueForm';
import IssuesList from '../components/IssuesList';
import { adminApi } from '../api/adminApi';
import { toAdminIssue, toCreateIssuePayload } from '../api/issueMapping';
import type { AdminIssue } from '../data/placeholderData';
import './AdminReportIssuePage.css';

// Report Issue page. Reached from the Open Issues "View All" link on the dashboard.
export default function AdminReportIssuePage() {
  const [issues, setIssues] = useState<AdminIssue[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingId, setPendingId] = useState<string | null>(null);

  const loadIssues = useCallback(async (signal?: AbortSignal) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await adminApi.listIssues(1, 100, { signal });
      setIssues(response.items.map(toAdminIssue));
    } catch (requestError) {
      if (requestError instanceof DOMException && requestError.name === 'AbortError') {
        return;
      }
      setError(`Could not load issues: ${getErrorMessage(requestError)}`);
    } finally {
      if (!signal?.aborted) {
        setIsLoading(false);
      }
    }
  }, []);

  useEffect(() => {
    const abortController = new AbortController();
    void loadIssues(abortController.signal);
    return () => abortController.abort();
  }, [loadIssues]);

  async function handleSubmit(newIssue: Omit<AdminIssue, 'id' | 'status' | 'reportedBy' | 'date'>) {
    setError(null);
    try {
      const created = await adminApi.createIssue(toCreateIssuePayload(newIssue));
      setIssues((current) => [toAdminIssue(created), ...current]);
    } catch (requestError) {
      setError(`Could not create issue: ${getErrorMessage(requestError)}`);
    }
  }

  async function handleResolve(issueId: string) {
    setError(null);
    setPendingId(issueId);
    try {
      const resolution = window.prompt('Resolution notes (optional):') ?? undefined;
      const updated = await adminApi.resolveIssue(issueId, resolution || undefined);
      setIssues((current) => current.map((issue) => (issue.id === issueId ? toAdminIssue(updated) : issue)));
    } catch (requestError) {
      setError(`Could not resolve issue: ${getErrorMessage(requestError)}`);
    } finally {
      setPendingId(null);
    }
  }

  async function handleAssign(issueId: string) {
    setError(null);
    setPendingId(issueId);
    try {
      const updated = await adminApi.assignIssue(issueId);
      setIssues((current) => current.map((issue) => (issue.id === issueId ? toAdminIssue(updated) : issue)));
    } catch (requestError) {
      setError(`Could not assign issue: ${getErrorMessage(requestError)}`);
    } finally {
      setPendingId(null);
    }
  }

  async function handleView(issueId: string) {
    setError(null);
    try {
      const issue = await adminApi.getIssue(issueId);
      const lines = [
        `Title: ${issue.title}`,
        `Status: ${issue.status}`,
        `Priority: ${issue.priority}`,
        `Type: ${issue.type}`,
        `Reported by: ${issue.reportedByDisplay ?? issue.reportedByUserId}`,
        `Created: ${new Date(issue.createdAtUtc).toLocaleString('en-GB')}`,
        issue.assignedToUserId ? `Assigned to: ${issue.assignedToUserId}` : null,
        issue.resolvedAtUtc ? `Resolved: ${new Date(issue.resolvedAtUtc).toLocaleString('en-GB')}` : null,
        issue.resolution ? `Resolution: ${issue.resolution}` : null,
        '',
        issue.description,
      ].filter(Boolean);
      window.alert(lines.join('\n'));
    } catch (requestError) {
      setError(`Could not load issue: ${getErrorMessage(requestError)}`);
    }
  }

  return (
    <PageSkeleton title="Report Issue" summary="File a new issue or browse the ones already reported.">
      <div className="admin-issues-page">
        <IssueForm onSubmit={handleSubmit} />
        <div>
          {error ? <p className="admin-issues-page__empty">{error}</p> : null}
          <IssuesList
            issues={issues}
            isLoading={isLoading}
            pendingId={pendingId}
            onView={handleView}
            onAssign={handleAssign}
            onResolve={handleResolve}
          />
        </div>
      </div>
    </PageSkeleton>
  );
}

function getErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const payload = err.payload as Record<string, unknown> | null;
    if (payload && typeof payload === 'object' && 'error' in payload) {
      return String(payload.error);
    }
    return err.message;
  }
  if (err instanceof Error) {
    return err.message;
  }
  return 'Unknown error';
}
