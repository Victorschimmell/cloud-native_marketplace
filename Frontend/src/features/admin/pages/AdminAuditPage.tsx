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
  const [filters, setFilters] = useState<AuditLogFilterState>(emptyFilters);

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  const summary = isLoading
    ? 'Loading audit logs...'
    : `${totalCount} audit log${totalCount === 1 ? '' : 's'} found`;

  const trimmedFilters = useMemo(() => {
    const actorUserId = filters.actorUserId.trim();
    const entityType = filters.entityType.trim();
    const entityId = filters.entityId.trim();

    return {
      actorUserId: actorUserId || undefined,
      entityType: entityType || undefined,
      entityId: entityType && entityId ? entityId : undefined,
    };
  }, [filters]);

  const hasActiveFilters = Object.values(filters).some((value) => value.trim().length > 0);
  const isEntityIdDisabled = filters.entityType.trim().length === 0;

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
    setFilters((currentFilters) => ({
      ...currentFilters,
      [field]: value,
    }));
    setPage(1);
  }

  function resetFilters() {
    setFilters(emptyFilters);
    setPage(1);
  }

  function updatePageSize(value: number) {
    setPageSize(value);
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
            filters={filters}
            hasActiveFilters={hasActiveFilters}
            isEntityIdDisabled={isEntityIdDisabled}
            onChange={updateFilter}
            onPageSizeChange={updatePageSize}
            onReset={resetFilters}
            pageSize={pageSize}
            pageSizeOptions={pageSizeOptions}
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
