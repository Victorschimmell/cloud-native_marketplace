// types/index.ts - basic types

export interface Product {
  id: string;
  name: string;
  price: number;
}

export interface BrowseProduct {
  productId: string;
  listingId: string;
  categoryId: string;
  productName: string;
  description: string;
  categoryName: string | null;
  price: number;
  stockQuantity: number;
  productPhotosQty: number;
  productWeightG: number;
  productLengthCm: number;
  productHeightCm: number;
  productWidthCm: number;
}

export interface PageResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface User {
  id: string;
  name: string;
  email: string;
}

export interface Order {
  id: string;
  status: string;
  total: number;
}

// Add more types later
