import { useEffect, useMemo, useState } from 'react';
import PageSkeleton from '../../../components/PageSkeleton';
import Pagination from '../../../shared/components/Pagination';
import StatusMessage from '../../../shared/components/StatusMessage';
import { getCurrencyLocale, useCurrency } from '../../../shared/currency/CurrencyContext';
import { productApi } from '../api/productApi';
import ProductCard from '../components/ProductCard';
import ProductCardSkeleton from '../components/ProductCardSkeleton';
import ProductFilters from '../components/ProductFilters';
import type { BrowseProduct, Category, ProductSortOption } from '../types';
import './ProductListPage.css';

const productPageSize = 15;

export default function ProductListPage() {
  const [products, setProducts] = useState<BrowseProduct[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [sortOption, setSortOption] = useState<ProductSortOption>('newest');
  const { currency } = useCurrency();

  const priceFormatter = useMemo(
    () => new Intl.NumberFormat(getCurrencyLocale(currency), { style: 'currency', currency }),
    [currency],
  );

  const totalPages = Math.max(1, Math.ceil(totalCount / productPageSize));

  useEffect(() => {
    const abortController = new AbortController();

    async function loadCategories() {
      try {
        const response = await productApi.getCategories(abortController.signal);
        setCategories(response);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }
      }
    }

    void loadCategories();

    return () => {
      abortController.abort();
    };
  }, []);

  useEffect(() => {
    const abortController = new AbortController();

    async function loadProducts() {
      try {
        setIsLoading(true);
        setError(null);
        const response = await productApi.getProducts(page, productPageSize, {
          categoryId: categoryFilter === 'all' ? undefined : categoryFilter,
          currency,
          search: searchTerm.trim() || undefined,
          signal: abortController.signal,
          sort: sortOption,
        });

        setProducts(response.items);
        setTotalCount(response.totalCount);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }

        setError('Products could not be loaded right now.');
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadProducts();

    return () => {
      abortController.abort();
    };
  }, [categoryFilter, currency, page, searchTerm, sortOption]);

  function goToPreviousPage() {
    setPage((currentPage) => Math.max(1, currentPage - 1));
  }

  function goToNextPage() {
    setPage((currentPage) => Math.min(totalPages, currentPage + 1));
  }

  function updateSearchTerm(value: string) {
    setSearchTerm(value);
    setPage(1);
  }

  function updateCategoryFilter(value: string) {
    setCategoryFilter(value);
    setPage(1);
  }

  function updateSortOption(value: ProductSortOption) {
    setSortOption(value);
    setPage(1);
  }

  return (
    <PageSkeleton
      summary={isLoading ? 'Loading products...' : `${totalCount} products available`}
      title="Browse Products"
      titleId="product-list-page-title"
    >
      <ProductFilters
        categories={categories}
        categoryFilter={categoryFilter}
        onCategoryChange={updateCategoryFilter}
        onSearchChange={updateSearchTerm}
        onSortChange={updateSortOption}
        searchTerm={searchTerm}
        sortOption={sortOption}
      />

      {error && (
        <StatusMessage variant="error">
          {error}
        </StatusMessage>
      )}

      {!isLoading && !error && products.length === 0 && (
        <StatusMessage>
          No products are available yet.
        </StatusMessage>
      )}

      <div className="product-list-page__grid" aria-busy={isLoading}>
        {isLoading
          ? Array.from({ length: productPageSize }, (_, index) => (
              <ProductCardSkeleton key={index} />
            ))
          : products.map((product) => (
              <ProductCard key={product.listingId} priceFormatter={priceFormatter} product={product} />
            ))}
      </div>

      {!error && (
        <Pagination
          currentPage={page}
          disabled={isLoading}
          label="Products pagination"
          onNext={goToNextPage}
          onPrevious={goToPreviousPage}
          totalPages={totalPages}
        />
      )}
    </PageSkeleton>
  );
}
