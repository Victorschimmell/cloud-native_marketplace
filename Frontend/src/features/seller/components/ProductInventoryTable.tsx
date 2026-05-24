import { useMemo, useState } from 'react';
import type { SellerListing } from '../api/sellerApi';

interface ProductInventoryTableProps {
  pendingListingId?: string | null;
  priceFormatter: Intl.NumberFormat;
  products: SellerListing[];
  onDeleteProduct: (product: SellerListing) => void;
  onEditProduct: (product: SellerListing) => void;
  onOpenProduct: (product: SellerListing) => void;
}

type ProductSortOption = 'name' | 'category' | 'price-high' | 'price-low' | 'stock-high' | 'stock-low' | 'status';

export default function ProductInventoryTable({
  pendingListingId = null,
  priceFormatter,
  products,
  onDeleteProduct,
  onEditProduct,
  onOpenProduct,
}: ProductInventoryTableProps) {
  const [statusFilter, setStatusFilter] = useState('all');
  const [sortBy, setSortBy] = useState<ProductSortOption>('name');

  const statusOptions = useMemo(() => {
    return Array.from(new Set(products.map((product) => product.visibilityStatus))).sort((a, b) => a.localeCompare(b));
  }, [products]);

  const visibleProducts = useMemo(() => {
    const filteredProducts = products.filter((product) => {
      const matchesStatus = statusFilter === 'all' || product.visibilityStatus === statusFilter;

      return matchesStatus;
    });

    return sortProducts(filteredProducts, sortBy);
  }, [products, sortBy, statusFilter]);

  return (
    <div className="seller-dashboard__panel">
      <div className="seller-dashboard__panel-header">
        <div>
          <h2 className="seller-dashboard__panel-title">Product Inventory</h2>
          <p className="seller-dashboard__panel-subtitle">Filter and sort product status, stock and pricing</p>
        </div>

        <div className="seller-dashboard__filters">
          <label className="seller-dashboard__filter">
            Status:
            <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
              <option value="all">All</option>
              {statusOptions.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </label>

          <label className="seller-dashboard__filter">
            Sort:
            <select value={sortBy} onChange={(event) => setSortBy(event.target.value as ProductSortOption)}>
              <option value="name">Name</option>
              <option value="category">Category</option>
              <option value="price-high">Price: high to low</option>
              <option value="price-low">Price: low to high</option>
              <option value="stock-high">Stock: high to low</option>
              <option value="stock-low">Stock: low to high</option>
              <option value="status">Status</option>
            </select>
          </label>
        </div>
      </div>

      {products.length === 0 ? (
        <p className="seller-dashboard__empty">No products yet. Add a product to create one.</p>
      ) : visibleProducts.length === 0 ? (
        <p className="seller-dashboard__empty">No products match the current filters.</p>
      ) : (
        <table className="seller-dashboard__table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Category</th>
              <th>Price</th>
              <th>Stock</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {visibleProducts.map((product) => (
              <ProductRow
                key={product.listingId}
                isPending={pendingListingId === product.listingId}
                priceFormatter={priceFormatter}
                product={product}
                onDeleteProduct={onDeleteProduct}
                onEditProduct={onEditProduct}
                onOpenProduct={onOpenProduct}
              />
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

function ProductRow({
  isPending,
  priceFormatter,
  product,
  onDeleteProduct,
  onEditProduct,
  onOpenProduct,
}: {
  isPending: boolean;
  priceFormatter: Intl.NumberFormat;
  product: SellerListing;
  onDeleteProduct: (product: SellerListing) => void;
  onEditProduct: (product: SellerListing) => void;
  onOpenProduct: (product: SellerListing) => void;
}) {
  return (
    <tr>
      <td data-label="Product">
        <button
          type="button"
          className="seller-dashboard__product-button"
          onClick={() => onOpenProduct(product)}
        >
          {product.productName}
        </button>
      </td>
      <td data-label="Category">{product.categoryName ?? '-'}</td>
      <td className="seller-dashboard__price" data-label="Price">{priceFormatter.format(product.listingPrice)}</td>
      <td data-label="Stock">{product.inventoryQuantity}</td>
      <td data-label="Status">{product.visibilityStatus}</td>
      <td data-label="Actions">
        <div className="seller-dashboard__row-actions">
          <button
            type="button"
            className="seller-dashboard__action seller-dashboard__action--edit"
            disabled={isPending}
            onClick={() => onEditProduct(product)}
          >
            Edit
          </button>
          <button
            type="button"
            className="seller-dashboard__action seller-dashboard__action--delete"
            disabled={isPending}
            onClick={() => onDeleteProduct(product)}
          >
            {isPending ? 'Working...' : 'Delete'}
          </button>
        </div>
      </td>
    </tr>
  );
}

function sortProducts(products: SellerListing[], sortBy: ProductSortOption): SellerListing[] {
  return [...products].sort((a, b) => {
    switch (sortBy) {
      case 'category':
        return (a.categoryName ?? 'Uncategorized').localeCompare(b.categoryName ?? 'Uncategorized');
      case 'price-high':
        return b.listingPrice - a.listingPrice;
      case 'price-low':
        return a.listingPrice - b.listingPrice;
      case 'stock-high':
        return b.inventoryQuantity - a.inventoryQuantity;
      case 'stock-low':
        return a.inventoryQuantity - b.inventoryQuantity;
      case 'status':
        return a.visibilityStatus.localeCompare(b.visibilityStatus);
      default:
        return a.productName.localeCompare(b.productName);
    }
  });
}
