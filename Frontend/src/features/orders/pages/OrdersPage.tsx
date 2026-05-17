import { useEffect, useMemo, useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import Pagination from '../../../shared/components/Pagination';
import StatusMessage from '../../../shared/components/StatusMessage';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { useAuth } from '../../auth/useAuth';
import { orderApi } from '../api/orderApi';
import { OrderSummaryCard, OrderSummarySkeleton } from '../components';
import type { OrderSummary } from '../types';
import './OrdersPage.css';

const ordersPageSize = 5;

export default function OrdersPage() {
  const { currency } = useCurrency();
  const { user } = useAuth();
  const [orders, setOrders] = useState<OrderSummary[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const totalPages = useMemo(() => Math.max(1, Math.ceil(totalCount / ordersPageSize)), [totalCount]);

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
  }, [currency, page, user]);

  function goToPreviousPage() {
    setPage((currentPage) => Math.max(1, currentPage - 1));
  }

  function goToNextPage() {
    setPage((currentPage) => Math.min(totalPages, currentPage + 1));
  }

  const summary = getSummaryText({ error, isLoading, totalCount });

  return (
    <PageSkeleton summary={summary} title="Orders" titleId="orders-page-title">
      <div className="orders-page">
        {error ? (
          <StatusMessage variant="error">{error}</StatusMessage>
        ) : null}

        {!isLoading && !error && orders.length === 0 ? (
          <StatusMessage>No orders yet. Items you buy will show up here.</StatusMessage>
        ) : null}

        <div className="orders-page__list" aria-busy={isLoading}>
          {isLoading
            ? Array.from({ length: ordersPageSize }, (_, index) => (
                <OrderSummarySkeleton key={`orders-skeleton-${index}`} />
              ))
            : orders.map((order) => <OrderSummaryCard key={order.id} order={order} />)}
        </div>

        {!error ? (
          <Pagination
            currentPage={page}
            disabled={isLoading}
            label="Orders pagination"
            onNext={goToNextPage}
            onPrevious={goToPreviousPage}
            totalPages={totalPages}
          />
        ) : null}
      </div>
    </PageSkeleton>
  );
}

function getSummaryText({ error, isLoading, totalCount }: { error: string | null; isLoading: boolean; totalCount: number }) {
  if (error) {
    return 'Orders are unavailable right now.';
  }

  if (isLoading) {
    return 'Loading orders...';
  }

  if (totalCount === 0) {
    return 'No orders yet.';
  }

  return `${totalCount} orders placed`;
}
