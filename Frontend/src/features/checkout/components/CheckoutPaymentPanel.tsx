import { getCurrencyLocale } from '../../../shared/currency/currency';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { Currency, PaymentType } from '../types';
import { PaymentType as PaymentTypeValues } from '../types';

interface CheckoutPaymentPanelProps {
  currency: CurrencyCode;
  total: number;
  paymentTotal: number;
  currencyInfo: Currency | null;
  selectedPaymentType: PaymentType | null;
  shippingAddressId: string;
  onPaymentTypeChange: (type: PaymentType) => void;
  onShippingAddressIdChange: (shippingAddressId: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  isSubmitting: boolean;
  isLoadingCurrency: boolean;
  isShippingAddressValid: boolean;
}

const paymentTypeOptions = [
  { value: PaymentTypeValues.CreditCard, label: 'Credit Card' },
  { value: PaymentTypeValues.DebitCard, label: 'Debit Card' },
  { value: PaymentTypeValues.BankTransfer, label: 'Bank Transfer' },
  { value: PaymentTypeValues.Wallet, label: 'Digital Wallet' },
];

export default function CheckoutPaymentPanel({
  currency,
  total,
  paymentTotal,
  currencyInfo,
  selectedPaymentType,
  shippingAddressId,
  onPaymentTypeChange,
  onShippingAddressIdChange,
  onSubmit,
  isSubmitting,
  isLoadingCurrency,
  isShippingAddressValid,
}: CheckoutPaymentPanelProps) {
  const locale = getCurrencyLocale(currency);
  const isFormValid = currencyInfo && selectedPaymentType && total > 0 && paymentTotal > 0 && isShippingAddressValid;
  const isDisabled = isSubmitting || isLoadingCurrency || !isFormValid;

  return (
    <section className="checkout-page__section checkout-page__section--payment" aria-labelledby="checkout-payment-title">
      <div className="checkout-page__section-heading">
        <div>
          <h2 id="checkout-payment-title" className="checkout-page__section-title">
            Payment method
          </h2>
          <p className="checkout-page__section-description">
            Select your preferred payment method to proceed.
          </p>
        </div>
      </div>

      {currencyInfo && (
        <div className="checkout-page__payment-card">
          <div className="checkout-page__payment-card-header">
            <div>
              <div className="checkout-page__payment-card-title">Brazilian Real</div>
              <div className="checkout-page__payment-card-subtitle">Payment currency: {currencyInfo.code}</div>
            </div>
            <span className="checkout-page__payment-card-badge">Default</span>
          </div>

          <dl className="checkout-page__payment-meta">
            <div className="checkout-page__payment-meta-item">
              <dt>Display total</dt>
              <dd>
                {total.toLocaleString(locale, {
                  style: 'currency',
                  currency,
                })}
              </dd>
            </div>
            <div className="checkout-page__payment-meta-item">
              <dt>Payment total</dt>
              <dd>
                {paymentTotal.toLocaleString('pt-BR', {
                  style: 'currency',
                  currency: currencyInfo.code,
                })}
              </dd>
            </div>
          </dl>

          <form className="checkout-page__payment-form" onSubmit={onSubmit}>
            <fieldset className="checkout-page__payment-fieldset" disabled={isSubmitting || isLoadingCurrency}>
              <legend className="checkout-page__payment-legend">Payment method</legend>

              <label className="checkout-page__shipping-field">
                Shipping address ID
                <input
                  aria-invalid={shippingAddressId.length > 0 && !isShippingAddressValid}
                  onChange={(e) => onShippingAddressIdChange(e.target.value)}
                  placeholder="00000000-0000-0000-0000-000000000000"
                  type="text"
                  value={shippingAddressId}
                />
              </label>

              <div className="checkout-page__payment-options">
                {paymentTypeOptions.map((option) => (
                  <label className="checkout-page__payment-option" key={option.value}>
                    <input
                      type="radio"
                      name="paymentType"
                      value={option.value}
                      checked={selectedPaymentType === option.value}
                      onChange={(e) => onPaymentTypeChange(e.target.value as PaymentType)}
                      className="checkout-page__payment-radio"
                    />
                    <span className="checkout-page__payment-option-label">{option.label}</span>
                  </label>
                ))}
              </div>

              <p className="checkout-page__payment-note">
                Installments are fixed at 1 for all payment methods.
              </p>

              <button
                className="checkout-page__payment-action"
                type="submit"
                disabled={isDisabled}
              >
                {isSubmitting ? 'Processing...' : 'Finalize checkout'}
              </button>
            </fieldset>
          </form>
        </div>
      )}

      {isLoadingCurrency && (
        <p className="checkout-page__payment-loading">Loading payment options...</p>
      )}
    </section>
  );
}
