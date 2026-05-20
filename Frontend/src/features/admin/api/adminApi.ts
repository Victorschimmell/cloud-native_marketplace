import { request } from '../../../shared/api/request';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { PageResponse } from '../../../shared/types/pagination';

// ── Dashboard stats ────────────────────────────────────────────────

export interface DashboardStatsResponse {
  activeUsers: number;
  ordersInLast24Hours: number;
  totalRevenue: number;
  currencyCode: string;
  openIssues: number;
  generatedAtUtc: string;
}

// ── Users ───────────────────────────────────────────────────────────

export type AdminUserRoleApi = 'Any' | 'Customer' | 'Seller' | 'Admin';
export type AdminUserStatusApi = 'Any' | 'Active' | 'PendingVerification' | 'Blocked';

export interface AdminUserResponse {
  id: string;
  email: string;
  name: string;
  role: 'Customer' | 'Seller' | 'Admin';
  status: 'active' | 'pending verification' | 'blocked';
  company?: string | null;
  sellerId?: string | null;
  pendingVerificationRequestId?: string | null;
  registeredOn: string;
  lastLoginAtUtc?: string | null;
}

interface GetUsersOptions {
  role?: AdminUserRoleApi;
  status?: AdminUserStatusApi;
  signal?: AbortSignal;
}

// ── Payments ────────────────────────────────────────────────────────

export interface AdminPaymentResponse {
  id: string;
  orderId: string;
  paymentSequential: number;
  customerName: string;
  date: string;
  amount: number;
  currencyCode: string;
  status: 'completed' | 'pending' | 'failed';
}

interface ListPaymentsOptions {
  signal?: AbortSignal;
}

// ── Issues ──────────────────────────────────────────────────────────

export type IssueTypeApi = 'UserBehavior' | 'Payment' | 'WorkloadAnomaly' | 'System' | 'Other';
export type IssuePriorityApi = 'Low' | 'Medium' | 'High';
export type IssueStatusApi = 'Open' | 'InProgress' | 'Resolved';

export interface IssueResponse {
  id: string;
  title: string;
  description: string;
  type: IssueTypeApi;
  priority: IssuePriorityApi;
  status: IssueStatusApi;
  reportedByUserId: string;
  reportedByDisplay?: string | null;
  assignedToUserId?: string | null;
  resolvedByUserId?: string | null;
  resolvedAtUtc?: string | null;
  resolution?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateIssuePayload {
  title: string;
  description: string;
  type: IssueTypeApi;
  priority: IssuePriorityApi;
}

interface ListIssuesOptions {
  status?: IssueStatusApi;
  priority?: IssuePriorityApi;
  signal?: AbortSignal;
}

interface VerifySellerPayload {
  approve: boolean;
  reviewNotes?: string | null;
  rejectionReason?: string | null;
}

// ── API client ──────────────────────────────────────────────────────

export const adminApi = {
  getDashboardStats: async (currency: CurrencyCode, signal?: AbortSignal) => {
    const params = new URLSearchParams({ currency });
    return request<DashboardStatsResponse>(`/api/admin/dashboard/stats?${params}`, { signal });
  },

  listUsers: async (page = 1, pageSize = 50, options?: GetUsersOptions) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (options?.role && options.role !== 'Any') params.set('role', options.role);
    if (options?.status && options.status !== 'Any') params.set('status', options.status);

    return request<PageResponse<AdminUserResponse>>(`/api/admin/users?${params}`, {
      signal: options?.signal,
    });
  },

  blockUser: async (userId: string, reason?: string, signal?: AbortSignal) => {
    return request<unknown>(`/api/admin/users/${userId}/block`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason: reason ?? null }),
      signal,
    });
  },

  unblockUser: async (userId: string, reason?: string, signal?: AbortSignal) => {
    return request<unknown>(`/api/admin/users/${userId}/unblock`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason: reason ?? null }),
      signal,
    });
  },

  verifySeller: async (
    sellerId: string,
    verificationRequestId: string,
    payload: VerifySellerPayload,
    signal?: AbortSignal,
  ) => {
    return request<unknown>(`/api/admin/sellers/${sellerId}/verify`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        verificationRequestId,
        approve: payload.approve,
        reviewNotes: payload.reviewNotes ?? null,
        rejectionReason: payload.rejectionReason ?? null,
      }),
      signal,
    });
  },

  listPayments: async (currency: CurrencyCode, page = 1, pageSize = 50, options?: ListPaymentsOptions) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      currency,
    });

    return request<PageResponse<AdminPaymentResponse>>(`/api/admin/payments?${params}`, {
      signal: options?.signal,
    });
  },

  listIssues: async (page = 1, pageSize = 50, options?: ListIssuesOptions) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (options?.status) params.set('status', options.status);
    if (options?.priority) params.set('priority', options.priority);

    return request<PageResponse<IssueResponse>>(`/api/admin/issues?${params}`, {
      signal: options?.signal,
    });
  },

  getIssue: async (issueId: string, signal?: AbortSignal) => {
    return request<IssueResponse>(`/api/admin/issues/${issueId}`, { signal });
  },

  createIssue: async (payload: CreateIssuePayload, signal?: AbortSignal) => {
    return request<IssueResponse>('/api/admin/issues', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
      signal,
    });
  },

  resolveIssue: async (issueId: string, resolution?: string, signal?: AbortSignal) => {
    return request<IssueResponse>(`/api/admin/issues/${issueId}/resolve`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ resolution: resolution ?? null }),
      signal,
    });
  },

  // Assigns the issue to the currently-logged-in admin (assign-to-self).
  assignIssue: async (issueId: string, signal?: AbortSignal) => {
    return request<IssueResponse>(`/api/admin/issues/${issueId}/assign`, {
      method: 'POST',
      signal,
    });
  },
};
