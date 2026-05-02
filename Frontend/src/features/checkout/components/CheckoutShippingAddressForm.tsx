import type { CheckoutShippingAddress } from '../types';

interface CheckoutShippingAddressFormProps {
  address: CheckoutShippingAddress;
  onAddressChange: (address: CheckoutShippingAddress) => void;
  disabled: boolean;
  showValidation: boolean;
}

export default function CheckoutShippingAddressForm({ address, disabled, onAddressChange, showValidation }: CheckoutShippingAddressFormProps) {
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
          invalid={showValidation && !address.addressLine1.trim()}
          label="Address"
          onChange={(value) => update('addressLine1', value)}
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
          invalid={showValidation && !address.city.trim()}
          label="City"
          onChange={(value) => update('city', value)}
          value={address.city}
        />
        <TextInput
          autoComplete="postal-code"
          disabled={disabled}
          invalid={showValidation && !address.postalCode.trim()}
          label="ZIP code"
          onChange={(value) => update('postalCode', value)}
          value={address.postalCode}
        />
        <TextInput
          autoComplete="address-level1"
          disabled={disabled}
          invalid={showValidation && !address.state.trim()}
          label="State / region"
          onChange={(value) => update('state', value)}
          value={address.state}
        />
        <TextInput
          autoComplete="country"
          disabled={disabled}
          invalid={showValidation && !address.countryCode.trim()}
          label="Country code"
          maxLength={3}
          onChange={(value) => update('countryCode', value.toUpperCase())}
          value={address.countryCode}
        />
      </div>
    </section>
  );
}

interface TextInputProps {
  autoComplete: string;
  disabled: boolean;
  invalid?: boolean;
  label: string;
  maxLength?: number;
  onChange: (value: string) => void;
  value: string;
  wide?: boolean;
}

function TextInput({ autoComplete, disabled, invalid = false, label, maxLength, onChange, value, wide = false }: TextInputProps) {
  return (
    <label className={wide ? 'checkout-page__field checkout-page__field--wide' : 'checkout-page__field'}>
      {label}
      <input
        aria-invalid={invalid}
        autoComplete={autoComplete}
        disabled={disabled}
        maxLength={maxLength}
        onChange={(event) => onChange(event.target.value)}
        type="text"
        value={value}
      />
    </label>
  );
}
