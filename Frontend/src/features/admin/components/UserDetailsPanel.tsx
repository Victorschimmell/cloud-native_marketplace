import type { AdminUser } from '../data/placeholderData';

interface UserDetailsPanelProps {
  // Currently selected user, or null if nothing has been clicked yet.
  user: AdminUser | null;
}

// Right-side panel on the User Management page. Empty while no user is selected.
export default function UserDetailsPanel({ user }: UserDetailsPanelProps) {
  if (!user) {
    return (
      <div className="admin-dashboard__detail">
        Select a user to view details and manage their account
      </div>
    );
  }

  return (
    <div className="admin-dashboard__panel">
      <div className="admin-dashboard__panel-header">
        <h2 className="admin-dashboard__panel-title">{user.name}</h2>
      </div>
      <div style={{ padding: '20px 24px', display: 'grid', gap: 10, fontSize: 14 }}>
        <DetailRow label="Email" value={user.email} />
        {user.company ? <DetailRow label="Company" value={user.company} /> : null}
        <DetailRow label="Role" value={user.role} />
        <DetailRow
          label="Status"
          value={
            <span className={`admin-dashboard__badge admin-dashboard__badge--${user.status.replace(/\s+/g, '-')}`}>
              {user.status}
            </span>
          }
        />
        <DetailRow label="Registered" value={new Date(user.registeredOn).toLocaleDateString('en-GB')} />
      </div>
    </div>
  );
}

function DetailRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12 }}>
      <span style={{ color: '#6b7280', fontWeight: 600 }}>{label}</span>
      <span>{value}</span>
    </div>
  );
}
