import type { ReactNode } from 'react';
import type { AdminUser } from '../data/placeholderData';

interface UserDetailsPanelProps {
  // Currently selected user, or null if nothing has been clicked yet.
  user: AdminUser | null;
}

// Right-side panel on the User Management page. Empty hint until a user is selected.
export default function UserDetailsPanel({ user }: UserDetailsPanelProps) {
  if (!user) {
    return (
      <div className="admin-users-page__detail-empty">
        Select a user to view details and manage their account
      </div>
    );
  }

  return (
    <div className="admin-users-page__panel">
      <div className="admin-users-page__panel-header">
        <h2 className="admin-users-page__panel-title">{user.name}</h2>
      </div>
      <div className="admin-users-page__detail-body">
        <DetailRow label="Email" value={user.email} />
        {user.company ? <DetailRow label="Company" value={user.company} /> : null}
        <DetailRow label="Role" value={user.role} />
        <DetailRow
          label="Status"
          value={
            <span className={`admin-users-page__badge admin-users-page__badge--${user.status.replace(/\s+/g, '-')}`}>
              {user.status}
            </span>
          }
        />
        <DetailRow label="Registered" value={new Date(user.registeredOn).toLocaleDateString('en-GB')} />
      </div>
    </div>
  );
}

function DetailRow({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="admin-users-page__detail-row">
      <span className="admin-users-page__detail-label">{label}</span>
      <span>{value}</span>
    </div>
  );
}
