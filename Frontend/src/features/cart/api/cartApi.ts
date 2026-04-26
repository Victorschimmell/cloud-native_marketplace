import { request } from '../../../shared/api/request';
import type { Cart } from '../types';

const cartIdStorageKey = 'marketplace.checkout.cartId';

export const cartApi = {
  addItem: async (listingId: string, quantity: number) => {
    // Temporary until real logged-in user cart resolution is wired up.
    const cartId = window.localStorage.getItem(cartIdStorageKey);
    const cart = await request<Cart>('/api/cart', {
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
};
