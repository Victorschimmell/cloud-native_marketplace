import { Link } from 'react-router-dom';
import type { SellerOrderSort, SellerOrderStatusFilter, SellerOrderSummary } from '../api/sellerApi';

interface OrdersTableProps {
  error?: string | null;
  orders: SellerOrderSummary[];
  page: number;
  pageSize: number;
  priceFormatter: Intl.NumberFormat;
  sort: SellerOrderSort;
  statusFilter: SellerOrderStatusFilter;
  totalCount: number;
  onPageChange: (page: number) => void;
  onSortChange: (sort: SellerOrderSort) => void;
  onStatusFilterChange: (status: SellerOrderStatusFilter) => void;
}

const statusOptions: SellerOrderStatusFilter[] = [
  'all',
  'Pending',
  'Approved',
  'Processing',
  'Shipped',
  'Delivered',
  'Cancelled',
  'Returned',
];

export default function OrdersTable({
  error,
  orders,
  page,
  pageSize,
  priceFormatter,
  sort,
  statusFilter,
  totalCount,
  onPageChange,
  onSortChange,
  onStatusFilterChange,
}: OrdersTableProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const firstVisible = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const lastVisible = Math.min(page * pageSize, totalCount);

  return (
    <div className="seller-dashboard__panel">
      <div className="seller-dashboard__panel-header">
        <div>
          <h2 className="seller-dashboard__panel-title">Orders Management</h2>
          <p className="seller-dashboard__panel-subtitle">Filter and sort fulfillment activity</p>
        </div>

        <div className="seller-dashboard__filters">
          <label className="seller-dashboard__filter">
            Status:
            <select
              value={statusFilter}
              onChange={(event) => onStatusFilterChange(event.target.value as SellerOrderStatusFilter)}
            >
              {statusOptions.map((status) => (
                <option key={status} value={status}>
                  {status === 'all' ? 'All' : status}
                </option>
              ))}
            </select>
          </label>

          <label className="seller-dashboard__filter">
            Sort:
            <select
              value={sort}
              onChange={(event) => onSortChange(event.target.value as SellerOrderSort)}
            >
              <option value="newest">Newest</option>
              <option value="oldest">Oldest</option>
              <option value="total-high">Total: high to low</option>
              <option value="total-low">Total: low to high</option>
            </select>
          </label>
        </div>
      </div>

      {error ? (
        <p className="seller-dashboard__empty">{error}</p>
      ) : orders.length === 0 ? (
        <p className="seller-dashboard__empty">
          {statusFilter === 'all' ? 'No orders yet.' : 'No orders match the current filters.'}
        </p>
      ) : (
        <>
          <table className="seller-dashboard__table">
            <thead>
              <tr>
                <th>Order ID</th>
                <th>Customer</th>
                <th>Date</th>
                <th>Total</th>
                <th>Status</th>
                <th>Tracking</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <OrderRow key={order.id} order={order} priceFormatter={priceFormatter} />
              ))}
            </tbody>
          </table>

          <div className="seller-dashboard__pagination">
            <span>
              Showing {firstVisible}-{lastVisible} of {totalCount}
            </span>
            <div className="seller-dashboard__pagination-actions">
              <button
                type="button"
                className="seller-dashboard__page-button"
                disabled={page <= 1}
                onClick={() => onPageChange(page - 1)}
              >
                Previous
              </button>
              <button
                type="button"
                className="seller-dashboard__page-button"
                disabled={page >= totalPages}
                onClick={() => onPageChange(page + 1)}
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

function OrderRow({ order, priceFormatter }: { order: SellerOrderSummary; priceFormatter: Intl.NumberFormat }) {
  const tracking = order.shipments.find((shipment) => shipment.trackingNumber)?.trackingNumber;

  return (
    <tr>
      <td data-label="Order ID">
        <Link to={`/seller/orders/${order.id}`} className="seller-dashboard__product-button">
          {order.orderNumber}
        </Link>
      </td>
      <td data-label="Customer">
        <p className="seller-dashboard__customer-name">{order.customerName}</p>
        <p className="seller-dashboard__customer-email">{order.customerEmail}</p>
      </td>
      <td data-label="Date">{formatDate(order.orderPurchaseTimestampUtc)}</td>
      <td className="seller-dashboard__price" data-label="Total">{priceFormatter.format(order.totalAmount)}</td>
      <td data-label="Status">
        <span className={`seller-dashboard__status seller-dashboard__status--${order.orderStatus.toLowerCase()}`}>
          {order.orderStatus}
        </span>
      </td>
      <td data-label="Tracking">{tracking ?? '-'}</td>
      <td data-label="Actions">
        <Link
          to={`/seller/orders/${order.id}`}
          className="seller-dashboard__action seller-dashboard__action--view"
        >
          View Details
        </Link>
      </td>
    </tr>
  );
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}
