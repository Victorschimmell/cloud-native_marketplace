import { useEffect, useMemo, useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { adminApi, type AdminPaymentResponse } from '../api/adminApi';
import type { PaymentStatus } from '../data/placeholderData';
import './AdminPaymentsPage.css';

// "All" means no filter; the real statuses come from PaymentStatus.
type StatusFilter = PaymentStatus | 'All';

// Full payments list, opened from "View All" on the dashboard.
export default function AdminPaymentsPage() {
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [payments, setPayments] = useState<AdminPaymentResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { currency } = useCurrency();
  const priceFormatter = useMemo(
    () => new Intl.NumberFormat(getCurrencyLocale(currency), { style: 'currency', currency }),
    [currency],
  );

  useEffect(() => {
    const abortController = new AbortController();

    async function load() {
      setIsLoading(true);
      setError(null);
      try {
        const response = await adminApi.listPayments(currency, 1, 100, abortController.signal);
        setPayments(response.items);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }
        setError('Could not load payments.');
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void load();
    return () => abortController.abort();
  }, [currency]);

  const filteredPayments = useMemo(() => {
    if (statusFilter === 'All') {
      return payments;
    }
    return payments.filter((payment) => payment.status === statusFilter);
  }, [statusFilter, payments]);

  return (
    <PageSkeleton title="Payments" summary="Every payment recorded on the platform.">
      <div className="admin-payments-page">
        <div className="admin-payments-page__panel">
          <div className="admin-payments-page__panel-header">
            <h2 className="admin-payments-page__panel-title">All Payments</h2>
            <div className="admin-payments-page__filters">
              <label className="admin-payments-page__filter">
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

          {error ? (
            <p className="admin-payments-page__empty">{error}</p>
          ) : isLoading ? (
            <p className="admin-payments-page__empty">Loading payments...</p>
          ) : filteredPayments.length === 0 ? (
            <p className="admin-payments-page__empty">No payments match the current filters.</p>
          ) : (
            <table className="admin-payments-page__table">
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
                  <tr key={`${payment.orderId}-${payment.paymentSequential}`}>
                    <td><strong>{payment.id}</strong></td>
                    <td>{payment.customerName}</td>
                    <td>{formatDate(payment.date)}</td>
                    <td><strong>{priceFormatter.format(payment.amount)}</strong></td>
                    <td>
                      <span className={`admin-payments-page__badge admin-payments-page__badge--${payment.status}`}>
                        {payment.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </PageSkeleton>
  );
}

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('en-GB');
}
