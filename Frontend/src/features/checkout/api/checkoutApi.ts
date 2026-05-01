import { request } from '../../../shared/api/request';
import type { CheckoutPreviewLine, Currency, PaymentType } from '../types';

const checkoutCartIdStorageKey = 'marketplace.checkout.cartId';

function buildCheckoutPreviewQuery(currency: string) {
  const cartId = window.localStorage.getItem(checkoutCartIdStorageKey);

  const searchParams = new URLSearchParams({
    currency,
  });

  if (cartId) {
    searchParams.set('cartId', cartId);
  }

  return searchParams.toString();
}

export interface CheckoutRequest {
  cartId?: string;
  userId?: string;
  sessionId?: string;
  shippingAddressId: string;
  payments: Array<{
    currencyId: string;
    paymentType: PaymentType;
    paymentInstallments: number;
    paymentValue: number;
  }>;
}

export interface CheckoutResponse {
  order: {
    id: string;
    orderNumber: string;
  };
  cart: {
    id: string;
  };
  payments: Array<{
    orderId: string;
    paymentSequential: number;
  }>;
  totalAmount: number;
}

export const checkoutApi = {
  getCheckoutPreview: async (currency: string, signal?: AbortSignal) => {
    const queryString = buildCheckoutPreviewQuery(currency);

    if (!queryString) {
      return [] as CheckoutPreviewLine[];
    }

    return await request<CheckoutPreviewLine[]>(`/api/checkout/preview?${queryString}`, {
      signal,
    });
  },

  getCurrency: async (code: string, signal?: AbortSignal) => {
    return await request<Currency>(`/api/payments/currency?code=${code}`, {
      signal,
    });
  },

  checkout: async (checkoutRequest: CheckoutRequest, currency: string, signal?: AbortSignal) => {
    return await request<CheckoutResponse>(`/api/checkout?currency=${currency}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(checkoutRequest),
      signal,
    });
  },

  clearCheckoutCart: () => {
    window.localStorage.removeItem(checkoutCartIdStorageKey);
  },
};
