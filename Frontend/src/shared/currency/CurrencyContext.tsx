import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import type { PropsWithChildren } from 'react';

export type CurrencyCode = 'BRL' | 'USD' | 'DKK';

export const currencyOptions: Array<{ code: CurrencyCode; label: string }> = [
  { code: 'DKK', label: 'Danske Kroner' },
  { code: 'USD', label: 'US Dollars' },
  { code: 'BRL', label: 'Real Brasileiro' },
];

interface CurrencyContextValue {
  currency: CurrencyCode;
  setCurrency: (currency: CurrencyCode) => void;
}

const currencyStorageKey = 'marketplace.currency';
const CurrencyContext = createContext<CurrencyContextValue | null>(null);

export function CurrencyProvider({ children }: PropsWithChildren) {
  const [currency, setCurrencyState] = useState<CurrencyCode>(() => {
    const storedCurrency = window.localStorage.getItem(currencyStorageKey);
    return isCurrencyCode(storedCurrency) ? storedCurrency : 'BRL';
  });

  useEffect(() => {
    window.localStorage.setItem(currencyStorageKey, currency);
  }, [currency]);

  const value = useMemo(
    () => ({
      currency,
      setCurrency: setCurrencyState,
    }),
    [currency],
  );

  return <CurrencyContext.Provider value={value}>{children}</CurrencyContext.Provider>;
}

export function useCurrency() {
  const context = useContext(CurrencyContext);

  if (!context) {
    throw new Error('useCurrency must be used inside CurrencyProvider.');
  }

  return context;
}

export function getCurrencyLocale(currency: CurrencyCode) {
  return currency === 'DKK' ? 'da-DK' : currency === 'USD' ? 'en-US' : 'pt-BR';
}

function isCurrencyCode(value: string | null): value is CurrencyCode {
  return value === 'BRL' || value === 'USD' || value === 'DKK';
}
