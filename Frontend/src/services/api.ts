import { request } from '../shared/api/request';

// Legacy placeholder API surface. New feature work should use feature-local api modules.
const api = {
  getProductById: async (id: string) => {
    return request<{ message: string }>(`/api/products/${id}`);
  },

  login: async () => {
    console.log('Login...');
    return { message: 'Login placeholder' };
  },
};

export default api;
