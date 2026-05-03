import type { CurrencyCode } from '../../shared/currency/currency';

export type OrderStatus =
  | 'Pending'
  | 'Approved'
  | 'Processing'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled'
  | 'Returned';

export type PaymentStatus = 'Pending' | 'Authorized' | 'Paid' | 'Failed' | 'Refunded' | 'Cancelled';

export type PaymentType = 'CreditCard' | 'DebitCard' | 'BankTransfer' | 'Wallet';

export interface OrderItem {
  orderId: string;
  orderItemId: number;
  listingId: string;
  productId: string;
  productName: string;
  sellerId: string;
  quantity: number;
  unitPrice: number;
  freightValue: number;
  currencyCode: CurrencyCode;
  shippingLimitDateUtc?: string | null;
}

export interface Payment {
  orderId: string;
  paymentSequential: number;
  currencyId: string;
  paymentType: PaymentType;
  paymentInstallments: number;
  paymentValue: number;
  paymentStatus: PaymentStatus;
  externalPaymentReference?: string | null;
  paidAtUtc?: string | null;
}

export interface Review {
  id: string;
  orderId: string;
  reviewScore: number;
  reviewCommentTitle?: string | null;
  reviewCommentMessage?: string | null;
  reviewCreationDateUtc: string;
  reviewAnswerTimestampUtc?: string | null;
}

export interface Shipment {
  id: string;
  orderId: string;
  sellerId: string;
  carrierName: string;
  trackingNumber: string;
  shipmentStatus: string;
  shippedAtUtc?: string | null;
  deliveredAtUtc?: string | null;
  returnedAtUtc?: string | null;
}

export interface Order {
  id: string;
  customerId: string;
  userId: string;
  shippingAddressId: string;
  orderNumber: string;
  orderStatus: OrderStatus;
  orderPurchaseTimestampUtc: string;
  orderApprovedAtUtc?: string | null;
  orderDeliveredCarrierDateUtc?: string | null;
  orderDeliveredCustomerDateUtc?: string | null;
  orderEstimatedDeliveryDateUtc?: string | null;
  subtotalAmount: number;
  freightAmount: number;
  totalAmount: number;
  currencyCode: CurrencyCode;
  placedFromCartId?: string | null;
  items: OrderItem[];
  payments: Payment[];
  reviews: Review[];
  shipments: Shipment[];
}
