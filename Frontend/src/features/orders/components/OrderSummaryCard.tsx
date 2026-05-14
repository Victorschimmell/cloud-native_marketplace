import { Link } from 'react-router-dom';
import type { OrderStatus, OrderSummary } from '../types';
import { formatDateTime, formatMoney, getStatusTone } from '../utils';
import ShipmentTrackingBox from './ShipmentTrackingBox';
import './OrderSummaryCard.css';

const cancellableStatuses = new Set<OrderStatus>(['Pending', 'Approved', 'Processing']);
const returnableStatuses = new Set<OrderStatus>(['Delivered']);

interface OrderSummaryCardProps {
  order: OrderSummary;
}

export default function OrderSummaryCard({ order }: OrderSummaryCardProps) {
  const canCancel = cancellableStatuses.has(order.orderStatus);
  const canReturn = returnableStatuses.has(order.orderStatus);
  const showShipments = isShipmentVisible(order) && order.shipments.length > 0;

  return (
    <article className="orders-page__card">
      <header className="orders-page__card-header">
        <div className="orders-page__card-main">
          <p className="orders-page__eyebrow">Order {order.orderNumber}</p>
          <p className="orders-page__card-date">{formatDateTime(order.orderPurchaseTimestampUtc)}</p>
        </div>
        <div className="orders-page__card-status">
          <StatusBadge label={order.orderStatus} />
        </div>
      </header>

      <div className="orders-page__items">
        {order.items.map((item) => (
          <div className="orders-page__item" key={`${item.orderId}-${item.orderItemId}`}>
            <span className="orders-page__item-name">
              {item.productName} × {item.quantity}
            </span>
            <span className="orders-page__item-price">{formatMoney(item.lineTotal, item.currencyCode)}</span>
          </div>
        ))}
      </div>

      {showShipments ? (
        <div className="orders-page__shipments">
          {order.shipments.map((shipment) => (
            <ShipmentTrackingBox key={shipment.id} shipment={shipment} />
          ))}
        </div>
      ) : null}

      <footer className="orders-page__footer">
        <div className="orders-page__total">
          Total: <strong>{formatMoney(order.totalAmount, order.currencyCode)}</strong>
        </div>
        <div className="orders-page__actions">
          <Link className="orders-page__action orders-page__action--primary" to={`/orders/${order.id}`}>
            View details
          </Link>
          {canCancel ? (
            <Link className="orders-page__action orders-page__action--ghost" to={`/orders/${order.id}#cancel`}>
              Cancel order
            </Link>
          ) : null}
          {canReturn ? (
            <Link className="orders-page__action orders-page__action--ghost" to={`/orders/${order.id}#return`}>
              Request return
            </Link>
          ) : null}
        </div>
      </footer>
    </article>
  );
}

function StatusBadge({ label }: { label: OrderStatus }) {
  return (
    <span className="orders-page__status" data-tone={getStatusTone(label)}>
      {label}
    </span>
  );
}

function isShipmentVisible(order: OrderSummary) {
  return order.orderStatus === 'Shipped' || order.orderStatus === 'Delivered';
}
