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
