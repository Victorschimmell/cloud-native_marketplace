import { request } from '../../../shared/api/request';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { PageResponse } from '../../../shared/types/pagination';
import type { OrderItem, OrderStatus, Shipment } from '../../orders/types';

export interface CreateProductRequest {
  categoryId: string;
  productName: string;
  description: string;
  price: number;
  inStock: boolean;
}

export interface UpdateProductRequest {
  categoryId: string;
  productName: string;
  description: string;
  price: number;
  inventoryQuantity: number;
  visibilityStatus: string;
}

export interface ProductResponse {
  id: string;
  categoryId: string;
  productName: string;
  description: string;
}

export interface SellerListing {
  listingId: string;
  productId: string;
  categoryId: string;
  productName: string;
  description: string;
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

  updateProduct: async (productId: string, data: UpdateProductRequest): Promise<ProductResponse> => {
    return request<ProductResponse>(`/api/products/${productId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
  },

  deleteProduct: async (productId: string): Promise<void> => {
    await request<void>(`/api/products/${productId}`, { method: 'DELETE' });
  },
};
