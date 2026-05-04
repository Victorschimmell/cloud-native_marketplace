export const PaymentType = {
  CreditCard: 'CreditCard',
  DebitCard: 'DebitCard',
  BankTransfer: 'BankTransfer',
  Wallet: 'Wallet',
} as const;

export type PaymentType = typeof PaymentType[keyof typeof PaymentType];

export interface Currency {
  id: string;
  code: string;
  name: string;
  symbol?: string;
}

export interface CheckoutPreviewLine {
  listingId: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  currencyCode: string;
}

export interface CheckoutPreview {
  lines: CheckoutPreviewLine[];
  subtotalAmount: number;
  freightAmount: number;
  totalAmount: number;
  currencyCode: string;
}

export interface CheckoutShippingAddress {
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state: string;
  postalCode: string;
  countryCode: string;
}

export interface Address extends CheckoutShippingAddress {
  id: string;
}

export interface CheckoutCustomerProfile {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  phone: string;
  defaultAddressId?: string | null;
}
