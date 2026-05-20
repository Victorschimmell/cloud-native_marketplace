import { request } from '../../../shared/api/request';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { PageResponse } from '../../../shared/types/pagination';
import type { OrderItem, OrderStatus, Shipment } from '../../orders/types';

export interface CreateProductRequest {
  categoryId: string;
  productName: string;
  description: string;
  imageUrl?: string | null;
  price: number;
  inventoryQuantity: number;
}

export interface UpdateProductRequest {
  categoryId: string;
  productName: string;
  description: string;
  imageUrl?: string | null;
  price: number;
  inventoryQuantity: number;
  visibilityStatus: string;
}

export interface ProductResponse {
  id: string;
  categoryId: string;
  productName: string;
  description: string;
  imageUrl?: string | null;
}

export interface SellerListing {
  listingId: string;
  productId: string;
  categoryId: string;
  productName: string;
  description: string;
  imageUrl?: string | null;
  categoryName: string | null;
  listingPrice: number;
  inventoryQuantity: number;
  visibilityStatus: string;
}

export interface SellerOrderSummary {
  id: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
  orderNumber: string;
  orderStatus: OrderStatus;
  orderStatusDescription?: string | null;
  orderPurchaseTimestampUtc: string;
  orderApprovedAtUtc?: string | null;
  orderDeliveredCarrierDateUtc?: string | null;
  orderDeliveredCustomerDateUtc?: string | null;
  orderEstimatedDeliveryDateUtc?: string | null;
  subtotalAmount: number;
  freightAmount: number;
  totalAmount: number;
  currencyCode: CurrencyCode;
  items: OrderItem[];
  shipments: Shipment[];
}

export interface SellerOrderStats {
  totalOrders: number;
  activeOrders: number;
  totalRevenue: number;
  currencyCode: CurrencyCode;
}

export const sellerApi = {
  createProduct: async (data: CreateProductRequest): Promise<ProductResponse> => {
    return request<ProductResponse>('/api/products', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
  },

  getMyListings: async (signal?: AbortSignal): Promise<SellerListing[]> => {
    return request<SellerListing[]>('/api/products/my-listings', { signal });
  },

  getMyOrders: async (
    currency: CurrencyCode,
    page = 1,
    pageSize = 50,
    signal?: AbortSignal,
  ): Promise<PageResponse<SellerOrderSummary>> => {
    const params = new URLSearchParams({
      currency,
      page: page.toString(),
      pageSize: pageSize.toString(),
      sort: 'newest',
    });

    return request<PageResponse<SellerOrderSummary>>(`/api/sellers/me/orders?${params}`, { signal });
  },

  getMyOrder: async (orderId: string, currency: CurrencyCode, signal?: AbortSignal): Promise<SellerOrderSummary> => {
    const params = new URLSearchParams({ currency });
    return request<SellerOrderSummary>(`/api/sellers/me/orders/${orderId}?${params}`, { signal });
  },

  getMyOrderStats: async (currency: CurrencyCode, signal?: AbortSignal): Promise<SellerOrderStats> => {
    const params = new URLSearchParams({ currency });
    return request<SellerOrderStats>(`/api/sellers/me/order-stats?${params}`, { signal });
  },

  updateMyOrderStatus: async (
    orderId: string,
    status: OrderStatus,
    currency: CurrencyCode,
  ): Promise<SellerOrderSummary> => {
    const params = new URLSearchParams({ currency });
    return request<SellerOrderSummary>(`/api/sellers/me/orders/${orderId}/status?${params}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status }),
    });
  },

  updateProduct: async (listingId: string, data: UpdateProductRequest): Promise<ProductResponse> => {
    return request<ProductResponse>(`/api/products/listings/${listingId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
  },

  deleteProduct: async (listingId: string): Promise<void> => {
    await request<void>(`/api/products/listings/${listingId}`, { method: 'DELETE' });
  },
};
