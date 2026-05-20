import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import type { OrderStatus } from '../../orders/types';
import type { SellerOrderSummary } from '../api/sellerApi';

interface OrdersTableProps {
  error?: string | null;
  orders: SellerOrderSummary[];
  priceFormatter: Intl.NumberFormat;
}

type SortKey = 'date' | 'total' | 'status';
type StatusFilter = 'all' | OrderStatus;
type SortOption = 'newest' | 'oldest' | 'total-high' | 'total-low' | 'status';

const statusOptions: StatusFilter[] = [
  'all',
  'Pending',
  'Approved',
  'Processing',
  'Shipped',
  'Delivered',
  'Cancelled',
  'Returned',
];

export default function OrdersTable({ error, orders, priceFormatter }: OrdersTableProps) {
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');
  const [sortBy, setSortBy] = useState<SortOption>('newest');

  const visibleOrders = useMemo(() => {
    const filteredOrders = statusFilter === 'all'
      ? orders
      : orders.filter((order) => order.orderStatus === statusFilter);

    return sortOrders(filteredOrders, sortBy);
  }, [orders, statusFilter, sortBy]);

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
              onChange={(event) => setStatusFilter(event.target.value as StatusFilter)}
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
              value={sortBy}
              onChange={(event) => setSortBy(event.target.value as SortOption)}
            >
              <option value="newest">Newest</option>
              <option value="oldest">Oldest</option>
              <option value="total-high">Total: high to low</option>
              <option value="total-low">Total: low to high</option>
              <option value="status">Status</option>
            </select>
          </label>
        </div>
      </div>

      {error ? (
        <p className="seller-dashboard__empty">{error}</p>
      ) : orders.length === 0 ? (
        <p className="seller-dashboard__empty">No orders yet.</p>
      ) : visibleOrders.length === 0 ? (
        <p className="seller-dashboard__empty">No orders match the current filters.</p>
      ) : (
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
            {visibleOrders.map((order) => (
              <OrderRow key={order.id} order={order} priceFormatter={priceFormatter} />
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

function OrderRow({ order, priceFormatter }: { order: SellerOrderSummary; priceFormatter: Intl.NumberFormat }) {
  const tracking = order.shipments.find((shipment) => shipment.trackingNumber)?.trackingNumber;

  return (
    <tr>
      <td data-label="Order ID"><strong>{order.orderNumber}</strong></td>
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

function sortOrders(orders: SellerOrderSummary[], sortBy: SortOption): SellerOrderSummary[] {
  const statusRank: Record<OrderStatus, number> = {
    Pending: 0,
    Approved: 1,
    Processing: 2,
    Shipped: 3,
    Delivered: 4,
    Cancelled: 5,
    Returned: 6,
  };
  const sortKey = getSortKey(sortBy);
  const sortDirection = getSortDirection(sortBy);

  const sorted = [...orders].sort((a, b) => {
    let result = 0;
    if (sortKey === 'date') {
      result = a.orderPurchaseTimestampUtc.localeCompare(b.orderPurchaseTimestampUtc);
    } else if (sortKey === 'total') {
      result = a.totalAmount - b.totalAmount;
    } else if (sortKey === 'status') {
      result = statusRank[a.orderStatus] - statusRank[b.orderStatus];
    }
    return sortDirection === 'asc' ? result : -result;
  });

  return sorted;
}

function getSortKey(sortBy: SortOption): SortKey {
  if (sortBy === 'total-high' || sortBy === 'total-low') {
    return 'total';
  }

  if (sortBy === 'status') {
    return 'status';
  }

  return 'date';
}

function getSortDirection(sortBy: SortOption): 'asc' | 'desc' {
  return sortBy === 'oldest' || sortBy === 'total-low' || sortBy === 'status' ? 'asc' : 'desc';
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}
