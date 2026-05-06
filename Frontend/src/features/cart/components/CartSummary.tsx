import { Link } from 'react-router-dom';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import type { CurrencyCode } from '../../../shared/currency/currency';
import './CartSummary.css';

interface CartSummaryProps {
  total: number;
  currency: CurrencyCode;
}

export default function CartSummary({
  total,
  currency,
}: CartSummaryProps) {
  const locale = getCurrencyLocale(currency);
  const formattedTotal = total.toLocaleString(locale, {
    style: 'currency',
    currency,
  });

  return (
    <aside className="cart-summary">
      <div className="cart-summary__header">
        <h3>Order Summary</h3>
      </div>

      <div className="cart-summary__row">
        <span>Subtotal</span>
        <span>{formattedTotal}</span>
      </div>

      <div className="cart-summary__row">
        <span>Shipping</span>
        <span>Calculated at checkout</span>
      </div>

      <div className="cart-summary__row total">
        <span>Total</span>
        <span>{formattedTotal}</span>
      </div>

      <Link className="cart-summary__checkout-link" to="/checkout">
        Go to checkout
      </Link>

      <Link className="cart-summary__continue-shopping-link" to="/products">
        Continue shopping
      </Link>
    </aside>
  );
}
