import type { ReactNode } from 'react';
import { useCallback, useMemo, useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { productApi } from '../../products/api/productApi';
import type { Category } from '../../products/types';
import ProductInventoryTable from './ProductInventoryTable';
import SellerProductDeleteDialog from './SellerProductDeleteDialog';
import SellerProductDetailsDialog from './SellerProductDetailsDialog';
import SellerProductEditDialog, { type SellerProductEditFormValues } from './SellerProductEditDialog';
import SellerProductPublishDialog from './SellerProductPublishDialog';
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

type PendingProductAction = {
  listingId: string;
  action: 'delete' | 'publish' | 'update';
} | null;

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
  const [categories, setCategories] = useState<Category[]>([]);
  const [orders, setOrders] = useState<SellerOrderSummary[]>([]);
  const [ordersPage, setOrdersPage] = useState(1);
  const [ordersTotalCount, setOrdersTotalCount] = useState(0);
  const [ordersStatusFilter, setOrdersStatusFilter] = useState<SellerOrderStatusFilter>('all');
  const [ordersSort, setOrdersSort] = useState<SellerOrderSort>('newest');
  const [orderStats, setOrderStats] = useState<SellerOrderStats | null>(null);
  const [ordersError, setOrdersError] = useState<string | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<SellerListing | null>(null);
  const [editingProduct, setEditingProduct] = useState<SellerListing | null>(null);
  const [deletingProduct, setDeletingProduct] = useState<SellerListing | null>(null);
  const [publishingProduct, setPublishingProduct] = useState<SellerListing | null>(null);
  const [pendingProductAction, setPendingProductAction] = useState<PendingProductAction>(null);
  const [productActionError, setProductActionError] = useState<string | null>(null);

  const loadListings = useCallback(async (signal?: AbortSignal) => {
    const listingsResponse = await sellerApi.getMyListings(signal);
    setListings(listingsResponse);
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    loadListings(controller.signal).catch(() => {});
    return () => controller.abort();
  }, [loadListings]);

  useEffect(() => {
    const controller = new AbortController();
    productApi.getCategories(controller.signal).then(setCategories).catch(() => {});
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

  function openProductDetails(product: SellerListing) {
    setProductActionError(null);
    setSelectedProduct(product);
  }

  function openEditProduct(product: SellerListing) {
    setProductActionError(null);
    setSelectedProduct(null);
    setEditingProduct(product);
  }

  function openDeleteProduct(product: SellerListing) {
    setProductActionError(null);
    setSelectedProduct(null);
    setDeletingProduct(product);
  }

  function openPublishProduct(product: SellerListing) {
    setProductActionError(null);
    setSelectedProduct(null);
    setPublishingProduct(product);
  }

  function closeProductDialogs() {
    if (pendingProductAction) {
      return;
    }

    setSelectedProduct(null);
    setEditingProduct(null);
    setDeletingProduct(null);
    setPublishingProduct(null);
    setProductActionError(null);
  }

  async function updateProduct(values: SellerProductEditFormValues) {
    if (!editingProduct) {
      return;
    }

    const inventoryQuantity = Number.parseInt(values.inventoryQuantity, 10);
    const listingPrice = Number.parseFloat(values.price);

    setPendingProductAction({ listingId: editingProduct.listingId, action: 'update' });
    setProductActionError(null);

    try {
      await sellerApi.updateProduct(editingProduct.listingId, {
        categoryId: values.categoryId,
        description: values.description,
        imageUrl: normalizeOptional(values.imageUrl),
        inventoryQuantity,
        price: listingPrice,
        productName: values.name,
        visibilityStatus: values.visibilityStatus,
      }, currency);

      const categoryName = categories.find((category) => category.id === values.categoryId);
      const updatedProduct: SellerListing = {
        ...editingProduct,
        categoryId: values.categoryId,
        categoryName: categoryName?.categoryNameEn ?? categoryName?.categoryNamePt ?? editingProduct.categoryName,
        description: values.description,
        imageUrl: normalizeOptional(values.imageUrl),
        inventoryQuantity,
        listingPrice,
        productName: values.name,
        visibilityStatus: values.visibilityStatus,
      };

      setListings((current) => current.map((listing) => (
        listing.listingId === updatedProduct.listingId ? updatedProduct : listing
      )));
      setEditingProduct(null);
    } catch (error) {
      setProductActionError(error instanceof Error ? error.message : 'Failed to update product.');
    } finally {
      setPendingProductAction(null);
    }
  }

  async function deleteProduct() {
    if (!deletingProduct) {
      return;
    }

    setPendingProductAction({ listingId: deletingProduct.listingId, action: 'delete' });
    setProductActionError(null);

    try {
      await sellerApi.deleteProduct(deletingProduct.listingId);
      setListings((current) => current.filter((listing) => listing.listingId !== deletingProduct.listingId));
      setSelectedProduct((current) => current?.listingId === deletingProduct.listingId ? null : current);
      setDeletingProduct(null);
    } catch (error) {
      setProductActionError(error instanceof Error ? error.message : 'Failed to delete product.');
    } finally {
      setPendingProductAction(null);
    }
  }

  async function publishProduct() {
    if (!publishingProduct) {
      return;
    }

    setPendingProductAction({ listingId: publishingProduct.listingId, action: 'publish' });
    setProductActionError(null);

    try {
      await sellerApi.updateProduct(publishingProduct.listingId, {
        categoryId: publishingProduct.categoryId,
        description: publishingProduct.description,
        imageUrl: publishingProduct.imageUrl ?? null,
        inventoryQuantity: publishingProduct.inventoryQuantity,
        price: publishingProduct.listingPrice,
        productName: publishingProduct.productName,
        visibilityStatus: 'Published',
      }, currency);

      const updatedProduct = {
        ...publishingProduct,
        visibilityStatus: 'Published',
      };

      setListings((current) => current.map((listing) => (
        listing.listingId === updatedProduct.listingId ? updatedProduct : listing
      )));
      setPublishingProduct(null);
    } catch (error) {
      setProductActionError(error instanceof Error ? error.message : 'Failed to publish product.');
    } finally {
      setPendingProductAction(null);
    }
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
            pendingListingId={pendingProductAction?.listingId ?? null}
            priceFormatter={priceFormatter}
            products={listings}
            onDeleteProduct={openDeleteProduct}
            onEditProduct={openEditProduct}
            onOpenProduct={openProductDetails}
            onPublishProduct={openPublishProduct}
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
        {selectedProduct ? (
          <SellerProductDetailsDialog
            priceFormatter={priceFormatter}
            product={selectedProduct}
            onClose={closeProductDialogs}
            onDelete={openDeleteProduct}
            onEdit={openEditProduct}
          />
        ) : null}
        {editingProduct ? (
          <SellerProductEditDialog
            categories={categories}
            currency={currency}
            error={productActionError}
            isPending={pendingProductAction?.listingId === editingProduct.listingId && pendingProductAction.action === 'update'}
            product={editingProduct}
            onClose={closeProductDialogs}
            onSubmit={updateProduct}
          />
        ) : null}
        {deletingProduct ? (
          <SellerProductDeleteDialog
            error={productActionError}
            isPending={pendingProductAction?.listingId === deletingProduct.listingId && pendingProductAction.action === 'delete'}
            product={deletingProduct}
            onClose={closeProductDialogs}
            onConfirm={deleteProduct}
          />
        ) : null}
        {publishingProduct ? (
          <SellerProductPublishDialog
            error={productActionError}
            isPending={pendingProductAction?.listingId === publishingProduct.listingId && pendingProductAction.action === 'publish'}
            product={publishingProduct}
            onClose={closeProductDialogs}
            onConfirm={publishProduct}
          />
        ) : null}
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

function normalizeOptional(value: string): string | null {
  const normalized = value.trim();
  return normalized.length > 0 ? normalized : null;
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
