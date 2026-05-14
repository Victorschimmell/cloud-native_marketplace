// Dashboard data shapes. Arrays and counters are empty until the back-end is wired up.

export type ProductCategory = 'Electronics' | 'Clothing' | 'Home' | 'Books' | 'Other';

export interface SellerProduct {
  id: string;
  name: string;
  category: ProductCategory;
  price: number;
  imageUrl: string;
  inStock: boolean;
  rating: number;
  ratingCount: number;
}

export type OrderStatus = 'Pending' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled';

export interface SellerOrder {
  id: string;
  customerName: string;
  customerEmail: string;
  date: string; // ISO date
  total: number;
  status: OrderStatus;
  tracking?: string;
}

export const placeholderProducts: SellerProduct[] = [];

export const placeholderOrders: SellerOrder[] = [];

export const placeholderStats = {
  totalProducts: 0,
  totalRevenue: 0,
  totalOrders: 0,
  activeOrders: 0,
};
