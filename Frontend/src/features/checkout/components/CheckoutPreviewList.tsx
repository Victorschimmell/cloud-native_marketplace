import { getCurrencyLocale } from '../../../shared/currency/currency';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { CheckoutPreviewLine } from '../types';

interface CheckoutPreviewListProps {
  lines: CheckoutPreviewLine[];
  currency: CurrencyCode;
}

export default function CheckoutPreviewList({ lines, currency }: CheckoutPreviewListProps) {
  const locale = getCurrencyLocale(currency);
  const total = lines.reduce((sum, line) => sum + line.lineTotal, 0);

  return (
    <section className="checkout-page__section checkout-page__section--preview" aria-labelledby="checkout-preview-title">
      <div className="checkout-page__section-heading">
        <div>
          <h2 id="checkout-preview-title" className="checkout-page__section-title">
            Checkout preview
          </h2>
          <p className="checkout-page__section-description">
            Review the cart total before proceeding to payment.
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

      <div className="checkout-page__preview-total">
        <span className="checkout-page__preview-total-label">Preview total</span>
        <strong className="checkout-page__preview-total-value">
          {total.toLocaleString(locale, {
            style: 'currency',
            currency: lines[0]?.currencyCode ?? currency,
          })}
        </strong>
      </div>
    </section>
  );
}
