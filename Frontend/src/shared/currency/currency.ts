export type CurrencyCode = 'BRL' | 'USD' | 'DKK';

export const currencyOptions: Array<{ code: CurrencyCode; label: string }> = [
  { code: 'DKK', label: 'Danske Kroner' },
  { code: 'USD', label: 'US Dollars' },
  { code: 'BRL', label: 'Real Brasileiro' },
];

export function getCurrencyLocale(currency: CurrencyCode) {
  return currency === 'DKK' ? 'da-DK' : currency === 'USD' ? 'en-US' : 'pt-BR';
}

export function isCurrencyCode(value: string | null): value is CurrencyCode {
  return value === 'BRL' || value === 'USD' || value === 'DKK';
}
