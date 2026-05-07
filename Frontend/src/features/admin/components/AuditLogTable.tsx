import { useMemo } from 'react';
import type { AuditActionType, AuditLogEntry, AuditOutcome } from '../types';
import './AuditLogTable.css';

interface AuditLogTableProps {
  entries: AuditLogEntry[];
  isLoading: boolean;
}

const actionLabels: Record<string, string> = {
  '1': 'Created',
  '2': 'Updated',
  '3': 'Deleted',
  '4': 'Login',
  '5': 'Logout',
  '6': 'Approve',
  '7': 'Reject',
  '8': 'Block',
  '9': 'Unblock',
  '10': 'Import',
  Created: 'Created',
  Updated: 'Updated',
  Deleted: 'Deleted',
  Login: 'Login',
  Logout: 'Logout',
  Approve: 'Approve',
  Reject: 'Reject',
  Block: 'Block',
  Unblock: 'Unblock',
  Import: 'Import',
};

const outcomeLabels: Record<string, string> = {
  '1': 'Succeeded',
  '2': 'Failed',
  '3': 'Partially succeeded',
  '4': 'Forbidden',
  Succeeded: 'Succeeded',
  Failed: 'Failed',
  PartiallySucceeded: 'Partially succeeded',
  Forbidden: 'Forbidden',
};

export default function AuditLogTable({ entries, isLoading }: AuditLogTableProps) {
  const dateFormatter = useMemo(
    () => new Intl.DateTimeFormat('en-GB', { dateStyle: 'medium', timeStyle: 'short', timeZone: 'UTC' }),
    []
  );

  return (
    <div className="audit-log-table__scroll" aria-busy={isLoading}>
      <table className="audit-log-table">
        <thead className="audit-log-table__head">
          <tr className="audit-log-table__row audit-log-table__row--head">
            <th className="audit-log-table__cell">Time (UTC)</th>
            <th className="audit-log-table__cell">Action</th>
            <th className="audit-log-table__cell">Entity</th>
            <th className="audit-log-table__cell">Actor</th>
            <th className="audit-log-table__cell">Outcome</th>
            <th className="audit-log-table__cell">Details</th>
          </tr>
        </thead>
        <tbody className="audit-log-table__body">
          {isLoading
            ? Array.from({ length: 6 }, (_, index) => (
                <tr className="audit-log-table__row" key={`skeleton-${index}`}>
                  <td className="audit-log-table__cell"><span className="audit-log-table__skeleton" /></td>
                  <td className="audit-log-table__cell"><span className="audit-log-table__skeleton" /></td>
                  <td className="audit-log-table__cell"><span className="audit-log-table__skeleton" /></td>
                  <td className="audit-log-table__cell"><span className="audit-log-table__skeleton" /></td>
                  <td className="audit-log-table__cell"><span className="audit-log-table__skeleton" /></td>
                  <td className="audit-log-table__cell"><span className="audit-log-table__skeleton" /></td>
                </tr>
              ))
            : entries.map((entry) => (
                <tr className="audit-log-table__row" key={entry.id}>
                  <td className="audit-log-table__cell">
                    <div className="audit-log-table__time">
                      {formatDateTime(entry.createdAtUtc, dateFormatter)}
                    </div>
                  </td>
                  <td className="audit-log-table__cell">
                    <span className="audit-log-table__tag">{formatAction(entry.actionType)}</span>
                  </td>
                  <td className="audit-log-table__cell">
                    <div className="audit-log-table__entity">
                      <span className="audit-log-table__entity-type">{entry.targetEntityType}</span>
                      <span className="audit-log-table__entity-id">{entry.targetEntityId}</span>
                    </div>
                  </td>
                  <td className="audit-log-table__cell">
                    <div className="audit-log-table__actor">
                      <span className="audit-log-table__actor-id">{entry.actorUserId ?? 'System'}</span>
                      <span className="audit-log-table__actor-ip">{entry.actorIpAddress ?? 'IP unavailable'}</span>
                    </div>
                  </td>
                  <td className="audit-log-table__cell">
                    <span className={`audit-log-table__pill audit-log-table__pill--${getOutcomeTone(entry.outcome)}`}>
                      {formatOutcome(entry.outcome)}
                    </span>
                  </td>
                  <td className="audit-log-table__cell">
                    <span className="audit-log-table__details">{entry.details}</span>
                  </td>
                </tr>
              ))}
        </tbody>
      </table>
    </div>
  );
}

function formatAction(action: AuditActionType) {
  return actionLabels[String(action)] ?? 'Unknown';
}

function formatOutcome(outcome: AuditOutcome) {
  return outcomeLabels[String(outcome)] ?? 'Unknown';
}

function getOutcomeTone(outcome: AuditOutcome) {
  const key = String(outcome);

  if (key === 'Succeeded' || key === '1') {
    return 'success';
  }

  if (key === 'Failed' || key === '2') {
    return 'error';
  }

  if (key === 'PartiallySucceeded' || key === '3') {
    return 'warning';
  }

  if (key === 'Forbidden' || key === '4') {
    return 'muted';
  }

  return 'muted';
}

function formatDateTime(value: string, formatter: Intl.DateTimeFormat) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return formatter.format(date);
}
