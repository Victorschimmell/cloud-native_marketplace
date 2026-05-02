import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import StatusMessage from '../../../shared/components/StatusMessage';
import { ApiError } from '../../../shared/api/request';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { useAuth } from '../../auth/useAuth';
import { checkoutApi } from '../api/checkoutApi';
import { CheckoutCustomerPanel, CheckoutPaymentPanel, CheckoutPreviewList, CheckoutShippingAddressForm } from '../components';
import { PaymentType } from '../types';
import type { CheckoutCustomerProfile, CheckoutPreview, CheckoutShippingAddress, Currency } from '../types';
import './CheckoutPage.css';

const paymentCurrencyCode = 'BRL';
const checkoutFormId = 'checkout-form';

const emptyPreview = (currencyCode: string): CheckoutPreview => ({
  lines: [],
  subtotalAmount: 0,
  freightAmount: 0,
  totalAmount: 0,
  currencyCode,
});

const emptyShippingAddress: CheckoutShippingAddress = {
  addressLine1: '',
  addressLine2: '',
  city: '',
  state: '',
  postalCode: '',
  countryCode: 'DK',
};

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
  const [isLoadingCustomer, setIsLoadingCustomer] = useState(true);
  const [isLoadingCurrency, setIsLoadingCurrency] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [preview, setPreview] = useState<CheckoutPreview>(() => emptyPreview('BRL'));
  const [paymentPreview, setPaymentPreview] = useState<CheckoutPreview>(() => emptyPreview(paymentCurrencyCode));
  const [customerProfile, setCustomerProfile] = useState<CheckoutCustomerProfile | null>(null);
  const [currencyInfo, setCurrencyInfo] = useState<Currency | null>(null);
  const [shippingAddress, setShippingAddress] = useState<CheckoutShippingAddress>(emptyShippingAddress);
  const [hasAttemptedSubmit, setHasAttemptedSubmit] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { currency } = useCurrency();
  const { user } = useAuth();

  useEffect(() => {
    const abortController = new AbortController();

    async function loadCheckoutPreview() {
      try {
        setIsLoading(true);
        setError(null);

        const displayPreview = await checkoutApi.getCheckoutPreview(currency, abortController.signal);
        const nextPaymentPreview = currency === paymentCurrencyCode
          ? displayPreview
          : await checkoutApi.getCheckoutPreview(paymentCurrencyCode, abortController.signal);
        setPreview(displayPreview);
        setPaymentPreview(nextPaymentPreview);
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
    if (!user) {
      setIsLoadingCustomer(false);
      return;
    }

    const abortController = new AbortController();

    async function loadCustomerProfile() {
      try {
        setIsLoadingCustomer(true);
        setError(null);

        const profile = await checkoutApi.getCustomerProfile(user!.id, abortController.signal);
        setCustomerProfile(profile);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }

        setError(`Failed to load customer information: ${getErrorMessage(requestError)}`);
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoadingCustomer(false);
        }
      }
    }

    void loadCustomerProfile();

    return () => {
      abortController.abort();
    };
  }, [user]);

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

  const previewTotal = preview.totalAmount;
  const paymentTotal = paymentPreview.totalAmount;
  const hasPreview = preview.lines.length > 0;
  const isCheckoutDisabled = isSubmitting
    || isLoadingCurrency
    || isLoadingCustomer
    || !currencyInfo
    || !customerProfile
    || !hasPreview
    || paymentTotal <= 0;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setHasAttemptedSubmit(true);

    if (!currencyInfo || !customerProfile || !hasPreview || paymentTotal <= 0) {
      setError('Please ensure your cart, customer profile, and payment information are ready.');
      return;
    }

    const normalizedShippingAddress = normalizeShippingAddress(shippingAddress);
    const addressValidationError = validateShippingAddress(normalizedShippingAddress);
    if (addressValidationError) {
      setError(addressValidationError);
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      const cartId = window.localStorage.getItem('marketplace.checkout.cartId') ?? undefined;
      const response = await checkoutApi.checkout(
        {
          cartId,
          shippingAddress: normalizedShippingAddress,
          payments: [
            {
              currencyId: currencyInfo.id,
              paymentType: PaymentType.CreditCard,
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

      <form id={checkoutFormId} className="checkout-page__layout" aria-busy={isLoading || isLoadingCustomer} noValidate onSubmit={handleSubmit}>
        <div className="checkout-page__main">
          <CheckoutCustomerPanel
            customer={customerProfile}
            email={user?.email ?? ''}
            isLoading={isLoadingCustomer}
          />

          <CheckoutShippingAddressForm
            address={shippingAddress}
            disabled={isSubmitting}
            onAddressChange={setShippingAddress}
            showValidation={hasAttemptedSubmit}
          />

          <CheckoutPaymentPanel
            currency={currency}
            total={previewTotal}
            paymentTotal={paymentTotal}
            currencyInfo={currencyInfo}
            isLoadingCurrency={isLoadingCurrency}
          />
        </div>

        <div className="checkout-page__sidebar">
          {hasPreview && (
            <CheckoutPreviewList
              currency={currency}
              formId={checkoutFormId}
              isDisabled={isCheckoutDisabled}
              isSubmitting={isSubmitting}
              preview={preview}
            />
          )}
        </div>
      </form>
    </PageSkeleton>
  );
}

function normalizeShippingAddress(address: CheckoutShippingAddress): CheckoutShippingAddress {
  return {
    addressLine1: address.addressLine1.trim(),
    addressLine2: address.addressLine2?.trim() || null,
    city: address.city.trim(),
    state: address.state.trim(),
    postalCode: address.postalCode.trim(),
    countryCode: address.countryCode.trim().toUpperCase(),
  };
}

function validateShippingAddress(address: CheckoutShippingAddress): string | null {
  if (!address.addressLine1) {
    return 'Enter a shipping address.';
  }

  if (!address.city) {
    return 'Enter a shipping city.';
  }

  if (!address.state) {
    return 'Enter a shipping state or region.';
  }

  if (!address.postalCode) {
    return 'Enter a ZIP or postal code.';
  }

  if (!address.countryCode) {
    return 'Enter a country code.';
  }

  return null;
}
