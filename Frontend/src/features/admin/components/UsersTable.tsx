import type { AdminUser } from '../data/placeholderData';

interface UsersTableProps {
  users: AdminUser[];
  // Called when "Manage" is clicked on a row.
  onManage: (user: AdminUser) => void;
}

// Users table on the User Management page. Filters and selection live in the parent.
export default function UsersTable({ users, onManage }: UsersTableProps) {
  if (users.length === 0) {
    return <p className="admin-users-page__empty">No users match the current filters.</p>;
  }

  return (
    <table className="admin-users-page__table">
      <thead>
        <tr>
          <th>User</th>
          <th>Role</th>
          <th>Status</th>
          <th>Registered</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {users.map((user) => (
          <UserRow key={user.id} user={user} onManage={onManage} />
        ))}
      </tbody>
    </table>
  );
}

function UserRow({ user, onManage }: { user: AdminUser; onManage: (user: AdminUser) => void }) {
  return (
    <tr>
      <td>
        <div className="admin-users-page__user-cell">
          <p className="admin-users-page__user-name">{user.name}</p>
          <p className="admin-users-page__user-email">{user.email}</p>
          {user.company ? <p className="admin-users-page__user-company">{user.company}</p> : null}
        </div>
      </td>
      <td>{user.role}</td>
      <td>
        <span className={`admin-users-page__badge admin-users-page__badge--${badgeKey(user.status)}`}>
          {user.status}
        </span>
      </td>
      <td>{formatDate(user.registeredOn)}</td>
      <td>
        <button
          type="button"
          className="admin-users-page__action"
          onClick={() => onManage(user)}
        >
          Manage
        </button>
      </td>
    </tr>
  );
}

function badgeKey(status: string): string {
  return status.replace(/\s+/g, '-');
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-GB');
}
