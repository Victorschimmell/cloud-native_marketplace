import { Link } from 'react-router-dom';
import type { ReactNode } from 'react';
import ProductInventoryTable from './ProductInventoryTable';
import OrdersTable from './OrdersTable';
import { placeholderOrders, placeholderProducts, placeholderStats } from '../data/placeholderData';
import './SellerDashboard.css';

export type SellerDashboardTab = 'products' | 'orders';

interface SellerDashboardProps {
  activeTab: SellerDashboardTab;
}

/**
 * Seller Dashboard
 *
 * This is the same page is used for the "My Products" and "Orders" tabs. 
 * The tab is controlled by `activeTab`, which comes from the route (so each tab has its
 * own URL). Replace placeholder arrays when the back-end is ready with real API calls.
 */
export default function SellerDashboard({ activeTab }: SellerDashboardProps) {
  return (
    <section className="seller-dashboard">
      <DashboardHeader />
      <StatsRow />
      <TabBar activeTab={activeTab} />

      {activeTab === 'products' ? (
        <ProductInventoryTable products={placeholderProducts} />
      ) : (
        <OrdersTable orders={placeholderOrders} />
      )}
    </section>
  );
}

/* Header */

function DashboardHeader() {
  return (
    <header className="seller-dashboard__header">
      <h1 className="seller-dashboard__title">
        <StoreIcon />
        Seller Dashboard
      </h1>
      <Link to="/seller/products/new" className="seller-dashboard__add-button">
        + Add Product
      </Link>
    </header>
  );
}

function StoreIcon() {
  return (
    <svg
      className="seller-dashboard__title-icon"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M3 9l1-5h16l1 5" />
      <path d="M4 9v11h16V9" />
      <path d="M9 22V12h6v10" />
    </svg>
  );
}

/* Stats */

function StatsRow() {
  const stats = placeholderStats;

  return (
    <div className="seller-dashboard__stats">
      <StatCard label="Total Products" value={stats.totalProducts.toString()} />
      <StatCard label="Total Revenue" value={`$${stats.totalRevenue.toLocaleString()}`} />
      <StatCard label="Total Orders" value={stats.totalOrders.toString()} />
      <StatCard label="Active Orders" value={stats.activeOrders.toString()} />
    </div>
  );
}

function StatCard({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="seller-dashboard__stat-card">
      <p className="seller-dashboard__stat-label">{label}</p>
      <p className="seller-dashboard__stat-value">{value}</p>
    </div>
  );
}

/*  Tabs  */

function TabBar({ activeTab }: { activeTab: SellerDashboardTab }) {
  return (
    <nav className="seller-dashboard__tabs" aria-label="Seller dashboard sections">
      <TabLink to="/seller/products" label="My Products" isActive={activeTab === 'products'} />
      <TabLink to="/seller/orders" label="Orders" isActive={activeTab === 'orders'} />
    </nav>
  );
}

function TabLink({ to, label, isActive }: { to: string; label: string; isActive: boolean }) {
  const className = `seller-dashboard__tab${isActive ? ' seller-dashboard__tab--active' : ''}`;
  return (
    <Link to={to} className={className} aria-current={isActive ? 'page' : undefined}>
      {label}
    </Link>
  );
}
