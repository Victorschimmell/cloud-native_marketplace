export interface CartItem {
  id: string;
  cartId: string;
  listingId: string;
  quantity: number;
  unitPriceAtAddition: number;
  currencyCode: string;
  addedAtUtc: string;
  updatedAtUtc: string;
}

export interface Cart {
  id: string;
  userId: string | null;
  sessionId: string | null;
  status: string;
  expiresAtUtc: string;
  items: CartItem[];
}
