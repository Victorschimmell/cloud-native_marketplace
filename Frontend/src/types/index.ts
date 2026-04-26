// types/index.ts - legacy shared types.

export interface Product {
  id: string;
  name: string;
  price: number;
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

// Prefer feature-local types for new feature work.
