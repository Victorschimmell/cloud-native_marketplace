import { Link } from 'react-router-dom';
import type { RecentPayment } from '../data/placeholderData';

interface RecentPaymentsPanelProps {
  payments: RecentPayment[];
}

// Recent Payments panel on the admin dashboard
export default function RecentPaymentsPanel({ payments }: RecentPaymentsPanelProps) {
  return (
    <div className="admin-dashboard__panel">
      <div className="admin-dashboard__panel-header">
        <h2 className="admin-dashboard__panel-title">Recent Payments</h2>
        <Link to="/admin/payments" className="admin-dashboard__panel-link">
          View All
        </Link>
      </div>

      {payments.length === 0 ? (
        <p className="admin-dashboard__panel-empty">No recent payments.</p>
      ) : (
        <ul className="admin-dashboard__list">
          {payments.map((payment) => (
            <PaymentRow key={payment.id} payment={payment} />
          ))}
        </ul>
      )}
    </div>
  );
}

function PaymentRow({ payment }: { payment: RecentPayment }) {
  return (
    <li className="admin-dashboard__list-item">
      <div className="admin-dashboard__list-row">
        <p className="admin-dashboard__list-primary">{payment.id}</p>
        <span className={`admin-dashboard__badge admin-dashboard__badge--${payment.status}`}>
          {payment.status}
        </span>
      </div>
      <p className="admin-dashboard__list-secondary">{payment.customerName}</p>
      <div className="admin-dashboard__list-row">
        <p className="admin-dashboard__list-meta">{formatDate(payment.date)}</p>
        <span className="admin-dashboard__list-amount">${payment.amount.toFixed(2)}</span>
      </div>
    </li>
  );
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-GB');
}
