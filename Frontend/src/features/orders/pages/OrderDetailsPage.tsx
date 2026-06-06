import { useEffect, useMemo, useState } from 'react';
import { Link, useLocation, useParams, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { FormNotice, TextAreaField, TextField } from '../../../shared/forms';
import { orderApi } from '../api/orderApi';
import { ShipmentTrackingBox, OrderDetailsSkeleton } from '../components';
import type { Order } from '../types';
import { formatDateTime, formatMoney, getOrderItemCount, getPaymentStatus, getStatusTone } from '../utils';
import './OrderDetailsPage.css';

const cancelAllowedStatuses = ['Pending', 'Approved', 'Processing'] as const;

export default function OrderDetailsPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const { currency } = useCurrency();
  const [order, setOrder] = useState<Order | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reviewingItemId, setReviewingItemId] = useState<number | null>(null);
  const [reviewScore, setReviewScore] = useState(5);
  const [reviewTitle, setReviewTitle] = useState('');
  const [reviewMessage, setReviewMessage] = useState('');
  const [reviewError, setReviewError] = useState<string | null>(null);
  const [isSubmittingReview, setIsSubmittingReview] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [cancelError, setCancelError] = useState<string | null>(null);
  const [cancelSuccess, setCancelSuccess] = useState<string | null>(null);
  const [isCancelSubmitting, setIsCancelSubmitting] = useState(false);
  const [returnReason, setReturnReason] = useState('');
  const [returnSubmittedReason, setReturnSubmittedReason] = useState<string | null>(null);
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

  useEffect(() => {
    if (!location.hash || isLoading) {
      return;
    }

    const targetId = location.hash.replace('#', '').trim();
    if (!targetId) {
      return;
    }

    const element = document.getElementById(targetId);
    if (!element) {
      return;
    }

    element.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }, [isLoading, location.hash]);

  const paymentStatus = useMemo(() => (order ? getPaymentStatus(order) : 'Unpaid'), [order]);
  const itemCount = useMemo(() => (order ? getOrderItemCount(order) : 0), [order]);
  const sellerSummary = useMemo(() => (order ? getSellerSummary(order) : ''), [order]);
  const approvalState = useMemo(() => (order ? getApprovalState(order) : 'Pending'), [order]);
  const reviewedItemIds = useMemo(() => new Set(order?.reviews.map((review) => review.orderItemId).filter(isNumber) ?? []), [order]);
  const reviewedProductIds = useMemo(() => new Set(order?.reviews.map((review) => review.productId).filter(isString) ?? []), [order]);
  const canReviewOrder = Boolean(order && isReviewable(order));

  const canCancel = useMemo(
    () => (order ? cancelAllowedStatuses.includes(order.orderStatus as (typeof cancelAllowedStatuses)[number]) : false),
    [order],
  );
  const canReturn = useMemo(() => order?.orderStatus === 'Delivered', [order]);
  const showShipments = useMemo(
    () => Boolean(order && order.shipments.length > 0 && ['Shipped', 'Delivered'].includes(order.orderStatus)),
    [order],
  );
  const cancelReasonLength = cancelReason.trim().length;
  const returnReasonLength = returnReason.trim().length;
  const cancelSummaryReason = useMemo(() => {
    if (order?.orderStatus === 'Cancelled') {
      return order.orderStatusDescription?.trim() ?? '';
    }

    return '';
  }, [order]);
  const returnSummaryReason = useMemo(() => {
    if (order?.orderStatus === 'Returned') {
      return order.orderStatusDescription?.trim() ?? '';
    }

    return returnSubmittedReason?.trim() ?? '';
  }, [order, returnSubmittedReason]);

  async function submitReview(orderItemId: number) {
    if (!order) {
      return;
    }

    try {
      setIsSubmittingReview(true);
      setReviewError(null);
      const review = await orderApi.createReview({
        orderId: order.id,
        orderItemId,
        reviewScore,
        reviewCommentTitle: reviewTitle.trim() || undefined,
        reviewCommentMessage: reviewMessage.trim() || undefined,
      });

      setOrder({
        ...order,
        reviews: [...order.reviews, review],
      });
      setReviewingItemId(null);
      setReviewScore(5);
      setReviewTitle('');
      setReviewMessage('');
    } catch (requestError) {
      setReviewError(requestError instanceof Error ? requestError.message : 'Could not submit this review.');
    } finally {
      setIsSubmittingReview(false);
    }
  }

  const handleCancelSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!order) {
      return;
    }

    const trimmedReason = cancelReason.trim();
    if (!trimmedReason) {
      setCancelError('Please provide a reason for cancellation.');
      return;
    }

    try {
      setIsCancelSubmitting(true);
      setCancelError(null);
      setCancelSuccess(null);

      const response = await orderApi.cancelOrder(order.id, currency, trimmedReason);
      setOrder(response);
      setCancelReason('');
      setCancelSuccess('Order cancelled successfully.');
    } catch (requestError) {
      setCancelError(`Cancel order failed: ${getErrorMessage(requestError)}`);
    } finally {
      setIsCancelSubmitting(false);
    }
  };

  const handleReturnSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const trimmedReason = returnReason.trim();
    if (!trimmedReason) {
      return;
    }

    setReturnSubmittedReason(trimmedReason);
    setReturnReason('');
    setOrder((currentOrder) =>
      currentOrder
        ? {
          ...currentOrder,
          orderStatusDescription: trimmedReason,
        }
        : currentOrder,
    );
  };

  return (
    <PageSkeleton
      title={isConfirmed ? 'Thank you for your purchase' : 'Order Details'}
      titleId="order-details-title"
    >
      <div className="order-details">
        {isLoading ? <OrderDetailsSkeleton /> : null}
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
                <StatusBadge label={order.orderStatus} />
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
                        {item.imageUrl ? (
                          <img alt="" src={item.imageUrl} />
                        ) : (
                          'No image'
                        )}
                      </span>
                      <div>
                        <h3>
                          <Link
                            className="order-details__product-link"
                            to={`/products/${item.productId}?listingId=${item.listingId}`}
                          >
                            {item.productName}
                          </Link>
                        </h3>
                        <p>
                          Quantity {item.quantity}
                          <span aria-hidden="true"> - </span>
                          Sold by {item.sellerName}
                          <br/>
                          Item Status - {item.fulfillmentStatus}
                        </p>
                      </div>
                    </div>
                    <div className="order-details__line-side">
                      <strong>{formatMoney(item.lineTotal, item.currencyCode)}</strong>
                      {reviewedItemIds.has(item.orderItemId) || reviewedProductIds.has(item.productId) ? (
                        <span className="order-details__review-state">Reviewed</span>
                      ) : canReviewOrder ? (
                        <button
                          className="order-details__review-button"
                          onClick={() => {
                            setReviewingItemId(item.orderItemId);
                            setReviewError(null);
                          }}
                          type="button"
                        >
                          Review product
                        </button>
                      ) : null}
                    </div>
                    {reviewingItemId === item.orderItemId ? (
                      <form
                        className="order-details__review-form"
                        onSubmit={(event) => {
                          event.preventDefault();
                          void submitReview(item.orderItemId);
                        }}
                      >
                        <div className="order-details__review-form-header">
                          <h3>Review product</h3>
                          <span>{item.productName}</span>
                        </div>

                        <fieldset className="order-details__rating-field" disabled={isSubmittingReview}>
                          <legend>Rating</legend>
                          <div className="order-details__rating-options">
                            {[1, 2, 3, 4, 5].map((score) => (
                              <button
                                aria-pressed={reviewScore === score}
                                aria-label={`${score} ${score === 1 ? 'star' : 'stars'}`}
                                className="order-details__rating-option"
                                data-active={score <= reviewScore}
                                key={score}
                                onClick={() => setReviewScore(score)}
                                type="button"
                              >
                                <span aria-hidden="true">★</span>
                              </button>
                            ))}
                          </div>
                        </fieldset>

                        <div className="order-details__review-title-field">
                          <TextField
                            className="order-details__review-control"
                            disabled={isSubmittingReview}
                            label="Title"
                            maxLength={200}
                            onChange={(event) => setReviewTitle(event.target.value)}
                            placeholder="Short summary"
                            value={reviewTitle}
                          />
                        </div>
                        <div className="order-details__review-form-message">
                          <TextAreaField
                            className="order-details__review-control order-details__review-feedback"
                            disabled={isSubmittingReview}
                            label="Feedback"
                            maxLength={2000}
                            onChange={(event) => setReviewMessage(event.target.value)}
                            placeholder="What should future customers know?"
                            rows={3}
                            value={reviewMessage}
                          />
                        </div>
                        {reviewError ? (
                          <div className="order-details__review-form-notice">
                            <FormNotice variant="error">{reviewError}</FormNotice>
                          </div>
                        ) : null}
                        <div className="order-details__review-actions">
                          <button
                            className="order-details__review-action order-details__review-action--primary"
                            disabled={isSubmittingReview}
                            type="submit"
                          >
                            {isSubmittingReview ? 'Submitting...' : 'Submit review'}
                          </button>
                          <button
                            className="order-details__review-action order-details__review-action--secondary"
                            disabled={isSubmittingReview}
                            onClick={() => setReviewingItemId(null)}
                            type="button"
                          >
                            Cancel
                          </button>
                        </div>
                      </form>
                    ) : null}
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

            {showShipments ? (
              <section className="order-details__section" aria-labelledby="order-shipments-title">
                <div className="order-details__section-heading">
                  <h2 id="order-shipments-title">Shipments</h2>
                </div>
                <div className="order-details__shipments">
                  {order.shipments.map((shipment) => (
                    <ShipmentTrackingBox key={shipment.id} shipment={shipment} />
                  ))}
                </div>
              </section>
            ) : null}

            {cancelSummaryReason ? (
              <section className="order-details__section" id="cancel" aria-labelledby="order-cancel-summary-title">
                <div className="order-details__section-heading">
                  <h2 id="order-cancel-summary-title">Cancel reason</h2>
                </div>
                <div className="order-details__reason-box">
                  <p className="order-details__reason">{cancelSummaryReason}</p>
                </div>
              </section>
            ) : canCancel ? (
              <section className="order-details__section" id="cancel" aria-labelledby="order-cancel-title">
                <div className="order-details__section-heading">
                  <h2 id="order-cancel-title">Cancel order</h2>
                </div>
                <form className="order-details__form" onSubmit={handleCancelSubmit}>
                  <label className="order-details__field">
                    <span>Reason</span>
                    <textarea
                      maxLength={500}
                      onChange={(event) => setCancelReason(event.target.value)}
                      placeholder="Tell us why you need to cancel"
                      value={cancelReason}
                    />
                    <div className="order-details__field-meta">
                      <span>Max 500 characters.</span>
                      <span>{cancelReasonLength}/500</span>
                    </div>
                  </label>
                  {cancelError ? <p className="order-details__notice order-details__notice--error">{cancelError}</p> : null}
                  {cancelSuccess ? (
                    <p className="order-details__notice order-details__notice--success">{cancelSuccess}</p>
                  ) : null}
                  <div className="order-details__form-actions">
                    <button
                      className="order-details__action order-details__action--danger"
                      disabled={isCancelSubmitting || cancelReasonLength === 0}
                      type="submit"
                    >
                      {isCancelSubmitting ? 'Cancelling...' : 'Cancel order'}
                    </button>
                  </div>
                </form>
              </section>
            ) : null}

            {returnSummaryReason ? (
              <section className="order-details__section" id="return" aria-labelledby="order-return-summary-title">
                <div className="order-details__section-heading">
                  <h2 id="order-return-summary-title">Return reason</h2>
                </div>
                <div className="order-details__reason-box">
                  <p className="order-details__reason">{returnSummaryReason}</p>
                </div>
              </section>
            ) : canReturn ? (
              <section className="order-details__section" id="return" aria-labelledby="order-return-title">
                <div className="order-details__section-heading">
                  <h2 id="order-return-title">Request return</h2>
                </div>
                <form className="order-details__form" onSubmit={handleReturnSubmit}>
                  <label className="order-details__field">
                    <span>Reason</span>
                    <textarea
                      maxLength={500}
                      onChange={(event) => setReturnReason(event.target.value)}
                      placeholder="Tell us why you want to return this order"
                      value={returnReason}
                    />
                    <div className="order-details__field-meta">
                      <span>Max 500 characters.</span>
                      <span>{returnReasonLength}/500</span>
                    </div>
                  </label>
                  <div className="order-details__form-actions">
                    <button
                      className="order-details__action order-details__action--danger"
                      disabled={returnReasonLength === 0}
                      type="submit"
                    >
                      Request return
                    </button>
                  </div>
                </form>
              </section>
            ) : null}

            <div className="order-details__actions">
              <Link className="order-details__primary-action" to="/products">
                Continue shopping
              </Link>
              <Link className="order-details__secondary-action" to="/orders">
                View orders
              </Link>
            </div>
          </>
        ) : null}
      </div>
    </PageSkeleton>
  );
}

function getErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const payload = err.payload as Record<string, unknown>;
    if (payload && typeof payload === 'object') {
      if ('error' in payload) {
        return String(payload.error);
      }
      if ('message' in payload) {
        return String(payload.message);
      }
    }

    return JSON.stringify(payload);
  }

  return err instanceof Error ? err.message : 'Unknown error';
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

function isReviewable(order: Order) {
  return order.orderStatus === 'Delivered' || order.orderStatus === 'Returned';
}

function isNumber(value: unknown): value is number {
  return typeof value === 'number';
}

function isString(value: unknown): value is string {
  return typeof value === 'string';
}
