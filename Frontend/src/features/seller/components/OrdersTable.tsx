import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import type { OrderStatus, SellerOrder } from '../data/placeholderData';

interface OrdersTableProps {
  orders: SellerOrder[];
  priceFormatter: Intl.NumberFormat;
}

type SortKey = 'date' | 'total' | 'status';
type SortDirection = 'asc' | 'desc';

export default function OrdersTable({ orders, priceFormatter }: OrdersTableProps) {
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

      {orders.length === 0 ? (
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

function OrderRow({ order, priceFormatter }: { order: SellerOrder; priceFormatter: Intl.NumberFormat }) {
  return (
    <tr>
      <td data-label="Order ID"><strong>{order.id}</strong></td>
      <td data-label="Customer">
        <p className="seller-dashboard__customer-name">{order.customerName}</p>
        <p className="seller-dashboard__customer-email">{order.customerEmail}</p>
      </td>
      <td data-label="Date">{formatDate(order.date)}</td>
      <td className="seller-dashboard__price" data-label="Total">{priceFormatter.format(order.total)}</td>
      <td data-label="Status">
        <span className={`seller-dashboard__status seller-dashboard__status--${order.status.toLowerCase()}`}>
          {order.status}
        </span>
      </td>
      <td data-label="Tracking">{order.tracking ?? '-'}</td>
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

function sortOrders(orders: SellerOrder[], key: SortKey, direction: SortDirection): SellerOrder[] {
  const statusRank: Record<OrderStatus, number> = {
    Pending: 0,
    Processing: 1,
    Shipped: 2,
    Delivered: 3,
    Cancelled: 4,
  };

  const sorted = [...orders].sort((a, b) => {
    let result = 0;
    if (key === 'date') {
      result = a.date.localeCompare(b.date);
    } else if (key === 'total') {
      result = a.total - b.total;
    } else if (key === 'status') {
      result = statusRank[a.status] - statusRank[b.status];
    }
    return direction === 'asc' ? result : -result;
  });

  return sorted;
}

function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}
