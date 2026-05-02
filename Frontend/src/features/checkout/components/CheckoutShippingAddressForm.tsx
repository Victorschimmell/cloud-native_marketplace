import type { CheckoutShippingAddress } from '../types';

interface CheckoutShippingAddressFormProps {
  address: CheckoutShippingAddress;
  onAddressChange: (address: CheckoutShippingAddress) => void;
  disabled: boolean;
}

export default function CheckoutShippingAddressForm({ address, disabled, onAddressChange }: CheckoutShippingAddressFormProps) {
  function update<K extends keyof CheckoutShippingAddress>(key: K, value: CheckoutShippingAddress[K]) {
    onAddressChange({
      ...address,
      [key]: value,
    });
  }

  return (
    <section className="checkout-page__section" aria-labelledby="checkout-shipping-title">
      <div className="checkout-page__section-heading">
        <div>
          <h2 id="checkout-shipping-title" className="checkout-page__section-title">
            Shipping information
          </h2>
          <p className="checkout-page__section-description">
            This address will be used for the order.
          </p>
        </div>
      </div>

      <div className="checkout-page__form-grid">
        <TextInput
          autoComplete="address-line1"
          disabled={disabled}
          label="Address"
          onChange={(value) => update('addressLine1', value)}
          required
          value={address.addressLine1}
          wide
        />
        <TextInput
          autoComplete="address-line2"
          disabled={disabled}
          label="Apartment, suite, etc."
          onChange={(value) => update('addressLine2', value)}
          value={address.addressLine2 ?? ''}
          wide
        />
        <TextInput
          autoComplete="address-level2"
          disabled={disabled}
          label="City"
          onChange={(value) => update('city', value)}
          required
          value={address.city}
        />
        <TextInput
          autoComplete="postal-code"
          disabled={disabled}
          label="ZIP code"
          onChange={(value) => update('postalCode', value)}
          required
          value={address.postalCode}
        />
        <TextInput
          autoComplete="address-level1"
          disabled={disabled}
          label="State / region"
          onChange={(value) => update('state', value)}
          required
          value={address.state}
        />
        <TextInput
          autoComplete="country"
          disabled={disabled}
          label="Country code"
          maxLength={3}
          onChange={(value) => update('countryCode', value.toUpperCase())}
          required
          value={address.countryCode}
        />
      </div>
    </section>
  );
}

interface TextInputProps {
  autoComplete: string;
  disabled: boolean;
  label: string;
  maxLength?: number;
  onChange: (value: string) => void;
  required?: boolean;
  value: string;
  wide?: boolean;
}

function TextInput({ autoComplete, disabled, label, maxLength, onChange, required = false, value, wide = false }: TextInputProps) {
  return (
    <label className={wide ? 'checkout-page__field checkout-page__field--wide' : 'checkout-page__field'}>
      {label}
      <input
        autoComplete={autoComplete}
        disabled={disabled}
        maxLength={maxLength}
        onChange={(event) => onChange(event.target.value)}
        required={required}
        type="text"
        value={value}
      />
    </label>
  );
}
