import type { ReactNode } from 'react';
import { useMemo, useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import { useCurrency } from '../../../shared/currency/useCurrency';
import ProductInventoryTable from './ProductInventoryTable';
import OrdersTable from './OrdersTable';
import { sellerApi, type SellerListing } from '../api/sellerApi';
import { placeholderOrders, placeholderStats } from '../data/placeholderData';
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
  const { currency } = useCurrency();
  const priceFormatter = useMemo(
    () => new Intl.NumberFormat(getCurrencyLocale(currency), { style: 'currency', currency }),
    [currency],
  );
  const [listings, setListings] = useState<SellerListing[]>([]);

  useEffect(() => {
    const controller = new AbortController();
    sellerApi.getMyListings(controller.signal).then(setListings).catch(() => {});
    return () => controller.abort();
  }, []);

  return (
    <PageSkeleton
      summary="Manage product inventory, seller performance and fulfillment activity."
      title="Seller Dashboard"
      titleId="seller-dashboard-title"
    >
      <section className="seller-dashboard" aria-labelledby="seller-dashboard-title">
        <div className="seller-dashboard__actions">
          <Link to="/seller/products/new" className="seller-dashboard__primary-action">
            Add Product
          </Link>
        </div>

        <StatsRow priceFormatter={priceFormatter} />
        <TabBar activeTab={activeTab} />

        {activeTab === 'products' ? (
          <ProductInventoryTable
            priceFormatter={priceFormatter}
            products={listings}
            onDeleteListing={(listingId) => setListings((prev) => prev.filter((l) => l.listingId !== listingId))}
          />
        ) : (
          <OrdersTable orders={placeholderOrders} priceFormatter={priceFormatter} />
        )}
      </section>
    </PageSkeleton>
  );
}

/* Stats */

function StatsRow({ priceFormatter }: { priceFormatter: Intl.NumberFormat }) {
  const stats = placeholderStats;

  return (
    <div className="seller-dashboard__stats">
      <StatCard label="Total Products" value={stats.totalProducts.toString()} />
      <StatCard label="Total Revenue" value={priceFormatter.format(stats.totalRevenue)} />
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
