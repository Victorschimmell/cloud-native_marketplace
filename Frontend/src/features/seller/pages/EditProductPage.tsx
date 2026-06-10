import { useState, useEffect, type FormEvent } from 'react';
import { Link, useNavigate, useLocation, useParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { productApi } from '../../products/api/productApi';
import { sellerApi } from '../api/sellerApi';
import type { Category } from '../../products/types';
import type { SellerListing } from '../api/sellerApi';
import './AddProductPage.css';

interface ProductFormState {
  name: string;
  description: string;
  price: string;
  categoryId: string;
  imageUrl: string;
  inventoryQuantity: string;
  visibilityStatus: string;
}

export default function EditProductPage() {
  const navigate = useNavigate();
  const { state } = useLocation() as { state: { listing: SellerListing } | null };
  const { listingId } = useParams<{ listingId: string }>();
  const { currency } = useCurrency();

  const [listing, setListing] = useState<SellerListing | null>(state?.listing ?? null);
  const [loadingListing, setLoadingListing] = useState(!state?.listing);

  const [form, setForm] = useState<ProductFormState>({
    name: state?.listing?.productName ?? '',
    description: state?.listing?.description ?? '',
    price: String(state?.listing?.listingPrice ?? ''),
    categoryId: state?.listing?.categoryId ?? '',
    imageUrl: state?.listing?.imageUrl ?? '',
    inventoryQuantity: String(state?.listing?.inventoryQuantity ?? 0),
    visibilityStatus: state?.listing?.visibilityStatus ?? 'Draft',
  });
  const [categories, setCategories] = useState<Category[]>([]);
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    productApi.getCategories(controller.signal).then(setCategories).catch(() => {});
    return () => controller.abort();
  }, []);

  useEffect(() => {
    if (state?.listing || !listingId) return;
    const controller = new AbortController();
    sellerApi
      .getMyListings(currency, controller.signal)
      .then((listings) => {
        const found = listings.find((l) => l.listingId === listingId) ?? null;
        setListing(found);
        if (found) {
          setForm({
            name: found.productName,
            description: found.description,
            price: String(found.listingPrice),
            categoryId: found.categoryId,
            imageUrl: found.imageUrl ?? '',
            inventoryQuantity: String(found.inventoryQuantity),
            visibilityStatus: found.visibilityStatus,
          });
        }
      })
      .catch(() => {})
      .finally(() => {
        if (!controller.signal.aborted) {
          setLoadingListing(false);
        }
      });
    return () => controller.abort();
  }, [currency, listingId, state?.listing]);

  function updateField<K extends keyof ProductFormState>(field: K, value: ProductFormState[K]) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!listing) return;
    setSubmitError(null);
    try {
      await sellerApi.updateProduct(listing.listingId, {
        categoryId: form.categoryId,
        productName: form.name,
        description: form.description,
        imageUrl: normalizeOptional(form.imageUrl),
        price: parseFloat(form.price),
        inventoryQuantity: parseInt(form.inventoryQuantity, 10),
        visibilityStatus: form.visibilityStatus,
      }, currency);
      navigate('/seller/products');
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : 'Failed to update product.');
    }
  }

  if (loadingListing) {
    return (
      <PageSkeleton title="Edit Product" titleId="edit-product-page-title" summary="">
        <section className="add-product">
          <p>Loading product...</p>
        </section>
      </PageSkeleton>
    );
  }

  if (!listing) {
    return (
      <PageSkeleton title="Edit Product" titleId="edit-product-page-title" summary="">
        <section className="add-product">
          <p>Product not found. <Link to="/seller/products">Back to products</Link></p>
        </section>
      </PageSkeleton>
    );
  }

  return (
    <PageSkeleton title="Edit Product" titleId="edit-product-page-title" summary="Update your product listing details.">
      <section className="add-product" aria-labelledby="edit-product-page-title">
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

            <label className="add-product__field">
              <span className="add-product__field-label">
                Visibility Status <span className="add-product__required">*</span>
              </span>
              <select
                className="add-product__select"
                value={form.visibilityStatus}
                required
                onChange={(event) => updateField('visibilityStatus', event.target.value)}
              >
                <option value="Draft">Draft</option>
                <option value="Published">Published</option>
              </select>
            </label>

            <div className="add-product__actions">
              <button type="submit" className="add-product__submit">
                Save Changes
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
