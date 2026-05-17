import { useEffect, useMemo, useState } from 'react';
import type { PropsWithChildren } from 'react';
import { CurrencyContext } from './CurrencyContext';
import type { CurrencyCode } from './currency';
import { isCurrencyCode } from './currency';

const currencyStorageKey = 'marketplace.currency';

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
