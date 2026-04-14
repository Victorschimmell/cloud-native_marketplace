// api.ts - placeholder for backend calls

const api = {
  getProducts: async () => {
    console.log('Fetching products...');
    return { message: 'Products will come from backend later' };
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