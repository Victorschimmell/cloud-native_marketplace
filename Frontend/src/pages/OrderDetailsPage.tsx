import { useEffect, useMemo, useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../components/PageSkeleton';
import { useCurrency } from '../shared/currency/useCurrency';
import { orderApi } from '../features/orders/api/orderApi';
import type { Order } from '../features/orders/types';
import { formatDateTime, formatMoney, getOrderItemCount, getPaymentStatus, getStatusTone } from '../features/orders/utils';
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

  return (
    <PageSkeleton
      title={isConfirmed ? 'Thank you' : 'Order Details'}
      summary={isConfirmed ? 'Thank you for your purchase. Your order details are below.' : 'Review order items, totals, payment, and status.'}
      titleId="order-details-title"
    >
      <div className="order-details">
        {isLoading ? <p className="order-details__loading">Loading order...</p> : null}
        {error ? <p className="order-details__notice order-details__notice--error">{error}</p> : null}

        {order ? (
          <>
            <section className="order-details__confirmation" aria-label="Order status">
              <div>
                <div className="order-details__eyebrow">Order {order.orderNumber}</div>
                <h2>{isConfirmed ? 'Purchase complete' : 'Purchase summary'}</h2>
                <p>{formatDateTime(order.orderPurchaseTimestampUtc)}</p>
              </div>
              <div className="order-details__badges">
                <StatusBadge label={paymentStatus} />
                <StatusBadge label={order.orderStatus} />
              </div>
            </section>

            <section className="order-details__section" aria-labelledby="order-items-title">
              <div className="order-details__section-heading">
                <div>
                  <h2 id="order-items-title">Items</h2>
                  <p>
                    {getOrderItemCount(order)} item{getOrderItemCount(order) === 1 ? '' : 's'} in this order.
                  </p>
                </div>
              </div>

              <div className="order-details__line-list">
                {order.items.map((item) => (
                  <article className="order-details__line" key={`${item.orderId}-${item.orderItemId}`}>
                    <div>
                      <h3>{item.productName}</h3>
                      <p>
                        Quantity {item.quantity}
                        <span aria-hidden="true"> - </span>
                        Listing {item.listingId.slice(0, 8)}
                      </p>
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
                <div>
                  <h2 id="order-followup-title">Follow-up</h2>
                  <p>Payment, shipment, and delivery progress for this order.</p>
                </div>
              </div>
              <dl className="order-details__info-grid">
                <InfoBlock label="Payment" value={paymentStatus} />
                <InfoBlock label="Approved" value={formatDateTime(order.orderApprovedAtUtc)} />
                <InfoBlock label="Carrier handoff" value={formatDateTime(order.orderDeliveredCarrierDateUtc)} />
                <InfoBlock label="Delivered" value={formatDateTime(order.orderDeliveredCustomerDateUtc)} />
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

function InfoBlock({ label, value }: { label: string; value: string }) {
  return (
    <div className="order-details__info-block">
      <dt>{label}</dt>
      <dd>{value}</dd>
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
