import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { useCurrency } from '../../../shared/currency/useCurrency';
import type { OrderStatus } from '../../orders/types';
import { formatDateTime, formatMoney, getStatusTone } from '../../orders/utils';
import { sellerApi, type SellerOrderSummary } from '../api/sellerApi';
import './SellerOrderDetailsPage.css';

interface StatusAction {
  status: OrderStatus;
  label: string;
  note: string;
}

export default function SellerOrderDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { currency } = useCurrency();
  const [order, setOrder] = useState<SellerOrderSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [pendingStatus, setPendingStatus] = useState<OrderStatus | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadOrder() {
      if (!id) {
        setError('Order could not be found.');
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        const response = await sellerApi.getMyOrder(id, currency, controller.signal);
        setOrder(response);
        setError(null);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }

        setError('Seller order details could not be loaded right now.');
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadOrder();

    return () => controller.abort();
  }, [currency, id]);

  const sellerFulfillmentStatus = useMemo(
    () => (order ? getSellerFulfillmentStatusFromItems(order) : undefined),
    [order],
  );
  const nextAction = useMemo(
    () => (order?.canUpdateStatus ? getNextStatusAction(sellerFulfillmentStatus) : null),
    [order?.canUpdateStatus, sellerFulfillmentStatus],
  );
  const itemCount = order?.items.reduce((total, item) => total + item.quantity, 0) ?? 0;
  const orderStatusDescription = order?.orderStatusDescription?.trim() ?? '';
  const hasOrderStatusDescription = orderStatusDescription.length > 0;
  const earliestShippingLimit = useMemo(() => {
    const limits = order?.items
      .map((item) => item.shippingLimitDateUtc)
      .filter((value): value is string => Boolean(value)) ?? [];

    return limits.sort()[0];
  }, [order?.items]);

  async function updateStatus(status: OrderStatus) {
    if (!order) {
      return;
    }

    try {
      setPendingStatus(status);
      setSuccessMessage(null);
      const updatedOrder = await sellerApi.updateMyOrderStatus(order.id, status, currency);
      setOrder(updatedOrder);
      setError(null);
      setSuccessMessage(`Seller items updated to ${status}.`);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Order status could not be updated.');
    } finally {
      setPendingStatus(null);
    }
  }

  const title = order ? `Order ${order.orderNumber}` : 'Seller Order Details';

  return (
    <PageSkeleton title={title} titleId="seller-order-details-title" summary="Review and update seller fulfillment status.">
      <section className="seller-order-details" aria-labelledby="seller-order-details-title">
        <Link to="/seller/orders" className="seller-order-details__back">
          Back to Dashboard
        </Link>

        {isLoading ? (
          <div className="seller-order-details__notice">Loading order details...</div>
        ) : null}

        {error ? (
          <div className="seller-order-details__notice seller-order-details__notice--error" role="alert">
            {error}
          </div>
        ) : null}

        {successMessage ? (
          <div className="seller-order-details__notice seller-order-details__notice--success" role="status">
            {successMessage}
          </div>
        ) : null}

        {!isLoading && order ? (
          <>
            <section className="seller-order-details__hero">
              <div className="seller-order-details__hero-copy">
                <span className="seller-order-details__eyebrow">Customer order</span>
                <h2>{order.orderNumber}</h2>
                <p>
                  {order.customerName} - {order.customerEmail}
                </p>
              </div>

              <div className="seller-order-details__hero-metrics">
                <div className="seller-order-details__metric">
                  <span>Status</span>
                  <StatusBadge status={order.orderStatus} />
                </div>
                <div className="seller-order-details__metric">
                  <span>Seller total</span>
                  <strong>{formatMoney(order.totalAmount, order.currencyCode)}</strong>
                </div>
                <div className="seller-order-details__metric">
                  <span>Items</span>
                  <strong>{itemCount}</strong>
                </div>
              </div>
            </section>

            <section className={`seller-order-details__grid${hasOrderStatusDescription ? ' seller-order-details__grid--with-cancellation' : ''}`}>
              <article className="seller-order-details__panel seller-order-details__panel--items">
                <div className="seller-order-details__panel-heading">
                  <div>
                    <h2>Seller Items</h2>
                    <p>{itemCount} item{itemCount === 1 ? '' : 's'} assigned to your seller account</p>
                  </div>
                </div>

                <div className="seller-order-details__items">
                  {order.items.map((item) => (
                    <div className="seller-order-details__item" key={item.orderItemId}>
                      <div className="seller-order-details__item-product">
                        <span className="seller-order-details__product-media" aria-hidden="true">
                          {item.imageUrl ? (
                            <img alt="" src={item.imageUrl} />
                          ) : (
                            'No image'
                          )}
                        </span>
                        <div>
                          <h3>{item.productName}</h3>
                          <p>Quantity {item.quantity} - Unit {formatMoney(item.unitPrice, item.currencyCode)}</p>
                          <p>Seller status {item.fulfillmentStatus}</p>
                          <p>Ship by {formatDateTime(item.shippingLimitDateUtc)}</p>
                        </div>
                      </div>
                      <div className="seller-order-details__item-totals">
                        <span>{formatMoney(item.lineTotal, item.currencyCode)}</span>
                        <small>Freight {formatMoney(item.freightValue, item.currencyCode)}</small>
                      </div>
                    </div>
                  ))}
                </div>
              </article>

              <article className="seller-order-details__panel seller-order-details__panel--status">
                <div className="seller-order-details__panel-heading">
                  <div>
                    <h2>Status</h2>
                    <p>Seller item fulfillment controls</p>
                  </div>
                </div>

                <dl className="seller-order-details__facts">
                  <div>
                    <dt>Placed</dt>
                    <dd>{formatDateTime(order.orderPurchaseTimestampUtc)}</dd>
                  </div>
                  <div>
                    <dt>Approved</dt>
                    <dd>{formatDateTime(order.orderApprovedAtUtc)}</dd>
                  </div>
                  <div>
                    <dt>Seller ship-by</dt>
                    <dd>{formatDateTime(earliestShippingLimit)}</dd>
                  </div>
                  <div>
                    <dt>Carrier handoff</dt>
                    <dd>{formatDateTime(order.orderDeliveredCarrierDateUtc)}</dd>
                  </div>
                </dl>
              </article>

              {hasOrderStatusDescription ? (
                <article className="seller-order-details__panel seller-order-details__panel--cancellation">
                  <div className="seller-order-details__panel-heading">
                    <div>
                      <h2>Order Status Description</h2>
                      <p>Reason captured for the current order status</p>
                    </div>
                  </div>

                  <p className="seller-order-details__reason">{orderStatusDescription}</p>
                </article>
              ) : null}

              <article className="seller-order-details__panel seller-order-details__panel--actions">
                <div className="seller-order-details__panel-heading">
                  <div>
                    <h2>Actions</h2>
                    <p>Move your items through the seller workflow</p>
                  </div>
                </div>

                {nextAction ? (
                  <div className="seller-order-details__actions">
                    <button
                      className="seller-order-details__primary-action"
                      disabled={pendingStatus !== null}
                      onClick={() => void updateStatus(nextAction.status)}
                      type="button"
                    >
                      {pendingStatus === nextAction.status ? 'Updating...' : nextAction.label}
                    </button>
                  </div>
                ) : (
                  <p className="seller-order-details__muted">
                    No seller status actions are available for your items on this order.
                  </p>
                )}

                {nextAction ? (
                  <p className="seller-order-details__action-note">
                    {nextAction.note}
                  </p>
                ) : null}
              </article>

              <article className="seller-order-details__panel seller-order-details__panel--totals">
                <div className="seller-order-details__panel-heading">
                  <div>
                    <h2>Seller Totals</h2>
                    <p>Only amounts for products assigned to your seller account</p>
                  </div>
                </div>

                <dl className="seller-order-details__totals">
                  <div>
                    <dt>Subtotal</dt>
                    <dd>{formatMoney(order.subtotalAmount, order.currencyCode)}</dd>
                  </div>
                  <div>
                    <dt>Freight</dt>
                    <dd>{formatMoney(order.freightAmount, order.currencyCode)}</dd>
                  </div>
                  <div>
                    <dt>Total</dt>
                    <dd>{formatMoney(order.totalAmount, order.currencyCode)}</dd>
                  </div>
                </dl>
              </article>
            </section>

            <section className="seller-order-details__panel seller-order-details__panel--path">
              <div className="seller-order-details__panel-heading">
                <div>
                  <h2>Status Path</h2>
                  <p>Order progress derived from all seller items</p>
                </div>
              </div>

              <dl className="seller-order-details__status-path">
                <StatusPathItem
                  detail={formatDateTime(order.orderPurchaseTimestampUtc)}
                  isComplete
                  label="Order placed"
                  value="Placed"
                />
                <StatusPathItem
                  detail={order.orderApprovedAtUtc ? formatDateTime(order.orderApprovedAtUtc) : undefined}
                  isComplete={isAtLeastStatus(order.orderStatus, 'Approved')}
                  label="Seller approval"
                  value={getOrderApprovalState(order)}
                />
                <StatusPathItem
                  detail={order.orderDeliveredCarrierDateUtc ? formatDateTime(order.orderDeliveredCarrierDateUtc) : undefined}
                  isComplete={isAtLeastStatus(order.orderStatus, 'Shipped')}
                  label="Order fulfillment"
                  value={getOrderFulfillmentState(order)}
                />
              </dl>
            </section>
          </>
        ) : null}
      </section>
    </PageSkeleton>
  );
}

