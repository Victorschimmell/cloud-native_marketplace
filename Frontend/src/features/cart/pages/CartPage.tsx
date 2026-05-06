import { useEffect, useMemo, useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import StatusMessage from '../../../shared/components/StatusMessage';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { cartApi } from '../api/cartApi';
import { ApiError } from '../../../shared/api/request';
import { CartEmpty, CartItemsList, CartSummary } from '../components';
import type { Cart, CartItem } from '../types';
import './CartPage.css';

export default function CartPage() {
  const [isLoading, setIsLoading] = useState(true);
  const [cart, setCart] = useState<Cart | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [updatingItems, setUpdatingItems] = useState<Set<string>>(new Set());
  const { currency } = useCurrency();

  const getErrorMessage = (err: unknown): string => {
    if (err instanceof ApiError) {
      const payload = err.payload as Record<string, unknown>;
      if (payload && typeof payload === 'object') {
        if ('error' in payload) {
          return String(payload.error);
        }
        if ('message' in payload) {
          return String(payload.message);
        }
      }
      return JSON.stringify(payload);
    }
    return 'Unknown error';
  };

  useEffect(() => {
    const loadCart = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const cartData = await cartApi.getCart(currency);
        setCart(cartData);
      } catch (err) {
        setError(`Failed to load cart: ${getErrorMessage(err)}`);
        console.error('Error loading cart:', err);
      } finally {
        setIsLoading(false);
      }
    };

    loadCart();
  }, [currency]);

  const { itemCount, total } = useMemo(() => {
    if (!cart?.items) return { itemCount: 0, total: 0 };

    const total = cart.items.reduce((sum, item) => {
      return sum + item.lineTotal;
    }, 0);
    const itemCount = cart.items.reduce((sum, item) => sum + item.quantity, 0);

    return { itemCount, total };
  }, [cart?.items]);

  const handleUpdateQuantity = async (item: CartItem, newQuantity: number) => {
    if (newQuantity <= 0) {
      await handleRemoveItem(item);
      return;
    }

    try {
      setUpdatingItems((prev) => new Set(prev).add(item.listingId));
      const updatedCart = await cartApi.updateItem(item.listingId, newQuantity, currency);
      setCart(updatedCart);
      setError(null);
    } catch (err) {
      setError(`Failed to update item quantity: ${getErrorMessage(err)}`);
    } finally {
      setUpdatingItems((prev) => {
        const next = new Set(prev);
        next.delete(item.listingId);
        return next;
      });
    }
  };

  const handleRemoveItem = async (item: CartItem) => {
    try {
      setUpdatingItems((prev) => new Set(prev).add(item.listingId));
      const updatedCart = await cartApi.removeItem(item.listingId, currency);
      setCart(updatedCart);
      setError(null);
    } catch (err) {
      setError(`Failed to remove item: ${getErrorMessage(err)}`);
    } finally {
      setUpdatingItems((prev) => {
        const next = new Set(prev);
        next.delete(item.listingId);
        return next;
      });
    }
  };

  if (isLoading) {
    return (
      <PageSkeleton
        summary="Loading cart..."
        title="Shopping Cart"
        titleId="cart-page-title"
      >
        <div className="cart-loading">Loading your cart...</div>
      </PageSkeleton>
    );
  }

  const isEmpty = !cart?.items || cart.items.length === 0;
  const pageSummary = isEmpty
    ? 'Your cart is ready when you are.'
    : `${itemCount} ${itemCount === 1 ? 'item' : 'items'} in your cart`;

  return (
    <PageSkeleton
      summary={pageSummary}
      title="Shopping Cart"
      titleId="cart-page-title"
    >
      {error && (
        <StatusMessage variant="error">
          {error}
        </StatusMessage>
      )}

      <div className="cart-container">
        {isEmpty ? (
          <CartEmpty />
        ) : (
          <div className="cart-content">
            <CartItemsList
              items={cart!.items}
              itemCount={itemCount}
              updatingItems={updatingItems}
              currency={currency}
              onUpdateQuantity={handleUpdateQuantity}
              onRemoveItem={handleRemoveItem}
            />

            <CartSummary
              total={total}
              currency={currency}
            />
          </div>
        )}
      </div>
    </PageSkeleton>
  );
}
