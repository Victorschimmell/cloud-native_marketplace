import { request } from '../../../shared/api/request';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { Order } from '../types';

export const orderApi = {
  getOrder: async (orderId: string, currency: CurrencyCode, signal?: AbortSignal) => {
    const params = new URLSearchParams({ currency });
    return request<Order>(`/api/orders/${orderId}?${params}`, { signal });
  },
};
