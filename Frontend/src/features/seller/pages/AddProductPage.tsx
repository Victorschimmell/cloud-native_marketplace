import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import type { ProductCategory } from '../data/placeholderData';
import './AddProductPage.css';

const CATEGORIES: ProductCategory[] = ['Electronics', 'Clothing', 'Home', 'Books', 'Other'];

interface ProductFormState {
  name: string;
  description: string;
  price: string;
  category: ProductCategory;
  imageUrl: string;
  inStock: boolean;
}

const INITIAL_STATE: ProductFormState = {
  name: '',
  description: '',
  price: '',
  category: 'Electronics',
  imageUrl: '',
  inStock: true,
};

/**
 * Add New Product
 *
 * This might change later.
 * When the back-end is ready, replace the `console.log`
 * inside `handleSubmit` with a call to the seller products API and navigate
 * back to /seller/products on success.
 */
export default function AddProductPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<ProductFormState>(INITIAL_STATE);

  function updateField<K extends keyof ProductFormState>(field: K, value: ProductFormState[K]) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    // TODO: send to back-end. Current behaviour: log + return to dashboard.
    console.log('New product submitted', form);
    navigate('/seller/products');
  }

  return (
    <section className="add-product">
      <header className="add-product__header">
        <StoreIcon />
        <h1 className="add-product__title">Add New Product</h1>
      </header>

      <Link to="/seller/products" className="add-product__back">
        Back to Dashboard
      </Link>

      <div className="add-product__panel">
        <h2 className="add-product__panel-title">Product Information</h2>

        <form className="add-product__form" onSubmit={handleSubmit}>
          <label className="add-product__field">
            <span className="add-product__field-label">
              Product Name <span className="add-product__required">*</span>
            </span>
            <input
              className="add-product__input"
              type="text"
              required
              placeholder="Enter product name"
              value={form.name}
              onChange={(event) => updateField('name', event.target.value)}
            />
          </label>

          <label className="add-product__field">
            <span className="add-product__field-label">
              Description <span className="add-product__required">*</span>
            </span>
            <textarea
              className="add-product__textarea"
              required
              placeholder="Enter product description"
              value={form.description}
              onChange={(event) => updateField('description', event.target.value)}
            />
          </label>

          <div className="add-product__row">
            <label className="add-product__field">
              <span className="add-product__field-label">
                Price ($) <span className="add-product__required">*</span>
              </span>
              <input
                className="add-product__input"
                type="number"
                step="0.01"
                min="0"
                required
                placeholder="0.00"
                value={form.price}
                onChange={(event) => updateField('price', event.target.value)}
              />
            </label>

            <label className="add-product__field">
              <span className="add-product__field-label">
                Category <span className="add-product__required">*</span>
              </span>
              <select
                className="add-product__select"
                value={form.category}
                onChange={(event) => updateField('category', event.target.value as ProductCategory)}
              >
                {CATEGORIES.map((category) => (
                  <option key={category} value={category}>
                    {category}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <label className="add-product__field">
            <span className="add-product__field-label">
              Product Image URL <span className="add-product__required">*</span>
            </span>
            <input
              className="add-product__input"
              type="url"
              required
              placeholder="https://example.com/image.jpg"
              value={form.imageUrl}
              onChange={(event) => updateField('imageUrl', event.target.value)}
            />
          </label>

          <label className="add-product__checkbox">
            <input
              type="checkbox"
              checked={form.inStock}
              onChange={(event) => updateField('inStock', event.target.checked)}
            />
            Product is in stock
          </label>

          <div className="add-product__actions">
            <button type="submit" className="add-product__submit">
              Add Product
            </button>
            <Link to="/seller/products" className="add-product__cancel">
              Cancel
            </Link>
          </div>
        </form>
      </div>
    </section>
  );
}

function StoreIcon() {
  return (
    <svg
      className="add-product__header-icon"
      width="26"
      height="26"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M3 9l1-5h16l1 5" />
      <path d="M4 9v11h16V9" />
      <path d="M9 22V12h6v10" />
    </svg>
  );
}
