import { ApiError, request } from '../../../shared/api/request';
import type { Cart } from '../types';

const cartIdStorageKey = 'marketplace.checkout.cartId';
const cartUpdatedEventName = 'marketplace:cart-updated';

function getEmptyCart(): Cart {
  return {
    id: '',
    userId: null,
    sessionId: null,
    status: 'empty',
    expiresAtUtc: '',
    items: [],
  } as Cart;
}

function clearStoredCartId() {
  window.localStorage.removeItem(cartIdStorageKey);
  notifyCartUpdated(getEmptyCart());
}

function shouldClearStoredCartId(error: unknown) {
  return error instanceof ApiError && (error.status === 403 || error.status === 404);
}

function notifyCartUpdated(cart: Cart) {
  const itemCount = cart.items.reduce((sum, item) => sum + item.quantity, 0);
  window.dispatchEvent(new CustomEvent(cartUpdatedEventName, { detail: { itemCount } }));
}

export function subscribeToCartUpdates(callback: (itemCount: number) => void) {
  function handleCartUpdated(event: Event) {
    const customEvent = event as CustomEvent<{ itemCount?: number }>;
    callback(customEvent.detail?.itemCount ?? 0);
  }

  window.addEventListener(cartUpdatedEventName, handleCartUpdated);

  return () => {
    window.removeEventListener(cartUpdatedEventName, handleCartUpdated);
  };
}

export const cartApi = {
  getCart: async (displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);

    const path = cartId
      ? `/api/cart/${cartId}?displayCurrency=${displayCurrency}`
      : `/api/cart/current?displayCurrency=${displayCurrency}`;

    try {
      const cart = await request<Cart>(path);
      window.localStorage.setItem(cartIdStorageKey, cart.id);
      notifyCartUpdated(cart);
      return cart;
    } catch (error) {
      if (shouldClearStoredCartId(error)) {
        clearStoredCartId();
        return getEmptyCart();
      }

      throw error;
    }
  },

  addItem: async (listingId: string, quantity: number, displayCurrency: string = 'USD') => {
    async function postItem(cartId?: string | null) {
      return await request<Cart>(`/api/cart/items?displayCurrency=${displayCurrency}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          cartId: cartId || undefined,
          listingId,
          quantity,
        }),
      });
    }

    let cart: Cart;
    const cartId = window.localStorage.getItem(cartIdStorageKey);

    try {
      cart = await postItem(cartId);
    } catch (error) {
      if (!cartId || !shouldClearStoredCartId(error)) {
        throw error;
      }

      clearStoredCartId();
      cart = await postItem();
    }

    window.localStorage.setItem(cartIdStorageKey, cart.id);
    notifyCartUpdated(cart);
    return cart;
  },

  clearStoredCart: clearStoredCartId,

  updateItem: async (listingId: string, quantity: number, displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    if (!cartId) throw new Error('No cart found');

    try {
      const cart = await request<Cart>(`/api/cart/items/${listingId}?displayCurrency=${displayCurrency}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          cartId,
          quantity,
        }),
      });
      notifyCartUpdated(cart);
      return cart;
    } catch (error) {
      if (shouldClearStoredCartId(error)) {
        clearStoredCartId();
      }

      throw error;
    }
  },

  removeItem: async (listingId: string, displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    if (!cartId) throw new Error('No cart found');

    try {
      const params = new URLSearchParams({ cartId, displayCurrency });
      const cart = await request<Cart>(`/api/cart/items/${listingId}?${params}`, {
        method: 'DELETE',
      });
      notifyCartUpdated(cart);
      return cart;
    } catch (error) {
      if (shouldClearStoredCartId(error)) {
        clearStoredCartId();
      }

      throw error;
    }
  },
};
