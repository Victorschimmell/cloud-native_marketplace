import type { CheckoutCustomerProfile } from '../types';

interface CheckoutCustomerPanelProps {
  customer: CheckoutCustomerProfile | null;
  email: string;
  isLoading: boolean;
}

export default function CheckoutCustomerPanel({ customer, email, isLoading }: CheckoutCustomerPanelProps) {
  return (
    <section className="checkout-page__section" aria-labelledby="checkout-customer-title">
      <div className="checkout-page__section-heading">
        <div>
          <h2 id="checkout-customer-title" className="checkout-page__section-title">
            Customer information
          </h2>
          <p className="checkout-page__section-description">
            Used from your account.
          </p>
        </div>
      </div>

      <div className="checkout-page__readonly-grid" aria-busy={isLoading}>
        <ReadOnlyField label="First name" value={customer?.firstName ?? ''} />
        <ReadOnlyField label="Last name" value={customer?.lastName ?? ''} />
        <ReadOnlyField label="Email" value={email} wide />
        <ReadOnlyField label="Phone" value={customer?.phone ?? ''} wide />
      </div>
    </section>
  );
}

function ReadOnlyField({ label, value, wide = false }: { label: string; value: string; wide?: boolean }) {
  const fieldValue = value || 'Loading...';

  return (
    <label className={wide ? 'checkout-page__readonly-field checkout-page__readonly-field--wide' : 'checkout-page__readonly-field'}>
      <span>{label}</span>
      <input readOnly value={fieldValue} />
    </label>
  );
}
