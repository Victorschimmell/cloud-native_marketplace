import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import AdminHeader from '../components/AdminHeader';
import UsersTable from '../components/UsersTable';
import UserDetailsPanel from '../components/UserDetailsPanel';
import {
  placeholderUsers,
  type AdminUser,
  type AdminUserRole,
  type AdminUserStatus,
} from '../data/placeholderData';
import '../components/AdminDashboard.css';

// "All" option is for no filter.
type RoleFilter = AdminUserRole | 'All';
type StatusFilter = 'active' | 'pending' | 'blocked' | 'All';

// User Management page. Filter users by role + status, click "Manage" to see details.
export default function AdminUsersPage() {
  const [roleFilter, setRoleFilter] = useState<RoleFilter>('All');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null);

  const filteredUsers = useMemo(() => {
    return placeholderUsers.filter((user) => {
      if (roleFilter !== 'All' && user.role !== roleFilter) {
        return false;
      }
      if (statusFilter !== 'All' && !matchesStatus(user.status, statusFilter)) {
        return false;
      }
      return true;
    });
  }, [roleFilter, statusFilter]);

  return (
    <section className="admin-dashboard">
      <AdminHeader title="User Management" />

      <Link to="/analytics" className="admin-dashboard__back">
        ← Back to Dashboard
      </Link>

      <div className="admin-dashboard__layout--users">
        <div className="admin-dashboard__panel">
          <div className="admin-dashboard__panel-header">
            <div>
              <h2 className="admin-dashboard__panel-title">All Users</h2>
              <div className="admin-dashboard__filters" style={{ marginTop: 10 }}>
                <label className="admin-dashboard__filter">
                  Role:
                  <select
                    value={roleFilter}
                    onChange={(event) => setRoleFilter(event.target.value as RoleFilter)}
                  >
                    <option value="All">All</option>
                    <option value="Customer">Customer</option>
                    <option value="Seller">Seller</option>
                    <option value="Admin">Admin</option>
                  </select>
                </label>

                <label className="admin-dashboard__filter">
                  Status:
                  <select
                    value={statusFilter}
                    onChange={(event) => setStatusFilter(event.target.value as StatusFilter)}
                  >
                    <option value="All">All</option>
                    <option value="active">Active</option>
                    <option value="pending">Pending</option>
                    <option value="blocked">Blocked</option>
                  </select>
                </label>
              </div>
            </div>
          </div>

          <UsersTable users={filteredUsers} onManage={setSelectedUser} />
        </div>

        <UserDetailsPanel user={selectedUser} />
      </div>
    </section>
  );
}

// The "pending" filter is stands for "pending verification".
function matchesStatus(status: AdminUserStatus, filter: StatusFilter): boolean {
  if (filter === 'pending') {
    return status === 'pending verification';
  }
  return status === filter;
}
