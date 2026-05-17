import type { SellerProduct } from '../data/placeholderData';

interface ProductInventoryTableProps {
  priceFormatter: Intl.NumberFormat;
  products: SellerProduct[];
}

export default function ProductInventoryTable({ priceFormatter, products }: ProductInventoryTableProps) {
  return (
    <div className="seller-dashboard__panel">
      <div className="seller-dashboard__panel-header">
        <h2 className="seller-dashboard__panel-title">Product Inventory</h2>
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
              <th>Rating</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {products.map((product) => (
              <ProductRow key={product.id} priceFormatter={priceFormatter} product={product} />
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

function ProductRow({ priceFormatter, product }: { priceFormatter: Intl.NumberFormat; product: SellerProduct }) {
  // No Operation Handlers for now , will be wired to the API later.
  function handleEdit() {
    console.log('Edit product', product.id);
  }

  function handleDelete() {
    console.log('Delete product', product.id);
  }

  return (
    <tr>
      <td>
        <div className="seller-dashboard__product-cell">
          <img
            className="seller-dashboard__product-image"
            src={product.imageUrl}
            alt=""
          />
          <span>{product.name}</span>
        </div>
      </td>
      <td>{product.category}</td>
      <td className="seller-dashboard__price">{priceFormatter.format(product.price)}</td>
      <td>
        <span
          className={`seller-dashboard__stock seller-dashboard__stock--${product.inStock ? 'in' : 'out'}`}
        >
          {product.inStock ? 'In Stock' : 'Out of Stock'}
        </span>
      </td>
      <td>
        {product.rating.toFixed(1)} ({product.ratingCount})
      </td>
      <td>
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
