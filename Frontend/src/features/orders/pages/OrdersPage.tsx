import { useEffect, useMemo, useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import Pagination from '../../../shared/components/Pagination';
import StatusMessage from '../../../shared/components/StatusMessage';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { useAuth } from '../../auth/useAuth';
import { orderApi } from '../api/orderApi';
import { OrderSummaryCard, OrderSummarySkeleton } from '../components';
import type { OrderStatus, OrderSummary } from '../types';
import './OrdersPage.css';

const ordersPageSize = 5;
const orderStatusOptions: Array<'all' | OrderStatus> = [
  'all',
  'Pending',
  'Approved',
  'Processing',
  'Shipped',
  'Delivered',
  'Cancelled',
  'Returned',
];
type OrderSort = 'newest' | 'oldest' | 'total-high' | 'total-low';

export default function OrdersPage() {
  const { currency } = useCurrency();
  const { user } = useAuth();
  const [orders, setOrders] = useState<OrderSummary[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<'all' | OrderStatus>('all');
  const [sortBy, setSortBy] = useState<OrderSort>('newest');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const totalPages = useMemo(() => Math.max(1, Math.ceil(totalCount / ordersPageSize)), [totalCount]);
  const hasActiveFilters = statusFilter !== 'all';

  useEffect(() => {
    if (!user) {
      setOrders([]);
      setTotalCount(0);
      setError('Please sign in to view your orders.');
      setIsLoading(false);
      return;
    }

    const abortController = new AbortController();

    async function loadOrders() {
      try {
        setIsLoading(true);
        setError(null);
        
        if (!user) {
          setOrders([]);
          setTotalCount(0);
          setError('Please sign in to view your orders.');
          setIsLoading(false);
          return;
        }

        const response = await orderApi.getOrdersSummary(
          user.id,
          page,
          ordersPageSize,
          currency,
          statusFilter,
          sortBy,
          abortController.signal,
        );

        setOrders(response.items);
        setTotalCount(response.totalCount);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }

        setError('Orders could not be loaded right now.');
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadOrders();

    return () => {
      abortController.abort();
    };
  }, [currency, page, sortBy, statusFilter, user]);

  function updateStatusFilter(nextStatus: 'all' | OrderStatus) {
    setStatusFilter(nextStatus);
    setPage(1);
  }

  function updateSort(nextSort: OrderSort) {
    setSortBy(nextSort);
    setPage(1);
  }

  function goToPreviousPage() {
    setPage((currentPage) => Math.max(1, currentPage - 1));
  }

  function goToNextPage() {
    setPage((currentPage) => Math.min(totalPages, currentPage + 1));
  }

  const summary = getSummaryText({ error, hasActiveFilters, isLoading, totalCount });

  return (
    <PageSkeleton summary={summary} title="Orders" titleId="orders-page-title">
      <div className="orders-page">
        {error ? (
          <StatusMessage variant="error">{error}</StatusMessage>
        ) : null}

        {!isLoading && !error && !hasActiveFilters && orders.length === 0 ? (
          <StatusMessage>No orders yet. Items you buy will show up here.</StatusMessage>
        ) : null}

        {!error ? (
          <section className="orders-page__panel" aria-labelledby="orders-list-title">
            <header className="orders-page__panel-header">
              <div>
                <h2 id="orders-list-title">Order history</h2>
                <p>{isLoading ? 'Loading your latest orders...' : summary}</p>
              </div>
              <div className="orders-page__filters" aria-label="Order filters">
                <label className="orders-page__filter">
                  Status
                  <select
                    disabled={isLoading}
                    onChange={(event) => updateStatusFilter(event.target.value as typeof statusFilter)}
                    value={statusFilter}
                  >
                    {orderStatusOptions.map((status) => (
                      <option key={status} value={status}>
                        {status === 'all' ? 'All statuses' : status}
                      </option>
                    ))}
                  </select>
                </label>
                <label className="orders-page__filter">
                  Sort
                  <select
                    disabled={isLoading}
                    onChange={(event) => updateSort(event.target.value as OrderSort)}
                    value={sortBy}
                  >
                    <option value="newest">Newest first</option>
                    <option value="oldest">Oldest first</option>
                    <option value="total-high">Highest total</option>
                    <option value="total-low">Lowest total</option>
                  </select>
                </label>
              </div>
            </header>

            <div className="orders-page__list" aria-busy={isLoading}>
              {isLoading
                ? Array.from({ length: ordersPageSize }, (_, index) => (
                    <OrderSummarySkeleton key={`orders-skeleton-${index}`} />
                  ))
                : orders.map((order) => <OrderSummaryCard key={order.id} order={order} />)}
              {!isLoading && hasActiveFilters && orders.length === 0 ? (
                <p className="orders-page__empty">No orders match the selected filters.</p>
              ) : null}
            </div>

            <Pagination
              currentPage={page}
              disabled={isLoading}
              label="Orders pagination"
              onNext={goToNextPage}
              onPrevious={goToPreviousPage}
              totalPages={totalPages}
            />
          </section>
        ) : null}
      </div>
    </PageSkeleton>
  );
}

function getSummaryText({
  error,
  hasActiveFilters,
  isLoading,
  totalCount,
}: {
  error: string | null;
  hasActiveFilters: boolean;
  isLoading: boolean;
  totalCount: number;
}) {
  if (error) {
    return 'Orders are unavailable right now.';
  }

  if (isLoading) {
    return 'Loading orders...';
  }

  if (totalCount === 0) {
    return hasActiveFilters ? 'No matching orders.' : 'No orders yet.';
  }

  return `${totalCount} orders placed`;
}
