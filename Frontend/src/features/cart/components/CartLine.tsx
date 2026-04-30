import { getCurrencyLocale } from '../../../shared/currency/currency';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { CartItem as CartItemType } from '../types';
import { productApi } from '../../products/api/productApi';
import './CartLine.css';

interface CartLineProps {
  item: CartItemType;
  isUpdating: boolean;
  currency: CurrencyCode;
  onUpdateQuantity: (item: CartItemType, newQuantity: number) => void;
  onRemoveItem: (item: CartItemType) => void;
}

export default function CartLine({
  item,
  isUpdating,
  currency,
  onUpdateQuantity,
  onRemoveItem,
}: CartLineProps) {
  const locale = getCurrencyLocale(currency);

  // TODO: No method to get product name using listingId currently
  return (
    <div className="cart-line">
      <div className="cart-line__info">
        <div className="cart-line__listing-id">Listing ID: {item.listingId}</div>
        <div className="cart-line__price">
          {item.unitPriceAtAddition.toLocaleString(locale, {
            style: 'currency',
            currency: item.currencyCode,
          })}
        </div>
      </div>

      <div className="cart-line__quantity">
        <label htmlFor={`qty-${item.id}`}>Quantity:</label>
        <input
          id={`qty-${item.id}`}
          type="number"
          min="1"
          value={item.quantity}
          onChange={(e) => onUpdateQuantity(item, parseInt(e.target.value) || 1)}
          disabled={isUpdating}
          className="cart-line__qty-input"
        />
      </div>

      <div className="cart-line__subtotal">
        Subtotal:{' '}
        {(item.unitPriceAtAddition * item.quantity).toLocaleString(locale, {
          style: 'currency',
          currency: item.currencyCode,
        })}
      </div>

      <button
        onClick={() => onRemoveItem(item)}
        disabled={isUpdating}
        className="cart-line__remove-btn"
      >
        Remove
      </button>
    </div>
  );
}
