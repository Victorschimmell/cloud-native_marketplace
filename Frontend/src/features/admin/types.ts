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

export type IssuePriority = 'low' | 'medium' | 'high';
export type IssueStatus = 'open' | 'in progress' | 'resolved';
export type IssueType = 'user behavior' | 'payment' | 'workload anomaly' | 'system' | 'other';

export interface AdminIssue {
  id: string;
  title: string;
  description: string;
  type: IssueType;
  priority: IssuePriority;
  status: IssueStatus;
  reportedBy: string;
  assignee?: string;
  date: string;
}

export type AdminUserRole = 'Customer' | 'Seller' | 'Admin';
export type AdminUserStatus = 'active' | 'pending verification' | 'blocked';

export interface AdminUser {
  id: string;
  name: string;
  email: string;
  company?: string;
  sellerId?: string;
  pendingVerificationRequestId?: string;
  role: AdminUserRole;
  status: AdminUserStatus;
  registeredOn: string;
}