function StatusBadge({ status }: { status: OrderStatus }) {
  return (
    <span className="seller-order-details__status" data-tone={getStatusTone(status)}>
      {status}
    </span>
  );
}

function StatusPathItem({
  detail,
  isComplete,
  label,
  value,
}: {
  detail?: string;
  isComplete: boolean;
  label: string;
  value: string;
}) {
  return (
    <div className="seller-order-details__path-item" data-complete={isComplete}>
      <dt>{label}</dt>
      <dd>{value}</dd>
      {detail ? <span>{detail}</span> : null}
    </div>
  );
}

function getNextStatusAction(status: OrderStatus | undefined): StatusAction | null {
  if (status === 'Pending') {
    return {
      status: 'Approved',
      label: 'Approve Seller Items',
      note: 'Approves the products assigned to your seller account.',
    };
  }

  if (status === 'Approved') {
    return {
      status: 'Processing',
      label: 'Start Processing',
      note: 'Use this when seller-owned items are being prepared.',
    };
  }

  if (status === 'Processing') {
    return {
      status: 'Shipped',
      label: 'Ship Items On Order',
      note: 'Use this once the seller-owned items have left your fulfillment queue.',
    };
  }

  if (status === 'Shipped') {
    return {
      status: 'Delivered',
      label: 'Mark Items Delivered',
      note: 'Use this once the seller-owned items have been delivered to the customer.',
    };
  }

  return null;
}

