import { request } from '../../../shared/api/request';
import type { Cart } from '../types';

const cartIdStorageKey = 'marketplace.checkout.cartId';

export const cartApi = {
  getCart: async (displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    if (!cartId) {
      return {
        id: '',
        userId: null,
        sessionId: null,
        status: 'empty',
        expiresAtUtc: '',
        items: [],
      } as Cart;
    }
    try {
      return await request<Cart>(`/api/cart/${cartId}?displayCurrency=${displayCurrency}`);
    } catch {
      return {
        id: '',
        userId: null,
        sessionId: null,
        status: 'empty',
        expiresAtUtc: '',
        items: [],
      } as Cart;
    }
  },

  addItem: async (listingId: string, quantity: number, displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    const cart = await request<Cart>(`/api/cart/items?displayCurrency=${displayCurrency}`, {
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

    window.localStorage.setItem(cartIdStorageKey, cart.id);
    return cart;
  },

  updateItem: async (listingId: string, quantity: number, displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    if (!cartId) throw new Error('No cart found');

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
  },

  // TODO: There should have a delete method in the backend, maybe change to that later
  removeItem: async (listingId: string, displayCurrency: string = 'USD') => {
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    if (!cartId) throw new Error('No cart found');

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
  },
};
