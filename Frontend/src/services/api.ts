import type { BrowseProduct, PageResponse } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, init);

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`);
  }

  return response.json() as Promise<T>;
}

const api = {
  getProducts: async (page = 1, pageSize = 12, signal?: AbortSignal) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    return request<PageResponse<BrowseProduct>>(`/api/products?${params}`, { signal });
  },

  getProductById: async (id: string) => {
    console.log(`Fetching product ${id}`);
    return { message: `Product ${id} details` };
  },

  login: async () => {
    console.log('Login...');
    return { message: 'Login placeholder' };
  },
};

export default api;
