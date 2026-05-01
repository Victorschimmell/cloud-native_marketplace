import { ApiError, request } from '../../../shared/api/request';
import type { Cart } from '../types';

const cartIdStorageKey = 'marketplace.checkout.cartId';

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
}

function shouldClearStoredCartId(error: unknown) {
  return error instanceof ApiError && (error.status === 403 || error.status === 404);
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
      return cart;
    } catch (error) {
      if (shouldClearStoredCartId(error)) {
        clearStoredCartId();
      }

      return getEmptyCart();
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
    return cart;
  },

  clearStoredCart: clearStoredCartId,

  updateItem: async (listingId: string, quantity: number, displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    if (!cartId) throw new Error('No cart found');

    try {
      return await request<Cart>(`/api/cart/items/${listingId}?displayCurrency=${displayCurrency}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          cartId,
          quantity,
        }),
      });
    } catch (error) {
      if (shouldClearStoredCartId(error)) {
        clearStoredCartId();
      }

      throw error;
    }
  },

  // TODO: There should have a delete method in the backend, maybe change to that later
  removeItem: async (listingId: string, displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    if (!cartId) throw new Error('No cart found');

    try {
      return await request<Cart>(`/api/cart/items/${listingId}?displayCurrency=${displayCurrency}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          cartId,
          quantity: 0,
        }),
      });
    } catch (error) {
      if (shouldClearStoredCartId(error)) {
        clearStoredCartId();
      }

      throw error;
    }
  },
};
