import { useEffect, useMemo, useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { orderApi } from '../api/orderApi';
import type { Order } from '../types';
import { formatDateTime, formatMoney, getOrderItemCount, getPaymentStatus, getStatusTone } from '../utils';
import './OrderDetailsPage.css';

export default function OrderDetailsPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const { currency } = useCurrency();
  const [order, setOrder] = useState<Order | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const isConfirmed = searchParams.get('confirmed') === '1';

  useEffect(() => {
    if (!id) {
      setError('Order id is missing.');
      setIsLoading(false);
      return;
    }

    const abortController = new AbortController();

    async function loadOrder() {
      try {
        setIsLoading(true);
        setError(null);
        const response = await orderApi.getOrder(id!, currency, abortController.signal);
        setOrder(response);
      } catch (requestError) {
        if (!abortController.signal.aborted) {
          setError(requestError instanceof Error ? requestError.message : 'Could not load this order.');
        }
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadOrder();
    return () => abortController.abort();
  }, [currency, id]);

  const paymentStatus = useMemo(() => (order ? getPaymentStatus(order) : 'Unpaid'), [order]);
  const itemCount = useMemo(() => (order ? getOrderItemCount(order) : 0), [order]);
  const sellerSummary = useMemo(() => (order ? getSellerSummary(order) : ''), [order]);
  const approvalState = useMemo(() => (order ? getApprovalState(order) : 'Pending'), [order]);

  return (
    <PageSkeleton
      title={isConfirmed ? 'Thank you for your purchase' : 'Order Details'}
      titleId="order-details-title"
    >
      <div className="order-details">
        {isLoading ? <p className="order-details__loading">Loading order...</p> : null}
        {error ? <p className="order-details__notice order-details__notice--error">{error}</p> : null}

        {order ? (
          <>
            <section className="order-details__confirmation" aria-label="Order status">
              <div className="order-details__confirmation-copy">
                <h2>{isConfirmed ? 'Order placed' : 'Order summary'}</h2>
                <div className="order-details__meta">
                  <span>Order {order.orderNumber}</span>
                  <span>{formatDateTime(order.orderPurchaseTimestampUtc)}</span>
                </div>
                <p className="order-details__seller-note">
                  {approvalState === 'Approved'
                    ? `Approved by ${sellerSummary}`
                    : `Waiting for approval from ${sellerSummary}`}
                </p>
              </div>
              <div className="order-details__badges">
                <StatusBadge label={paymentStatus} />
                <StatusBadge label={approvalState} />
              </div>
            </section>

            <section className="order-details__section" aria-labelledby="order-items-title">
              <div className="order-details__section-heading">
                <h2 id="order-items-title">Items</h2>
                <span className="order-details__item-count">
                  {itemCount} item{itemCount === 1 ? '' : 's'}
                </span>
              </div>

              <div className="order-details__line-list">
                {order.items.map((item) => (
                  <article className="order-details__line" key={`${item.orderId}-${item.orderItemId}`}>
                    <div className="order-details__line-product">
                      <span className="order-details__product-media" aria-hidden="true">
                        No image
                      </span>
                      <div>
                        <h3>{item.productName}</h3>
                        <p>
                          Quantity {item.quantity}
                          <span aria-hidden="true"> - </span>
                          Sold by {item.sellerName}
                        </p>
                      </div>
                    </div>
                    <strong>{formatMoney(item.unitPrice * item.quantity, item.currencyCode)}</strong>
                  </article>
                ))}
              </div>

              <dl className="order-details__totals">
                <div>
                  <dt>Subtotal</dt>
                  <dd>{formatMoney(order.subtotalAmount, order.currencyCode)}</dd>
                </div>
                <div>
                  <dt>Shipping</dt>
                  <dd>{formatMoney(order.freightAmount, order.currencyCode)}</dd>
                </div>
                <div>
                  <dt>Total</dt>
                  <dd>{formatMoney(order.totalAmount, order.currencyCode)}</dd>
                </div>
              </dl>
            </section>

            <section className="order-details__section" aria-labelledby="order-followup-title">
              <div className="order-details__section-heading">
                <h2 id="order-followup-title">Status</h2>
              </div>
              <dl className="order-details__status-list">
                <StatusItem isComplete={paymentStatus === 'Paid'} label="Payment" value={paymentStatus} />
                <StatusItem
                  detail={sellerSummary}
                  isComplete={approvalState === 'Approved'}
                  label="Seller approval"
                  value={approvalState}
                />
                <StatusItem
                  isComplete={Boolean(order.orderDeliveredCustomerDateUtc)}
                  label="Delivery"
                  value={getDeliveryState(order)}
                />
              </dl>
            </section>

            <div className="order-details__actions">
              <Link className="order-details__primary-action" to="/products">
                Continue shopping
              </Link>
            </div>
          </>
        ) : null}
      </div>
    </PageSkeleton>
  );
}

function StatusItem({
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
    <div className="order-details__status-item" data-complete={isComplete}>
      <dt>{label}</dt>
      <dd>{value}</dd>
      {detail ? <span>{detail}</span> : null}
    </div>
  );
}

function StatusBadge({ label }: { label: string }) {
  return (
    <span className="order-details__status" data-tone={getStatusTone(label as Parameters<typeof getStatusTone>[0])}>
      {label}
    </span>
  );
}

function getSellerSummary(order: Order) {
  const sellerNames = Array.from(new Set(order.items.map((item) => item.sellerName).filter(Boolean)));
  const firstSeller = sellerNames[0] ?? 'the seller';

  if (sellerNames.length <= 1) {
    return firstSeller;
  }

  return `${firstSeller} and ${sellerNames.length - 1} more`;
}

function getApprovalState(order: Order) {
  if (order.orderApprovedAtUtc || ['Approved', 'Processing', 'Shipped', 'Delivered'].includes(order.orderStatus)) {
    return 'Approved';
  }

  return 'Pending';
}

function getDeliveryState(order: Order) {
  if (order.orderDeliveredCustomerDateUtc) {
    return formatDateTime(order.orderDeliveredCustomerDateUtc);
  }

  if (order.orderDeliveredCarrierDateUtc || order.orderStatus === 'Shipped') {
    return 'In transit';
  }

  return 'Not started';
}
