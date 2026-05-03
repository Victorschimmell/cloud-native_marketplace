import { request } from '../../../shared/api/request';
import type { Address, CheckoutCustomerProfile, CheckoutPreview, CheckoutShippingAddress, Currency, PaymentType } from '../types';

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
  shippingAddress: CheckoutShippingAddress;
  saveShippingAddressAsDefault: boolean;
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
      return {
        lines: [],
        subtotalAmount: 0,
        freightAmount: 0,
        totalAmount: 0,
        currencyCode: currency,
      } as CheckoutPreview;
    }

    return await request<CheckoutPreview>(`/api/checkout/preview?${queryString}`, {
      signal,
    });
  },

  getCustomerProfile: async (userId: string, signal?: AbortSignal) => {
    return await request<CheckoutCustomerProfile>(`/api/customers/${userId}`, {
      signal,
    });
  },

  getAddress: async (addressId: string, signal?: AbortSignal) => {
    return await request<Address>(`/api/addresses/${addressId}`, {
      signal,
    });
  },

  createAddress: async (address: CheckoutShippingAddress, makeDefault: boolean, signal?: AbortSignal) => {
    return await request<Address>('/api/addresses', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        ...address,
        makeDefault,
      }),
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
