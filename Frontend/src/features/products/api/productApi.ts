import { request } from '../../../shared/api/request';
import type { PageResponse } from '../../../shared/types/pagination';
import type { BrowseProduct, Category, ProductDetails, ProductSortOption } from '../types';

interface GetProductsOptions {
  categoryId?: string;
  search?: string;
  sort?: ProductSortOption;
  signal?: AbortSignal;
}

export const productApi = {
  getProducts: async (page = 1, pageSize = 12, options?: GetProductsOptions) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (options?.categoryId) {
      params.set('categoryId', options.categoryId);
    }

    if (options?.search) {
      params.set('search', options.search);
    }

    if (options?.sort) {
      params.set('sort', options.sort);
    }

    return request<PageResponse<BrowseProduct>>(`/api/products?${params}`, { signal: options?.signal });
  },

  getCategories: async (signal?: AbortSignal) => {
    return request<Category[]>('/api/categories', { signal });
  },

  getProduct: async (productId: string, listingId?: string | null, signal?: AbortSignal) => {
    const params = new URLSearchParams();

    if (listingId) {
      params.set('listingId', listingId);
    }

    const query = params.size > 0 ? `?${params}` : '';
    return request<ProductDetails>(`/api/products/${productId}${query}`, { signal });
  },
};
