import type { AdminIssue, IssuePriority, IssueStatus, IssueType } from '../types';
import type {
  CreateIssuePayload,
  IssuePriorityApi,
  IssueResponse,
  IssueStatusApi,
  IssueTypeApi,
} from './adminApi';

const typeToUi: Record<IssueTypeApi, IssueType> = {
  UserBehavior: 'user behavior',
  Payment: 'payment',
  WorkloadAnomaly: 'workload anomaly',
  System: 'system',
  Other: 'other',
};

const typeToApi: Record<IssueType, IssueTypeApi> = {
  'user behavior': 'UserBehavior',
  payment: 'Payment',
  'workload anomaly': 'WorkloadAnomaly',
  system: 'System',
  other: 'Other',
};

const priorityToUi: Record<IssuePriorityApi, IssuePriority> = {
  Low: 'low',
  Medium: 'medium',
  High: 'high',
};

const priorityToApi: Record<IssuePriority, IssuePriorityApi> = {
  low: 'Low',
  medium: 'Medium',
  high: 'High',
};

const statusToUi: Record<IssueStatusApi, IssueStatus> = {
  Open: 'open',
  InProgress: 'in progress',
  Resolved: 'resolved',
};

export function toAdminIssue(issue: IssueResponse): AdminIssue {
  return {
    id: issue.id,
    title: issue.title,
    description: issue.description,
    type: typeToUi[issue.type] ?? 'other',
    priority: priorityToUi[issue.priority] ?? 'medium',
    status: statusToUi[issue.status] ?? 'open',
    reportedBy: issue.reportedByDisplay ?? issue.reportedByUserId,
    assignee: issue.assignedToDisplay ?? issue.assignedToUserId ?? undefined,
    date: issue.createdAtUtc,
  };
}

export function toCreateIssuePayload(input: {
  title: string;
  description: string;
  type: IssueType;
  priority: IssuePriority;
}): CreateIssuePayload {
  return {
    title: input.title,
    description: input.description,
    type: typeToApi[input.type],
    priority: priorityToApi[input.priority],
  };
}
