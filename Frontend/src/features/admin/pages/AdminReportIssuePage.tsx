import { useCallback, useEffect, useMemo, useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import Modal from '../../../shared/components/Modal';
import Pagination from '../../../shared/components/Pagination';
import IssuesList from '../components/IssuesList';
import { adminApi } from '../api/adminApi';
import { toAdminIssue } from '../api/issueMapping';
import type { AdminIssue } from '../types';
import './AdminReportIssuePage.css';

const issuesPageSize = 10;

// Issues page. Reached from the Unresolved Issues "View All" link on the dashboard.
export default function AdminReportIssuePage() {
  const [issues, setIssues] = useState<AdminIssue[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingId, setPendingId] = useState<string | null>(null);
  const [resolvingIssueId, setResolvingIssueId] = useState<string | null>(null);
  const [resolutionText, setResolutionText] = useState('');

  const totalPages = useMemo(() => Math.max(1, Math.ceil(totalCount / issuesPageSize)), [totalCount]);
  const resolvingIssue = resolvingIssueId ? issues.find((issue) => issue.id === resolvingIssueId) : null;

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

  function openResolveDialog(issueId: string) {
    setError(null);
    setResolutionText('');
    setResolvingIssueId(issueId);
  }

  function closeResolveDialog() {
    if (pendingId) {
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
    setPendingId(resolvingIssueId);
    try {
      const updated = await adminApi.resolveIssue(resolvingIssueId, resolutionText.trim() || undefined);
      setIssues((current) => current.map((issue) => (issue.id === resolvingIssueId ? toAdminIssue(updated) : issue)));
      setResolvingIssueId(null);
      setResolutionText('');
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
          onResolve={openResolveDialog}
        />
        {resolvingIssue ? (
          <ResolveIssueDialog
            issue={resolvingIssue}
            isPending={pendingId === resolvingIssue.id}
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

interface ResolveIssueDialogProps {
  issue: AdminIssue;
  isPending: boolean;
  resolutionText: string;
  onChangeResolution: (value: string) => void;
  onClose: () => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
}

function ResolveIssueDialog({
  issue,
  isPending,
  resolutionText,
  onChangeResolution,
  onClose,
  onSubmit,
}: ResolveIssueDialogProps) {
  return (
    <Modal
      title="Resolve issue"
      subtitle={issue.title}
      onClose={onClose}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" disabled={isPending} onClick={onClose}>
            Cancel
          </button>
          <button type="submit" form="admin-issue-resolution-form" className="modal__button modal__button--success" disabled={isPending}>
            {isPending ? 'Working...' : 'Resolve'}
          </button>
        </>
      )}
    >
      <form id="admin-issue-resolution-form" className="admin-issues-page__resolution-form" onSubmit={onSubmit}>
        <label htmlFor="admin-issue-resolution">Resolution description</label>
        <textarea
          id="admin-issue-resolution"
          value={resolutionText}
          onChange={(event) => onChangeResolution(event.target.value)}
          placeholder="Describe what was done or why this issue can be closed."
          disabled={isPending}
          rows={5}
        />
      </form>
    </Modal>
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
