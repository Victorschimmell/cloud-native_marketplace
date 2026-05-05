import { getCurrencyLocale } from '../../../shared/currency/currency';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { Currency } from '../types';

interface CheckoutPaymentPanelProps {
  currency: CurrencyCode;
  total: number;
  paymentTotal: number;
  currencyInfo: Currency | null;
  isLoadingCurrency: boolean;
}

export default function CheckoutPaymentPanel({
  currency,
  total,
  paymentTotal,
  currencyInfo,
  isLoadingCurrency,
}: CheckoutPaymentPanelProps) {
  const locale = getCurrencyLocale(currency);
  const formattedDisplayTotal = total.toLocaleString(locale, {
    style: 'currency',
    currency,
  });
  const formattedPaymentTotal = currencyInfo
    ? paymentTotal.toLocaleString('pt-BR', {
        style: 'currency',
        currency: currencyInfo.code,
      })
    : '';
  const showChargedCurrencyDisclosure = currencyInfo?.code !== currency;

  return (
    <section className="checkout-page__section checkout-page__section--payment" aria-labelledby="checkout-payment-title">
      <div className="checkout-page__section-heading">
        <div>
          <h2 id="checkout-payment-title" className="checkout-page__section-title">
            Payment method
          </h2>
          <p className="checkout-page__section-description">
            Credit card is currently the supported checkout method.
          </p>
        </div>
      </div>

      {currencyInfo && (
        <div className="checkout-page__payment-card">
          <div className="checkout-page__payment-card-header">
            <div>
              <div className="checkout-page__payment-card-title">Credit card</div>
              <div className="checkout-page__payment-card-subtitle">Charged in {currencyInfo.code}</div>
            </div>
            <span className="checkout-page__payment-card-badge">Selected</span>
          </div>

          <dl className="checkout-page__payment-meta">
            <div className="checkout-page__payment-meta-item">
              <dt>Display total</dt>
              <dd>{formattedDisplayTotal}</dd>
            </div>
          </dl>

          <p className="checkout-page__payment-note">
            Installments are fixed at 1.
            {showChargedCurrencyDisclosure && ` Your card will be charged ${formattedPaymentTotal} in ${currencyInfo.code}.`}
            {' '}Card details are handled by the marketplace payment flow.
          </p>
        </div>
      )}

      {isLoadingCurrency && (
        <p className="checkout-page__payment-loading">Loading payment options...</p>
      )}
    </section>
  );
}
