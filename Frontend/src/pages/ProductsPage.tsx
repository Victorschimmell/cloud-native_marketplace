import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import Pagination from '../components/Pagination';
import StatusMessage from '../components/StatusMessage';
import api from '../services/api';
import type { BrowseProduct } from '../types';
import './ProductsPage.css';

const productPageSize = 10;

type SortOption = 'default' | 'price-asc' | 'price-desc' | 'name-asc';

export default function ProductsPage() {
  const [products, setProducts] = useState<BrowseProduct[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [sortOption, setSortOption] = useState<SortOption>('default');

  const priceFormatter = useMemo(
    () => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }),
    [],
  );

  const totalPages = Math.max(1, Math.ceil(totalCount / productPageSize));

  const categoryOptions = useMemo(() => {
    const categories = new Set(
      products
        .map((product) => product.categoryName)
        .filter((categoryName): categoryName is string => Boolean(categoryName)),
    );

    return Array.from(categories).sort((first, second) => first.localeCompare(second));
  }, [products]);

  const visibleProducts = useMemo(() => {
    const normalizedSearchTerm = searchTerm.trim().toLowerCase();

    const filteredProducts = products.filter((product) => {
      const matchesCategory = categoryFilter === 'all' || product.categoryName === categoryFilter;
      const matchesSearch =
        normalizedSearchTerm.length === 0 ||
        product.productName.toLowerCase().includes(normalizedSearchTerm) ||
        product.description.toLowerCase().includes(normalizedSearchTerm) ||
        (product.categoryName?.toLowerCase().includes(normalizedSearchTerm) ?? false);

      return matchesCategory && matchesSearch;
    });

    return [...filteredProducts].sort((first, second) => {
      switch (sortOption) {
        case 'price-asc':
          return first.price - second.price;
        case 'price-desc':
          return second.price - first.price;
        case 'name-asc':
          return first.productName.localeCompare(second.productName);
        default:
          return 0;
      }
    });
  }, [categoryFilter, products, searchTerm, sortOption]);

  useEffect(() => {
    const abortController = new AbortController();

    async function loadProducts() {
      try {
        setIsLoading(true);
        setError(null);
        const response = await api.getProducts(page, productPageSize, abortController.signal);

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
  }, [page]);

  function goToPreviousPage() {
    setPage((currentPage) => Math.max(1, currentPage - 1));
  }

  function goToNextPage() {
    setPage((currentPage) => Math.min(totalPages, currentPage + 1));
  }

  return (
    <section className="products-page" aria-labelledby="products-page-title">
      <header className="products-page__header">
        <div>
          <h1 id="products-page-title" className="products-page__title">Browse Products</h1>
          <p className="products-page__summary">
            {isLoading ? 'Loading products...' : `${totalCount} products available`}
          </p>
        </div>
      </header>

      <div className="products-page__controls">
        <label className="products-page__control">
          <span>Search</span>
          <input
            onChange={(event) => setSearchTerm(event.target.value)}
            placeholder="Product or category"
            type="search"
            value={searchTerm}
          />
        </label>

        <label className="products-page__control">
          <span>Category</span>
          <select onChange={(event) => setCategoryFilter(event.target.value)} value={categoryFilter}>
            <option value="all">All categories</option>
            {categoryOptions.map((categoryName) => (
              <option key={categoryName} value={categoryName}>
                {categoryName}
              </option>
            ))}
          </select>
        </label>

        <label className="products-page__control">
          <span>Sort</span>
          <select onChange={(event) => setSortOption(event.target.value as SortOption)} value={sortOption}>
            <option value="default">Newest</option>
            <option value="price-asc">Price low to high</option>
            <option value="price-desc">Price high to low</option>
            <option value="name-asc">Name A to Z</option>
          </select>
        </label>
      </div>

      {error && (
        <StatusMessage variant="error">
          {error}
        </StatusMessage>
      )}

      {!isLoading && !error && visibleProducts.length === 0 && (
        <StatusMessage>
          No products are available yet.
        </StatusMessage>
      )}

      <div className="products-page__grid" aria-busy={isLoading}>
        {isLoading
          ? Array.from({ length: productPageSize }, (_, index) => (
              <article className="product-card product-card--loading" key={index}>
                <div className="product-card__skeleton product-card__skeleton--category" />
                <div className="product-card__skeleton product-card__skeleton--title" />
                <div className="product-card__skeleton product-card__skeleton--text" />
                <div className="product-card__skeleton product-card__skeleton--text product-card__skeleton--text-short" />
                <div className="product-card__skeleton product-card__skeleton--meta" />
              </article>
            ))
          : visibleProducts.map((product) => {
              const stockText = product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Stock pending';

              return (
                <Link
                  aria-label={`View ${product.productName}`}
                  className="product-card"
                  key={product.listingId}
                  to={`/products/${product.productId}?listingId=${product.listingId}`}
                >
                  <div className="product-card__category">
                    {product.categoryName ?? 'Marketplace'}
                  </div>
                  <h2 className="product-card__title">
                    {product.productName}
                  </h2>
                  <p className="product-card__description">
                    {product.description}
                  </p>
                  <div className="product-card__meta">
                    <strong className="product-card__price">{priceFormatter.format(product.price)}</strong>
                    <span className={`product-card__stock${product.stockQuantity === 0 ? ' product-card__stock--empty' : ''}`}>
                      {stockText}
                    </span>
                  </div>
                </Link>
              );
            })}
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
    </section>
  );
}
