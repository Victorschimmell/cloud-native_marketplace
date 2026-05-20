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
type SortDirection = 'asc' | 'desc';

export default function OrdersTable({ error, orders, priceFormatter }: OrdersTableProps) {
  const [sortKey, setSortKey] = useState<SortKey>('date');
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc');

  const sortedOrders = useMemo(() => {
    return sortOrders(orders, sortKey, sortDirection);
  }, [orders, sortKey, sortDirection]);

  function handleSort(key: SortKey) {
    if (key === sortKey) {
      setSortDirection((current) => (current === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortKey(key);
      setSortDirection('asc');
    }
  }

  return (
    <div className="seller-dashboard__panel">
      <div className="seller-dashboard__panel-header">
        <h2 className="seller-dashboard__panel-title">Orders Management</h2>
        <p className="seller-dashboard__panel-subtitle">Click column headers to sort</p>
      </div>

      {error ? (
        <p className="seller-dashboard__empty">{error}</p>
      ) : orders.length === 0 ? (
        <p className="seller-dashboard__empty">No orders yet.</p>
      ) : (
        <table className="seller-dashboard__table">
          <thead>
            <tr>
              <th>Order ID</th>
              <th>Customer</th>
              <SortableHeader
                direction={sortDirection}
                isActive={sortKey === 'date'}
                label="Date"
                onClick={() => handleSort('date')}
              />
              <SortableHeader
                direction={sortDirection}
                isActive={sortKey === 'total'}
                label="Total"
                onClick={() => handleSort('total')}
              />
              <SortableHeader
                direction={sortDirection}
                isActive={sortKey === 'status'}
                label="Status"
                onClick={() => handleSort('status')}
              />
              <th>Tracking</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {sortedOrders.map((order) => (
              <OrderRow key={order.id} order={order} priceFormatter={priceFormatter} />
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

function SortableHeader({
  direction,
  isActive,
  label,
  onClick,
}: {
  direction: SortDirection;
  isActive: boolean;
  label: string;
  onClick: () => void;
}) {
  const ariaSort = isActive ? (direction === 'asc' ? 'ascending' : 'descending') : 'none';

  return (
    <th aria-sort={ariaSort}>
      <button
        type="button"
        aria-label={`Sort by ${label}`}
        onClick={onClick}
        className="seller-dashboard__sort-button"
      >
        {label}
        {isActive ? (
          <span className="seller-dashboard__sort-indicator" aria-hidden="true">
            {direction === 'asc' ? 'Asc' : 'Desc'}
          </span>
        ) : null}
      </button>
    </th>
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

function sortOrders(orders: SellerOrderSummary[], key: SortKey, direction: SortDirection): SellerOrderSummary[] {
  const statusRank: Record<OrderStatus, number> = {
    Pending: 0,
    Approved: 1,
    Processing: 1,
    Shipped: 2,
    Delivered: 3,
    Cancelled: 4,
    Returned: 5,
  };

  const sorted = [...orders].sort((a, b) => {
    let result = 0;
    if (key === 'date') {
      result = a.orderPurchaseTimestampUtc.localeCompare(b.orderPurchaseTimestampUtc);
    } else if (key === 'total') {
      result = a.totalAmount - b.totalAmount;
    } else if (key === 'status') {
      result = statusRank[a.orderStatus] - statusRank[b.orderStatus];
    }
    return direction === 'asc' ? result : -result;
  });

  return sorted;
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}
