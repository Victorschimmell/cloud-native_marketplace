import type { SellerListing } from '../api/sellerApi';
import { sellerApi } from '../api/sellerApi';
import { useNavigate } from 'react-router-dom';

interface ProductInventoryTableProps {
  priceFormatter: Intl.NumberFormat;
  products: SellerListing[];
  onDeleteListing: (listingId: string) => void;
}

export default function ProductInventoryTable({ priceFormatter, products, onDeleteListing }: ProductInventoryTableProps) {
  return (
    <div className="seller-dashboard__panel">
      <div className="seller-dashboard__panel-header">
        <h2 className="seller-dashboard__panel-title">Product Inventory</h2>
        <p className="seller-dashboard__panel-subtitle">Review product status, stock and pricing</p>
      </div>

      {products.length === 0 ? (
        <p className="seller-dashboard__empty">No products yet. Add a product to create one.</p>
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
            {products.map((product) => (
              <ProductRow key={product.listingId} priceFormatter={priceFormatter} product={product} onDeleteListing={onDeleteListing} />
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

function ProductRow({ priceFormatter, product, onDeleteListing }: { priceFormatter: Intl.NumberFormat; product: SellerListing; onDeleteListing: (listingId: string) => void }) {
  const navigate = useNavigate();

  function handleEdit() {
    navigate(`/seller/products/${product.listingId}/edit`, { state: { listing: product } });
  }

  async function handleDelete() {
    if (!window.confirm(`Are you sure you want to delete "${product.productName}"? This cannot be undone.`)) return;
    try {
      await sellerApi.deleteProduct(product.productId);
      onDeleteListing(product.listingId);
    } catch {
      alert('Failed to delete product. Please try again.');
    }
  }

  return (
    <tr>
      <td data-label="Product">
        <span>{product.productName}</span>
      </td>
      <td data-label="Category">{product.categoryName ?? '—'}</td>
      <td className="seller-dashboard__price" data-label="Price">{priceFormatter.format(product.listingPrice)}</td>
      <td data-label="Stock">{product.inventoryQuantity}</td>
      <td data-label="Status">{product.visibilityStatus}</td>
      <td data-label="Actions">
        <div className="seller-dashboard__row-actions">
          <button
            type="button"
            className="seller-dashboard__action seller-dashboard__action--edit"
            onClick={handleEdit}
          >
            Edit
          </button>
          <button
            type="button"
            className="seller-dashboard__action seller-dashboard__action--delete"
            onClick={handleDelete}
          >
            Delete
          </button>
        </div>
      </td>
    </tr>
  );
}

