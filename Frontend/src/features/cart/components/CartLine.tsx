import { getCurrencyLocale } from '../../../shared/currency/currency';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { CartItem as CartItemType } from '../types';
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
  const formattedUnitPrice = item.unitPriceAtAddition.toLocaleString(locale, {
    style: 'currency',
    currency: item.currencyCode,
  });
  const formattedLineTotal = item.lineTotal.toLocaleString(locale, {
    style: 'currency',
    currency: item.currencyCode,
  });

  function updateQuantity(value: string) {
    const nextQuantity = Number(value);
    onUpdateQuantity(item, Number.isFinite(nextQuantity) && nextQuantity >= 1 ? nextQuantity : 1);
  }

  return (
    <div className="cart-line">
      <div className="cart-line__product">
        <span className="cart-line__media" aria-hidden="true">
          {item.imageUrl ? (
            <img alt="" src={item.imageUrl} />
          ) : (
            'No image'
          )}
        </span>
        <div className="cart-line__info">
          <div className="cart-line__product-name">{item.productName}</div>
          <div className="cart-line__detail">
            {formattedUnitPrice} each
          </div>
          <button
            aria-label={`Remove ${item.productName} from cart`}
            className="cart-line__remove-btn"
            disabled={isUpdating}
            onClick={() => onRemoveItem(item)}
            type="button"
          >
            Remove item
          </button>
        </div>
      </div>

      <div className="cart-line__price">
        {formattedUnitPrice}
      </div>

      <div className="cart-line__quantity">
        <label htmlFor={`qty-${item.id}`}>
          Quantity
        </label>
        <input
          className="cart-line__qty-input"
          disabled={isUpdating}
          id={`qty-${item.id}`}
          min="1"
          onChange={(event) => updateQuantity(event.target.value)}
          type="number"
          value={item.quantity}
        />
      </div>

      <div className="cart-line__subtotal">
        {formattedLineTotal}
      </div>
    </div>
  );
}
