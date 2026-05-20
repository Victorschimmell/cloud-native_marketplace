import type { ReactNode } from 'react';
import { useMemo, useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import { useCurrency } from '../../../shared/currency/useCurrency';
import ProductInventoryTable from './ProductInventoryTable';
import OrdersTable from './OrdersTable';
import {
  sellerApi,
  type SellerListing,
  type SellerOrderSort,
  type SellerOrderStats,
  type SellerOrderStatusFilter,
  type SellerOrderSummary,
} from '../api/sellerApi';
import './SellerDashboard.css';

export type SellerDashboardTab = 'products' | 'orders';

const ORDERS_PAGE_SIZE = 25;

interface SellerDashboardProps {
  activeTab?: SellerDashboardTab;
}

/**
 * Seller Dashboard
 *
 * This same component is used for the "My Products" and "Orders" routes so tab changes
 * swap table content without remounting and refetching the dashboard counters.
 */
export default function SellerDashboard({ activeTab }: SellerDashboardProps) {
  const location = useLocation();
  const selectedTab = activeTab ?? getActiveTab(location.pathname);
  const { currency } = useCurrency();
  const priceFormatter = useMemo(
    () => new Intl.NumberFormat(getCurrencyLocale(currency), { style: 'currency', currency }),
    [currency],
  );
  const [listings, setListings] = useState<SellerListing[]>([]);
  const [orders, setOrders] = useState<SellerOrderSummary[]>([]);
  const [ordersPage, setOrdersPage] = useState(1);
  const [ordersTotalCount, setOrdersTotalCount] = useState(0);
  const [ordersStatusFilter, setOrdersStatusFilter] = useState<SellerOrderStatusFilter>('all');
  const [ordersSort, setOrdersSort] = useState<SellerOrderSort>('newest');
  const [orderStats, setOrderStats] = useState<SellerOrderStats | null>(null);
  const [ordersError, setOrdersError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    sellerApi.getMyListings(controller.signal).then(setListings).catch(() => {});
    return () => controller.abort();
  }, []);

  useEffect(() => {
    const controller = new AbortController();

    async function loadOrders() {
      try {
        const ordersResponse = await sellerApi.getMyOrders({
          currency,
          page: ordersPage,
          pageSize: ORDERS_PAGE_SIZE,
          status: ordersStatusFilter,
          sort: ordersSort,
          signal: controller.signal,
        });

        setOrders(ordersResponse.items);
        setOrdersTotalCount(ordersResponse.totalCount);
        setOrdersError(null);
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return;
        }

        setOrders([]);
        setOrdersTotalCount(0);
        setOrdersError('Orders could not be loaded right now.');
      }
    }

    void loadOrders();

    return () => controller.abort();
  }, [currency, ordersPage, ordersSort, ordersStatusFilter]);

  useEffect(() => {
    const controller = new AbortController();

    async function loadOrderStats() {
      try {
        const statsResponse = await sellerApi.getMyOrderStats(currency, controller.signal);
        setOrderStats(statsResponse);
      } catch (error) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return;
        }

        setOrderStats(null);
      }
    }

    void loadOrderStats();

    return () => controller.abort();
  }, [currency]);

  function handleOrdersStatusChange(status: SellerOrderStatusFilter) {
    setOrdersStatusFilter(status);
    setOrdersPage(1);
  }

  function handleOrdersSortChange(sort: SellerOrderSort) {
    setOrdersSort(sort);
    setOrdersPage(1);
  }

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

        <StatsRow listings={listings} orderStats={orderStats} priceFormatter={priceFormatter} />
        <TabBar activeTab={selectedTab} />

        {selectedTab === 'products' ? (
          <ProductInventoryTable
            priceFormatter={priceFormatter}
            products={listings}
            onDeleteListing={(listingId) => setListings((prev) => prev.filter((l) => l.listingId !== listingId))}
          />
        ) : (
          <OrdersTable
            error={ordersError}
            orders={orders}
            page={ordersPage}
            pageSize={ORDERS_PAGE_SIZE}
            priceFormatter={priceFormatter}
            sort={ordersSort}
            statusFilter={ordersStatusFilter}
            totalCount={ordersTotalCount}
            onPageChange={setOrdersPage}
            onSortChange={handleOrdersSortChange}
            onStatusFilterChange={handleOrdersStatusChange}
          />
        )}
      </section>
    </PageSkeleton>
  );
}

/* Stats */

function StatsRow({
  listings,
  orderStats,
  priceFormatter,
}: {
  listings: SellerListing[];
  orderStats: SellerOrderStats | null;
  priceFormatter: Intl.NumberFormat;
}) {
  return (
    <div className="seller-dashboard__stats">
      <StatCard label="Total Products" value={listings.length.toString()} />
      <StatCard label="Total Revenue" value={priceFormatter.format(orderStats?.totalRevenue ?? 0)} />
      <StatCard label="Total Orders" value={(orderStats?.totalOrders ?? 0).toString()} />
      <StatCard label="Active Orders" value={(orderStats?.activeOrders ?? 0).toString()} />
    </div>
  );
}

function getActiveTab(pathname: string): SellerDashboardTab {
  return pathname.includes('/seller/orders') ? 'orders' : 'products';
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
