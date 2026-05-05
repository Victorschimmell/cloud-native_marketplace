import { getCurrencyLocale } from '../../shared/currency/currency';
import type { CurrencyCode } from '../../shared/currency/currency';
import type { Order, OrderStatus, PaymentStatus } from './types';

export function formatMoney(amount: number, currency: CurrencyCode) {
  return amount.toLocaleString(getCurrencyLocale(currency), {
    style: 'currency',
    currency,
  });
}

export function formatDateTime(value: string | null | undefined) {
  if (!value) {
    return 'Not set';
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function getOrderItemCount(order: Order) {
  return order.items.reduce((total, item) => total + item.quantity, 0);
}

export function getPaymentStatus(order: Order): PaymentStatus | 'Unpaid' {
  if (order.payments.some((payment) => payment.paymentStatus === 'Paid')) {
    return 'Paid';
  }

  return order.payments[0]?.paymentStatus ?? 'Unpaid';
}

export function getStatusTone(status: OrderStatus | PaymentStatus | 'Unpaid') {
  if (status === 'Paid' || status === 'Approved' || status === 'Delivered') {
    return 'success';
  }

  if (status === 'Cancelled' || status === 'Failed' || status === 'Refunded' || status === 'Returned') {
    return 'danger';
  }

  if (status === 'Shipped' || status === 'Processing' || status === 'Authorized') {
    return 'info';
  }

  return 'neutral';
}
