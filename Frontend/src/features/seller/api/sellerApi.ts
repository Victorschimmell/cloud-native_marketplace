import { request } from '../../../shared/api/request';

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
