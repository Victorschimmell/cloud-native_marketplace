import { useState, useEffect, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { productApi } from '../../products/api/productApi';
import { sellerApi } from '../api/sellerApi';
import type { Category } from '../../products/types';
import './AddProductPage.css';

interface ProductFormState {
  name: string;
  description: string;
  price: string;
  categoryId: string;
  imageUrl: string;
  inventoryQuantity: string;
}

const INITIAL_STATE: ProductFormState = {
  name: '',
  description: '',
  price: '',
  categoryId: '',
  imageUrl: '',
  inventoryQuantity: '0',
};

export default function AddProductPage() {
  const navigate = useNavigate();
  const { currency } = useCurrency();
  const [form, setForm] = useState<ProductFormState>(INITIAL_STATE);
  const [categories, setCategories] = useState<Category[]>([]);
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    productApi.getCategories(controller.signal).then(setCategories).catch(() => {});
    return () => controller.abort();
  }, []);

  function updateField<K extends keyof ProductFormState>(field: K, value: ProductFormState[K]) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitError(null);
    try {
      await sellerApi.createProduct({
        categoryId: form.categoryId,
        productName: form.name,
        description: form.description,
        imageUrl: normalizeOptional(form.imageUrl),
        price: parseFloat(form.price),
        inventoryQuantity: parseInt(form.inventoryQuantity, 10),
      });
      navigate('/seller/products');
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : 'Failed to create product.');
    }
  }

  return (
    <PageSkeleton
      summary="Create a product listing for your seller catalog."
      title="Add New Product"
      titleId="add-product-page-title"
    >
      <section className="add-product" aria-labelledby="add-product-page-title">
        <div className="add-product__navigation">
          <Link to="/seller/products" className="add-product__back">
            Back to Dashboard
          </Link>
        </div>

        <div className="add-product__panel">
          <h2 className="add-product__panel-title">Product Information</h2>

          {submitError && <p className="add-product__error">{submitError}</p>}

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
                  Price ({currency}) <span className="add-product__required">*</span>
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
                  value={form.categoryId}
                  required
                  onChange={(event) => updateField('categoryId', event.target.value)}
                >
                  <option value="" disabled>Select a category</option>
                  {categories.map((category) => (
                    <option key={category.id} value={category.id}>
                      {category.categoryNameEn ?? category.categoryNamePt}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            <label className="add-product__field">
              <span className="add-product__field-label">
                Product Image URL
              </span>
              <input
                className="add-product__input"
                type="url"
                placeholder="https://example.com/image.jpg"
                value={form.imageUrl}
                onChange={(event) => updateField('imageUrl', event.target.value)}
              />
            </label>

            <label className="add-product__field">
              <span className="add-product__field-label">
                Inventory Quantity <span className="add-product__required">*</span>
              </span>
              <input
                className="add-product__input"
                type="number"
                min="0"
                step="1"
                required
                placeholder="0"
                value={form.inventoryQuantity}
                onChange={(event) => updateField('inventoryQuantity', event.target.value)}
              />
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
    </PageSkeleton>
  );
}

function normalizeOptional(value: string): string | null {
  const normalized = value.trim();
  return normalized.length > 0 ? normalized : null;
}