function getOrderApprovalState(order: SellerOrderSummary): string {
  return isAtLeastStatus(order.orderStatus, 'Approved') ? 'Approved' : 'Pending';
}

function getOrderFulfillmentState(order: SellerOrderSummary): string {
  if (order.orderStatus === 'Delivered') {
    return 'Delivered';
  }

  if (order.orderStatus === 'Shipped' || order.orderDeliveredCarrierDateUtc) {
    return 'In transit';
  }

  if (order.orderStatus === 'Processing') {
    return 'Preparing';
  }

  return 'Not started';
}

function getSellerFulfillmentStatusFromItems(order: SellerOrderSummary): OrderStatus {
  if (order.items.length === 0) {
    return 'Pending';
  }

  return order.items.reduce<OrderStatus>((lowestStatus, item) => (
    getStatusRank(item.fulfillmentStatus) < getStatusRank(lowestStatus) ? item.fulfillmentStatus : lowestStatus
  ), 'Delivered');
}

function isAtLeastStatus(current: OrderStatus, target: OrderStatus): boolean {
  return getStatusRank(current) >= getStatusRank(target);
}

function getStatusRank(status: OrderStatus): number {
  const rank: Record<OrderStatus, number> = {
    Pending: 1,
    Approved: 2,
    Processing: 3,
    Shipped: 4,
    Delivered: 5,
    Cancelled: 0,
    Returned: 0,
  };

  return rank[status];
}
