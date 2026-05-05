import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/useAuth';
import { cartApi, subscribeToCartUpdates } from '../features/cart/api/cartApi';
import { currencyOptions } from '../shared/currency/currency';
import { useCurrency } from '../shared/currency/useCurrency';
import '../css/variables.css';
import './Navbar.css';

export default function Navbar() {
  const { currency, setCurrency } = useCurrency();
  const { isAuthenticated, logout, user } = useAuth();
  const navigate = useNavigate();
  const [isCurrencyMenuOpen, setIsCurrencyMenuOpen] = useState(false);
  const [cartItemCount, setCartItemCount] = useState(0);
  const cartBadgeText = cartItemCount > 9 ? '9+' : cartItemCount.toString();

  useEffect(() => {
    let isMounted = true;

    async function loadCartCount() {
      if (!isAuthenticated) {
        setCartItemCount(0);
        return;
      }

      try {
        const cart = await cartApi.getCart(currency);

        if (isMounted) {
          setCartItemCount(cart.items.reduce((sum, item) => sum + item.quantity, 0));
        }
      } catch {
        if (isMounted) {
          setCartItemCount(0);
        }
      }
    }

    const unsubscribe = subscribeToCartUpdates((itemCount) => {
      if (isMounted) {
        setCartItemCount(itemCount);
      }
    });

    void loadCartCount();

    return () => {
      isMounted = false;
      unsubscribe();
    };
  }, [currency, isAuthenticated]);

  function selectCurrency(nextCurrency: typeof currency) {
    setCurrency(nextCurrency);
    setIsCurrencyMenuOpen(false);
  }

  function handleLogout() {
    logout();
    cartApi.clearStoredCart();
    navigate('/');
  }

  return (
    <nav className="navbar">
      <div className="navbar__inner">
        <Link className="navbar__brand" to="/">
          Marketplace
        </Link>

        <div className="navbar__links">
          <Link to="/products">Browse Products</Link>
          <Link to="/categories">Categories</Link>
          <Link to="/seller/products">Seller Dashboard</Link>
          <Link to="/admin/users">Admin</Link>
        </div>

        <div className="navbar__actions">
          <div
            className="navbar__currency"
            data-open={isCurrencyMenuOpen}
            onMouseEnter={() => setIsCurrencyMenuOpen(true)}
            onMouseLeave={() => setIsCurrencyMenuOpen(false)}
          >
            <button
              aria-expanded={isCurrencyMenuOpen}
              aria-label="Select currency"
              className="navbar__currency-trigger"
              onClick={() => setIsCurrencyMenuOpen((isOpen) => !isOpen)}
              type="button"
            >
              {currency}
            </button>
            <div className="navbar__currency-menu">
              {currencyOptions.map((option) => (
                <button
                  aria-pressed={currency === option.code}
                  key={option.code}
                  onClick={() => selectCurrency(option.code)}
                  type="button"
                >
                  <span>{option.code}</span>
                  {option.label}
                </button>
              ))}
            </div>
          </div>

          <Link
            aria-label={`Cart with ${cartItemCount > 9 ? '9 or more' : cartItemCount} ${cartItemCount === 1 ? 'item' : 'items'}`}
            className="navbar__cart"
            to="/cart"
          >
            <svg aria-hidden="true" focusable="false" viewBox="0 0 24 24">
              <path d="M6.3 6h15l-1.7 8.5a2 2 0 0 1-2 1.5H9.1a2 2 0 0 1-2-1.6L5.6 4H2" />
              <circle cx="9.5" cy="20" r="1.3" />
              <circle cx="17.5" cy="20" r="1.3" />
            </svg>
            {cartItemCount > 0 ? <span className="navbar__cart-badge">{cartBadgeText}</span> : null}
          </Link>

          <span className="navbar__utility-divider" aria-hidden="true" />

          {isAuthenticated ? (
            <>
              <span className="navbar__user">{user?.email}</span>
              <button className="navbar__login" onClick={handleLogout} type="button">
                Log out
              </button>
            </>
          ) : (
            <>
              <Link className="navbar__login" to="/login">
                Log in
              </Link>
              <Link className="navbar__register" to="/register">
                Register
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
}
