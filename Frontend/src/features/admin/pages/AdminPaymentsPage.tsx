import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import AdminHeader from '../components/AdminHeader';
import { placeholderPayments, type PaymentStatus } from '../data/placeholderData';
import '../components/AdminDashboard.css';

// "All" means no filter; the real statuses come from PaymentStatus.
type StatusFilter = PaymentStatus | 'All';

// Full payments list, opened from "View All" on the dashboard.
export default function AdminPaymentsPage() {
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');

  const filteredPayments = useMemo(() => {
    if (statusFilter === 'All') {
      return placeholderPayments;
    }
    return placeholderPayments.filter((payment) => payment.status === statusFilter);
  }, [statusFilter]);

  return (
    <section className="admin-dashboard">
      <AdminHeader title="Payments" />

      <Link to="/analytics" className="admin-dashboard__back">
        ← Back to Dashboard
      </Link>

      <div className="admin-dashboard__panel">
        <div className="admin-dashboard__panel-header">
          <div>
            <h2 className="admin-dashboard__panel-title">All Payments</h2>
            <div className="admin-dashboard__filters" style={{ marginTop: 10 }}>
              <label className="admin-dashboard__filter">
                Status:
                <select
                  value={statusFilter}
                  onChange={(event) => setStatusFilter(event.target.value as StatusFilter)}
                >
                  <option value="All">All</option>
                  <option value="completed">Completed</option>
                  <option value="pending">Pending</option>
                  <option value="failed">Failed</option>
                </select>
              </label>
            </div>
          </div>
        </div>

        {filteredPayments.length === 0 ? (
          <p className="admin-dashboard__panel-empty">No payments match the current filters.</p>
        ) : (
          <table className="admin-dashboard__table">
            <thead>
              <tr>
                <th>Payment ID</th>
                <th>Customer</th>
                <th>Date</th>
                <th>Amount</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {filteredPayments.map((payment) => (
                <tr key={payment.id}>
                  <td><strong>{payment.id}</strong></td>
                  <td>{payment.customerName}</td>
                  <td>{formatDate(payment.date)}</td>
                  <td><strong>${payment.amount.toFixed(2)}</strong></td>
                  <td>
                    <span className={`admin-dashboard__badge admin-dashboard__badge--${payment.status}`}>
                      {payment.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('en-GB');
}
