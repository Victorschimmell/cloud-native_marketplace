import type { AuditLogFilterState } from '../types';
import searchIconUrl from '../../../assets/search.svg';
import './AuditLogFilters.css';

interface AuditLogFiltersProps {
  filters: AuditLogFilterState;
  hasActiveFilters: boolean;
  isEntityIdDisabled: boolean;
  onChange: (field: keyof AuditLogFilterState, value: string) => void;
  onPageSizeChange: (value: number) => void;
  onReset: () => void;
  onSearch: () => void;
  pageSize: number;
  pageSizeOptions: readonly number[];
  searchDisabled: boolean;
}

export default function AuditLogFilters({
  filters,
  hasActiveFilters,
  isEntityIdDisabled,
  onChange,
  onPageSizeChange,
  onReset,
  onSearch,
  pageSize,
  pageSizeOptions,
  searchDisabled,
}: AuditLogFiltersProps) {
  return (
    <div className="audit-log-filters">
      <label className="audit-log-filters__control">
        <span>Actor user ID</span>
        <input
          inputMode="text"
          onChange={(event) => onChange('actorUserId', event.target.value)}
          placeholder="e.g. 4b30c152-3d7e-44f4-b51c-2e8fe0d03c01"
          spellCheck={false}
          type="text"
          value={filters.actorUserId}
        />
      </label>

      <label className="audit-log-filters__control">
        <span>Entity type</span>
        <input
          inputMode="text"
          onChange={(event) => onChange('entityType', event.target.value)}
          placeholder="e.g. UserAccount"
          spellCheck={false}
          type="text"
          value={filters.entityType}
        />
      </label>

      <label className="audit-log-filters__control">
        <span>Entity ID</span>
        <input
          disabled={isEntityIdDisabled}
          inputMode="text"
          onChange={(event) => onChange('entityId', event.target.value)}
          placeholder="Provide entity type first"
          spellCheck={false}
          type="text"
          value={filters.entityId}
        />
      </label>

      <label className="audit-log-filters__control">
        <span>Page size</span>
        <select
          onChange={(event) => onPageSizeChange(Number(event.target.value))}
          value={pageSize}
        >
          {pageSizeOptions.map((option) => (
            <option key={option} value={option}>
              {option} per page
            </option>
          ))}
        </select>
      </label>

      <div className="audit-log-filters__actions">
        <div className="audit-log-filters__action-buttons">
          <button
            className="audit-log-filters__button"
            disabled={!hasActiveFilters}
            onClick={onReset}
            type="button"
          >
            Clear filters
          </button>
          <button
            aria-label="Search audit logs"
            className="audit-log-filters__button audit-log-filters__button--icon"
            disabled={searchDisabled}
            onClick={onSearch}
            type="button"
          >
            <img className="audit-log-filters__icon" src={searchIconUrl} alt="" aria-hidden="true" />
          </button>
        </div>
      </div>
    </div>
  );
}
