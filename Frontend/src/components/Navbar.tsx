import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/useAuth';
import { currencyOptions } from '../shared/currency/currency';
import { useCurrency } from '../shared/currency/useCurrency';
import '../css/variables.css';
import './Navbar.css';

export default function Navbar() {
  const { currency, setCurrency } = useCurrency();
  const { isAuthenticated, logout, user } = useAuth();
  const navigate = useNavigate();
  const [isCurrencyMenuOpen, setIsCurrencyMenuOpen] = useState(false);

  function selectCurrency(nextCurrency: typeof currency) {
    setCurrency(nextCurrency);
    setIsCurrencyMenuOpen(false);
  }

  function handleLogout() {
    logout();
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
          <Link to="/cart">Cart</Link>
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
