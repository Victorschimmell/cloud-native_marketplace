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

    return (
        <aside className="cart-summary">
            <h3>Order Summary</h3>

            {/* <div className="summary-row freight-fee">
                <span>Freight Fee:</span>
                <span>
                    {freightFee.toLocaleString(locale, {
                        style: 'currency',
                        currency,
                    })}
                </span>
            </div> */}

            <div className="cart-summary__row total">
                <span>Total:</span>
                <span>
                    {total.toLocaleString(locale, {
                        style: 'currency',
                        currency,
                    })}
                </span>
            </div>

            <Link className="cart-summary__checkout-link" to="/checkout">
                Proceed to Checkout
            </Link>

            <Link className="cart-summary__continue-shopping-link" to="/products">
                Back to products
            </Link>
        </aside>
    );
}
