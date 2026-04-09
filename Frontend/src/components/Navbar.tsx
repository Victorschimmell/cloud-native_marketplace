import { Link } from 'react-router-dom';
import '../css/variables.css';

export default function Navbar() {
  return (
    <nav style={{
      backgroundColor: '#fff',
      borderBottom: '1px solid #e5e7eb',
      boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
      padding: '16px 0',
      width: '100%'
    }}>
      <div style={{
        maxWidth: '1280px',
        margin: '0 auto',
        padding: '0 40px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between'
      }}>
        <Link to="/" style={{ fontSize: '28px', fontWeight: '700', color: 'var(--primary-color)', textDecoration: 'none' }}>
          Marketplace
        </Link>

        <div style={{ display: 'flex', gap: '32px', fontSize: '15px' }}>
          <Link to="/products" style={{ color: 'var(--text-dark)', textDecoration: 'none' }}>Browse Products</Link>
          <Link to="/categories" style={{ color: 'var(--text-dark)', textDecoration: 'none' }}>Categories</Link>
          <Link to="/cart" style={{ color: 'var(--text-dark)', textDecoration: 'none' }}>Cart</Link>
          <Link to="/seller/products" style={{ color: 'var(--text-dark)', textDecoration: 'none' }}>Seller Dashboard</Link>
          <Link to="/admin/users" style={{ color: 'var(--text-dark)', textDecoration: 'none' }}>Admin</Link>
        </div>

        <div style={{ display: 'flex', gap: '12px' }}>
          <Link 
            to="/login"
            style={{
              padding: '8px 24px',
              border: '1px solid #d1d5db',
              borderRadius: '8px',
              color: 'var(--text-dark)',
              textDecoration: 'none',
              fontSize: '14px'
            }}
          >
            Log in
          </Link>
          <Link 
            to="/register"
            style={{
              padding: '8px 24px',
              backgroundColor: 'var(--primary-color)',
              color: 'var(--text-light)',
              borderRadius: '8px',
              textDecoration: 'none',
              fontSize: '14px'
            }}
          >
            Register
          </Link>
        </div>
      </div>
    </nav>
  );
}