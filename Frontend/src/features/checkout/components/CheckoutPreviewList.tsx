import { Link } from 'react-router-dom';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { CheckoutPreview } from '../types';

interface CheckoutPreviewListProps {
  preview: CheckoutPreview;
  currency: CurrencyCode;
  formId: string;
  isDisabled: boolean;
  isSubmitting: boolean;
}

export default function CheckoutPreviewList({ currency, formId, isDisabled, isSubmitting, preview }: CheckoutPreviewListProps) {
  const locale = getCurrencyLocale(currency);
  const lines = preview.lines;

  return (
    <aside className="checkout-page__section checkout-page__summary" aria-labelledby="checkout-summary-title">
      <div className="checkout-page__section-heading">
        <div>
          <h2 id="checkout-summary-title" className="checkout-page__section-title">
            Order summary
          </h2>
          <p className="checkout-page__section-description">
            Review totals before placing the order.
          </p>
        </div>
        <div className="checkout-page__section-badge">
          {lines.length} item{lines.length === 1 ? '' : 's'}
        </div>
      </div>

      <div className="checkout-page__preview-list">
        {lines.map((line) => (
          <article className="checkout-page__preview-line" key={line.listingId}>
            <div className="checkout-page__preview-line-main">
              <div className="checkout-page__preview-line-label">Listing ID</div>
              <div className="checkout-page__preview-line-value">{line.listingId}</div>
            </div>

            <dl className="checkout-page__preview-line-meta">
              <div className="checkout-page__preview-line-meta-item">
                <dt>Quantity</dt>
                <dd>{line.quantity}</dd>
              </div>
              <div className="checkout-page__preview-line-meta-item">
                <dt>Unit price</dt>
                <dd>
                  {line.unitPrice.toLocaleString(locale, {
                    style: 'currency',
                    currency: line.currencyCode,
                  })}
                </dd>
              </div>
              <div className="checkout-page__preview-line-meta-item checkout-page__preview-line-meta-item--total">
                <dt>Line total</dt>
                <dd>
                  {line.lineTotal.toLocaleString(locale, {
                    style: 'currency',
                    currency: line.currencyCode,
                  })}
                </dd>
              </div>
            </dl>
          </article>
        ))}
      </div>

      <dl className="checkout-page__summary-totals">
        <div>
          <dt>Subtotal</dt>
          <dd>
            {preview.subtotalAmount.toLocaleString(locale, {
              style: 'currency',
              currency: preview.currencyCode,
            })}
          </dd>
        </div>
        <div>
          <dt>Shipping</dt>
          <dd>
            {preview.freightAmount.toLocaleString(locale, {
              style: 'currency',
              currency: preview.currencyCode,
            })}
          </dd>
        </div>
        <div className="checkout-page__summary-total">
          <dt>Total</dt>
          <dd>
            {preview.totalAmount.toLocaleString(locale, {
              style: 'currency',
              currency: preview.currencyCode,
            })}
          </dd>
        </div>
      </dl>

      <div className="checkout-page__summary-actions">
        <button
          className="checkout-page__primary-action"
          type="submit"
          form={formId}
          disabled={isDisabled}
        >
          {isSubmitting ? 'Processing...' : 'Place order'}
        </button>
        <Link className="checkout-page__secondary-action" to="/cart">
          Back to cart
        </Link>
      </div>
    </aside>
  );
}
