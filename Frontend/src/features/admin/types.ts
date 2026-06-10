export type AuditActionType =
  | 'Created'
  | 'Updated'
  | 'Deleted'
  | 'Login'
  | 'Logout'
  | 'Approve'
  | 'Reject'
  | 'Block'
  | 'Unblock'
  | 'Import'
  | 'Cancelled'
  | 'Refunded'
  | number;

export type AuditOutcome =
  | 'Succeeded'
  | 'Failed'
  | 'PartiallySucceeded'
  | 'Forbidden'
  | number;

export interface AuditLogEntry {
  id: string;
  actorUserId?: string | null;
  actorIpAddress?: string | null;
  actionType: AuditActionType;
  targetEntityType: string;
  targetEntityId: string;
  outcome: AuditOutcome;
  details: string;
  createdAtUtc: string;
}

export interface AuditLogFilterState {
  actorUserId: string;
  entityType: string;
  entityId: string;
}
