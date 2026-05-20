import { request } from '../../../shared/api/request';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { PageResponse } from '../../../shared/types/pagination';
import type { Order, OrderStatus, OrderSummary, Review } from '../types';

interface CreateReviewRequest {
  orderId: string;
  orderItemId: number;
  reviewScore: number;
  reviewCommentTitle?: string;
  reviewCommentMessage?: string;
}

export const orderApi = {
  getOrder: async (orderId: string, currency: CurrencyCode, signal?: AbortSignal) => {
    const params = new URLSearchParams({ currency });
    return request<Order>(`/api/orders/${orderId}?${params}`, { signal });
  },
  createReview: async (review: CreateReviewRequest, signal?: AbortSignal) => {
    return request<Review>('/api/reviews', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(review),
      signal,
    });
  },
  cancelOrder: async (orderId: string, currency: CurrencyCode, reason: string, signal?: AbortSignal) => {
    const params = new URLSearchParams({ currency });
    return request<Order>(`/api/orders/${orderId}/cancel?${params}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
      signal,
    });
  },
  getOrdersSummary: async (
    userId: string,
    page: number,
    pageSize: number,
    currency: CurrencyCode,
    status: 'all' | OrderStatus,
    sort: string,
    signal?: AbortSignal,
  ) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      currency,
      sort,
    });
    if (status !== 'all') {
      params.set('status', status);
    }

    return request<PageResponse<OrderSummary>>(`/api/customers/${userId}/orders?${params}`, { signal });
  },
};
