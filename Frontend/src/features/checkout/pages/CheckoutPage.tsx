import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import StatusMessage from '../../../shared/components/StatusMessage';
import { ApiError } from '../../../shared/api/request';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { checkoutApi } from '../api/checkoutApi';
import { CheckoutPaymentPanel, CheckoutPreviewList } from '../components';
import type { CheckoutPreviewLine, PaymentType, Currency } from '../types';
import './CheckoutPage.css';

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const emptyGuid = '00000000-0000-0000-0000-000000000000';
const paymentCurrencyCode = 'BRL';

function getErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const payload = err.payload as Record<string, unknown>;
    if (payload && typeof payload === 'object') {
      if ('error' in payload) {
        return String(payload.error);
      }
      if ('message' in payload) {
        return String(payload.message);
      }
    }

    return JSON.stringify(payload);
  }

  return 'Unknown error';
}

export default function CheckoutPage() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingCurrency, setIsLoadingCurrency] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [previewLines, setPreviewLines] = useState<CheckoutPreviewLine[]>([]);
  const [paymentPreviewLines, setPaymentPreviewLines] = useState<CheckoutPreviewLine[]>([]);
  const [currencyInfo, setCurrencyInfo] = useState<Currency | null>(null);
  const [selectedPaymentType, setSelectedPaymentType] = useState<PaymentType | null>(null);
  const [shippingAddressId, setShippingAddressId] = useState('');
  const [error, setError] = useState<string | null>(null);
  const { currency } = useCurrency();

  useEffect(() => {
    const abortController = new AbortController();

    async function loadCheckoutPreview() {
      try {
        setIsLoading(true);
        setError(null);

        const lines = await checkoutApi.getCheckoutPreview(currency, abortController.signal);
        const paymentLines = currency === paymentCurrencyCode
          ? lines
          : await checkoutApi.getCheckoutPreview(paymentCurrencyCode, abortController.signal);
        setPreviewLines(lines);
        setPaymentPreviewLines(paymentLines);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }

        setError(`Failed to load checkout preview: ${getErrorMessage(requestError)}`);
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadCheckoutPreview();

    return () => {
      abortController.abort();
    };
  }, [currency]);

  useEffect(() => {
    const abortController = new AbortController();

    async function loadCurrency() {
      try {
        setIsLoadingCurrency(true);
        setError(null);

        const info = await checkoutApi.getCurrency(paymentCurrencyCode, abortController.signal);
        setCurrencyInfo(info);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }

        setError(`Failed to load currency information: ${getErrorMessage(requestError)}`);
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoadingCurrency(false);
        }
      }
    }

    void loadCurrency();

    return () => {
      abortController.abort();
    };
  }, []);

  const previewTotal = useMemo(() => {
    return previewLines.reduce((sum, line) => sum + line.lineTotal, 0);
  }, [previewLines]);

  const paymentTotal = useMemo(() => {
    return paymentPreviewLines.reduce((sum, line) => sum + line.lineTotal, 0);
  }, [paymentPreviewLines]);

  const hasPreview = previewLines.length > 0;
  const trimmedShippingAddressId = shippingAddressId.trim();
  const isShippingAddressValid = guidPattern.test(trimmedShippingAddressId) && trimmedShippingAddressId !== emptyGuid;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!currencyInfo || !selectedPaymentType || !hasPreview || paymentTotal <= 0) {
      setError('Please select a payment method and ensure items are in your cart.');
      return;
    }

    if (!isShippingAddressValid) {
      setError('Enter a valid shipping address ID.');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      const cartId = window.localStorage.getItem('marketplace.checkout.cartId') ?? undefined;
      const response = await checkoutApi.checkout(
        {
          cartId,
          shippingAddressId: trimmedShippingAddressId,
          payments: [
            {
              currencyId: currencyInfo.id,
              paymentType: selectedPaymentType,
              paymentInstallments: 1,
              paymentValue: paymentTotal,
            },
          ],
        },
        paymentCurrencyCode
      );

      checkoutApi.clearCheckoutCart();
      navigate(`/orders/${response.order.id}`);
    } catch (requestError) {
      setError(`Checkout failed: ${getErrorMessage(requestError)}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <PageSkeleton
      title="Checkout"
      titleId="checkout-page-title"
    >
      {error && (
        <StatusMessage variant="error">
          {error}
        </StatusMessage>
      )}

      {!isLoading && !error && !hasPreview && (
        <StatusMessage>
          No checkout preview is available yet. Add products to your cart first.
          {' '}
          <Link className="checkout-page__inline-link" to="/products">
            Browse products
          </Link>
          .
        </StatusMessage>
      )}

      <div className="checkout-page__layout" aria-busy={isLoading}>
        <div className="checkout-page__main">
          {hasPreview && (
            <CheckoutPreviewList
              currency={currency}
              lines={previewLines}
            />
          )}
        </div>

        <div className="checkout-page__sidebar">
          <CheckoutPaymentPanel
            currency={currency}
            total={previewTotal}
            paymentTotal={paymentTotal}
            currencyInfo={currencyInfo}
            selectedPaymentType={selectedPaymentType}
            shippingAddressId={shippingAddressId}
            onPaymentTypeChange={setSelectedPaymentType}
            onShippingAddressIdChange={setShippingAddressId}
            onSubmit={handleSubmit}
            isSubmitting={isSubmitting}
            isLoadingCurrency={isLoadingCurrency}
            isShippingAddressValid={isShippingAddressValid}
          />

        </div>
      </div>
    </PageSkeleton>
  );
}
