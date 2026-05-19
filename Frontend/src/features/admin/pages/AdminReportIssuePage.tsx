import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import Pagination from '../../../shared/components/Pagination';
import IssuesList from '../components/IssuesList';
import { adminApi } from '../api/adminApi';
import { toAdminIssue } from '../api/issueMapping';
import type { AdminIssue } from '../data/placeholderData';
import './AdminReportIssuePage.css';

const issuesPageSize = 10;

// Report Issue page. Reached from the Open Issues "View All" link on the dashboard.
export default function AdminReportIssuePage() {
  const [issues, setIssues] = useState<AdminIssue[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingId, setPendingId] = useState<string | null>(null);

  const totalPages = useMemo(() => Math.max(1, Math.ceil(totalCount / issuesPageSize)), [totalCount]);

  const loadIssues = useCallback(async (targetPage: number, signal?: AbortSignal) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await adminApi.listIssues(targetPage, issuesPageSize, { signal });
      setIssues(response.items.map(toAdminIssue));
      setTotalCount(response.totalCount);
    } catch (requestError) {
      if (requestError instanceof DOMException && requestError.name === 'AbortError') {
        return;
      }
      setError(`Could not load issues: ${getErrorMessage(requestError)}`);
      setIssues([]);
      setTotalCount(0);
    } finally {
      if (!signal?.aborted) {
        setIsLoading(false);
      }
    }
  }, []);

  useEffect(() => {
    const abortController = new AbortController();
    void loadIssues(page, abortController.signal);
    return () => abortController.abort();
  }, [loadIssues, page]);

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

  function goToPreviousPage() {
    setPage((currentPage) => Math.max(1, currentPage - 1));
  }

  function goToNextPage() {
    setPage((currentPage) => Math.min(totalPages, currentPage + 1));
  }

  return (
    <PageSkeleton title="Issues" summary="Browse, assign and resolve administrator issues.">
      <div className="admin-issues-page">
        <div className="admin-issues-page__toolbar">
          <Link to="/admin/dashboard" className="admin-issues-page__back">
            Back to Dashboard
          </Link>
          <Link to="/admin/issues/new" className="admin-issues-page__primary-action">
            Create New Issue
          </Link>
        </div>

        {error ? <p className="admin-issues-page__empty admin-issues-page__empty--error">{error}</p> : null}
        <IssuesList
          issues={issues}
          isLoading={isLoading}
          pagination={!error ? (
            <Pagination
              currentPage={page}
              disabled={isLoading}
              label="Issues pagination"
              onNext={goToNextPage}
              onPrevious={goToPreviousPage}
              totalPages={totalPages}
            />
          ) : null}
          pendingId={pendingId}
          onAssign={handleAssign}
          onResolve={handleResolve}
        />
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
