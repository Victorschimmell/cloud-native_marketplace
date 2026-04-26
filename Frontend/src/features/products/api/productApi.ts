import { request } from '../../../shared/api/request';
import type { PageResponse } from '../../../shared/types/pagination';
import type { BrowseProduct, Category, ProductCurrencyCode, ProductDetails, ProductSortOption } from '../types';

interface GetProductsOptions {
  categoryId?: string;
  currency?: ProductCurrencyCode;
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

    if (options?.currency) {
      params.set('currency', options.currency);
    }

    return request<PageResponse<BrowseProduct>>(`/api/products?${params}`, { signal: options?.signal });
  },

  getCategories: async (signal?: AbortSignal) => {
    return request<Category[]>('/api/categories', { signal });
  },

  getProduct: async (productId: string, listingId?: string | null, currency?: ProductCurrencyCode, signal?: AbortSignal) => {
    const params = new URLSearchParams();

    if (listingId) {
      params.set('listingId', listingId);
    }

    if (currency) {
      params.set('currency', currency);
    }

    const query = params.size > 0 ? `?${params}` : '';
    return request<ProductDetails>(`/api/products/${productId}${query}`, { signal });
  },
};
