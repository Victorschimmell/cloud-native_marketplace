import { request } from '../../../shared/api/request';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { Order, Review } from '../types';

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
};
