import CartLine from './CartLine';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { CartItem as CartItemType } from '../types';
import './CartItemsList.css';

interface CartItemsListProps {
  items: CartItemType[];
  itemCount: number;
  updatingItems: Set<string>;
  currency: CurrencyCode;
  onUpdateQuantity: (item: CartItemType, newQuantity: number) => void;
  onRemoveItem: (item: CartItemType) => void;
}

export default function CartItemsList({
  items,
  itemCount,
  updatingItems,
  currency,
  onUpdateQuantity,
  onRemoveItem,
}: CartItemsListProps) {
  return (
    <div className="cart-items__section">
      <div className="cart-items__header">
        <h2>Products</h2>
        <span>{itemCount} {itemCount === 1 ? 'item' : 'items'}</span>
      </div>
      <div className="cart-items__columns" aria-hidden="true">
        <span>Product</span>
        <span>Price</span>
        <span>Quantity</span>
        <span>Total</span>
      </div>
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
