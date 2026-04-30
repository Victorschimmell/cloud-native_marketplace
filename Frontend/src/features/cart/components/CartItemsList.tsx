import CartLine from './CartLine';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { CartItem as CartItemType } from '../types';
import './CartItemsList.css';

interface CartItemsListProps {
  items: CartItemType[];
  updatingItems: Set<string>;
  currency: CurrencyCode;
  onUpdateQuantity: (item: CartItemType, newQuantity: number) => void;
  onRemoveItem: (item: CartItemType) => void;
}

export default function CartItemsList({
  items,
  updatingItems,
  currency,
  onUpdateQuantity,
  onRemoveItem,
}: CartItemsListProps) {
  return (
    <div className="cart-items__section">
      <h2>Cart Items ({items.length})</h2>
      <div className="cart-items__list">
        {items.map((item) => (
          <CartLine
            key={item.id}
            item={item}
            isUpdating={updatingItems.has(item.listingId)}
            currency={currency}
            onUpdateQuantity={onUpdateQuantity}
            onRemoveItem={onRemoveItem}
          />
        ))}
      </div>
    </div>
  );
}
