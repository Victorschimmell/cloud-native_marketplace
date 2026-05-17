import { useEffect, useMemo, useState } from 'react';
// import { Link } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import Pagination from '../../../shared/components/Pagination';
import StatusMessage from '../../../shared/components/StatusMessage';
import { auditLogApi } from '../api/auditLogApi';
import AuditLogFilters from '../components/AuditLogFilters';
import AuditLogTable from '../components/AuditLogTable';
import type { AuditLogEntry, AuditLogFilterState } from '../types';
import './AdminAuditPage.css';

const pageSizeOptions = [10, 20, 50, 100] as const;
const emptyFilters: AuditLogFilterState = {
  actorUserId: '',
  entityType: '',
  entityId: '',
};

export default function AdminAuditPage() {
  const [entries, setEntries] = useState<AuditLogEntry[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [draftFilters, setDraftFilters] = useState<AuditLogFilterState>(emptyFilters);
  const [appliedFilters, setAppliedFilters] = useState<AuditLogFilterState>(emptyFilters);

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  const summary = isLoading
    ? 'Loading audit logs...'
    : `${totalCount} audit log${totalCount === 1 ? '' : 's'} found`;

  const trimmedFilters = useMemo(() => {
    const actorUserId = appliedFilters.actorUserId.trim();
    const entityType = appliedFilters.entityType.trim();
    const entityId = appliedFilters.entityId.trim();

    return {
      actorUserId: actorUserId || undefined,
      entityType: entityType || undefined,
      entityId: entityType && entityId ? entityId : undefined,
    };
  }, [appliedFilters]);

  const hasActiveFilters = Object.values(draftFilters).some((value) => value.trim().length > 0);
  const isEntityIdDisabled = draftFilters.entityType.trim().length === 0;

  useEffect(() => {
    const abortController = new AbortController();

    async function loadAuditLogs() {
      try {
        setIsLoading(true);
        setError(null);

        const response = await auditLogApi.getAuditLogs(page, pageSize, {
          actorUserId: trimmedFilters.actorUserId,
          entityType: trimmedFilters.entityType,
          entityId: trimmedFilters.entityId,
          signal: abortController.signal,
        });

        setEntries(response.items);
        setTotalCount(response.totalCount);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }

        setError(`Audit logs could not be loaded: ${getErrorMessage(requestError)}`);
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadAuditLogs();

    return () => {
      abortController.abort();
    };
  }, [page, pageSize, trimmedFilters]);

  function goToPreviousPage() {
    setPage((currentPage) => Math.max(1, currentPage - 1));
  }

  function goToNextPage() {
    setPage((currentPage) => Math.min(totalPages, currentPage + 1));
  }

  function updateFilter(field: keyof AuditLogFilterState, value: string) {
    setDraftFilters((currentFilters) => ({
      ...currentFilters,
      [field]: value,
    }));
  }

  function resetFilters() {
    setDraftFilters(emptyFilters);
  }

  function updatePageSize(value: number) {
    setPageSize(value);
    setPage(1);
  }

  function applySearch() {
    setAppliedFilters({ ...draftFilters });
    setPage(1);
  }

  return (
    <PageSkeleton title="Audit Logs" titleId="admin-audit-page-title" summary={summary}>
      <div className="admin-audit-page">

        {/* <div className="admin-audit-page__actions">
          <Link className="admin-audit-page__back-link" to="/admin/">
            Back to Admin Overview
          </Link>
        </div> */}

        <div className="admin-audit-page__filters">
          <AuditLogFilters
            filters={draftFilters}
            hasActiveFilters={hasActiveFilters}
            isEntityIdDisabled={isEntityIdDisabled}
            onChange={updateFilter}
            onPageSizeChange={updatePageSize}
            onReset={resetFilters}
            onSearch={applySearch}
            pageSize={pageSize}
            pageSizeOptions={pageSizeOptions}
            searchDisabled={isLoading}
          />
        </div>

        {error && (
          <StatusMessage variant="error">
            {error}
          </StatusMessage>
        )}

        {!isLoading && !error && entries.length === 0 && (
          <StatusMessage>
            No audit log entries match the current filters.
          </StatusMessage>
        )}

        <AuditLogTable entries={entries} isLoading={isLoading} />

        {!error && (
          <Pagination
            currentPage={page}
            disabled={isLoading}
            label="Audit log pagination"
            onNext={goToNextPage}
            onPrevious={goToPreviousPage}
            totalPages={totalPages}
          />
        )}
      </div>
    </PageSkeleton>
  );
}

function getErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const payload = err.payload as Record<string, unknown>;
    if (payload && typeof payload === 'object') {
      if ('errors' in payload) {
        const details = formatValidationErrors(payload.errors);
        if (details) {
          return details;
        }
      }

      if ('error' in payload) {
        return String(payload.error);
      }
      if ('message' in payload) {
        return String(payload.message);
      }
    }

    return JSON.stringify(payload);
  }

  return 'Unknown error';
}

function formatValidationErrors(errors: unknown): string | null {
  if (!errors || typeof errors !== 'object') {
    return null;
  }

  const messages = Object.values(errors as Record<string, unknown>)
    .flatMap((value) => (Array.isArray(value) ? value : []))
    .filter((message) => typeof message === 'string' && message.trim().length > 0)
    .map((message) => message.trim());

  return messages.length > 0 ? messages.join('; ') : null;
}
