// Types and placeholder data for the admin pages.
// This has been added so is visible how the view will look like
// Remove once real API calls and Data come from back-end 

export interface AnalyticsStats {
  activeUsers: number;
  activeUsersChange: number; // percentage change vs previous period
  requestsPerDay: number;
  requestsPerDayChange: number;
  totalRevenue: number;
  totalRevenueChange: number;
  unresolvedIssues: number;
}

export type PaymentStatus = 'completed' | 'pending' | 'failed';

export interface RecentPayment {
  id: string;
  customerName: string;
  date: string; // ISO date
  amount: number;
  status: PaymentStatus;
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
  date: string; // ISO date
}

export type AdminUserRole = 'Customer' | 'Seller' | 'Admin';
export type AdminUserStatus = 'active' | 'pending verification' | 'blocked';

export interface AdminUser {
  id: string;
  name: string;
  email: string;
  company?: string; // only shown for sellers
  sellerId?: string;
  pendingVerificationRequestId?: string;
  role: AdminUserRole;
  status: AdminUserStatus;
  registeredOn: string; // ISO date
}

export const placeholderStats: AnalyticsStats = {
  activeUsers: 0,
  activeUsersChange: 0,
  requestsPerDay: 0,
  requestsPerDayChange: 0,
  totalRevenue: 0,
  totalRevenueChange: 0,
  unresolvedIssues: 0,
};

export const placeholderPayments: RecentPayment[] = [];

export const placeholderIssues: AdminIssue[] = [];

// Place Holder Data
// Empty this array once the API calls are used.
export const placeholderUsers: AdminUser[] = [
  {
    id: 'u1',
    name: 'John Doe',
    email: 'placeholder.john.doe@example.com',
    role: 'Customer',
    status: 'active',
    registeredOn: '2026-01-15',
  },
  {
    id: 'u2',
    name: 'New Seller',
    email: 'placeholder.new.seller@example.com',
    company: 'New Shop LLC',
    role: 'Seller',
    status: 'pending verification',
    registeredOn: '2026-03-08',
  },
  {
    id: 'u3',
    name: 'Blocked User',
    email: 'placeholder.blocked.user@example.com',
    role: 'Customer',
    status: 'blocked',
    registeredOn: '2026-03-01',
  },
];
