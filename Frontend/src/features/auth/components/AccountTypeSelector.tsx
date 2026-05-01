import type { AccountType } from '../types';

interface AccountTypeSelectorProps {
  value: AccountType;
  onChange: (value: AccountType) => void;
}

export function AccountTypeSelector({ onChange, value }: AccountTypeSelectorProps) {
  return (
    <fieldset className="auth-form__account-type">
      <legend>Account type</legend>
      <label data-selected={value === 'customer'}>
        <input checked={value === 'customer'} name="accountType" onChange={() => onChange('customer')} type="radio" />
        <span>Customer</span>
      </label>
      <label data-selected={value === 'seller'}>
        <input checked={value === 'seller'} name="accountType" onChange={() => onChange('seller')} type="radio" />
        <span>Seller</span>
      </label>
    </fieldset>
  );
}
