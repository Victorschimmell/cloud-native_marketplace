import { useEffect, useState, type FormEvent } from 'react';
import Modal from '../../../shared/components/Modal';
import type { CurrencyCode } from '../../../shared/currency/currency';
import type { Category } from '../../products/types';
import type { SellerListing } from '../api/sellerApi';
import './SellerProductDialog.css';

export interface SellerProductEditFormValues {
  categoryId: string;
  description: string;
  imageUrl: string;
  inventoryQuantity: string;
  name: string;
  price: string;
  visibilityStatus: string;
}

interface SellerProductEditDialogProps {
  categories: Category[];
  currency: CurrencyCode;
  error: string | null;
  isPending: boolean;
  product: SellerListing;
  onClose: () => void;
  onSubmit: (values: SellerProductEditFormValues) => Promise<void> | void;
}

export default function SellerProductEditDialog({
  categories,
  currency,
  error,
  isPending,
  product,
  onClose,
  onSubmit,
}: SellerProductEditDialogProps) {
  const [form, setForm] = useState<SellerProductEditFormValues>(() => toFormValues(product));
  const hasCurrentCategory = categories.some((category) => category.id === form.categoryId);

  useEffect(() => {
    setForm(toFormValues(product));
  }, [product]);

  function updateField<K extends keyof SellerProductEditFormValues>(field: K, value: SellerProductEditFormValues[K]) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void onSubmit(form);
  }

  return (
    <Modal
      title="Edit product"
      subtitle={product.productName}
      onClose={onClose}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" disabled={isPending} onClick={onClose}>
            Cancel
          </button>
          <button type="submit" form="seller-product-edit-form" className="modal__button modal__button--primary" disabled={isPending}>
            {isPending ? 'Saving...' : 'Save changes'}
          </button>
        </>
      )}
    >
      {error ? <p className="seller-product-dialog__error">{error}</p> : null}

      <form id="seller-product-edit-form" className="seller-product-dialog__form" onSubmit={handleSubmit}>
        <label className="seller-product-dialog__field">
          <span>Product name</span>
          <input
            type="text"
            required
            value={form.name}
            disabled={isPending}
            onChange={(event) => updateField('name', event.target.value)}
          />
        </label>

        <label className="seller-product-dialog__field">
          <span>Description</span>
          <textarea
            required
            rows={4}
            value={form.description}
            disabled={isPending}
            onChange={(event) => updateField('description', event.target.value)}
          />
        </label>

        <div className="seller-product-dialog__form-row">
          <label className="seller-product-dialog__field">
            <span>Price ({currency})</span>
            <input
              type="number"
              step="0.01"
              min="0"
              required
              value={form.price}
              disabled={isPending}
              onChange={(event) => updateField('price', event.target.value)}
            />
          </label>

          <label className="seller-product-dialog__field">
            <span>Category</span>
            <select
              required
              value={form.categoryId}
              disabled={isPending}
              onChange={(event) => updateField('categoryId', event.target.value)}
            >
              <option value="" disabled>Select a category</option>
              {!hasCurrentCategory && form.categoryId ? (
                <option value={form.categoryId}>{product.categoryName ?? 'Current category'}</option>
              ) : null}
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.categoryNameEn ?? category.categoryNamePt}
                </option>
              ))}
            </select>
          </label>
        </div>

        <label className="seller-product-dialog__field">
          <span>Product image URL</span>
          <input
            type="url"
            value={form.imageUrl}
            disabled={isPending}
            onChange={(event) => updateField('imageUrl', event.target.value)}
          />
        </label>

        <div className="seller-product-dialog__form-row">
          <label className="seller-product-dialog__field">
            <span>Inventory quantity</span>
            <input
              type="number"
              min="0"
              step="1"
              required
              value={form.inventoryQuantity}
              disabled={isPending}
              onChange={(event) => updateField('inventoryQuantity', event.target.value)}
            />
          </label>

          <label className="seller-product-dialog__field">
            <span>Visibility status</span>
            <select
              required
              value={form.visibilityStatus}
              disabled={isPending}
              onChange={(event) => updateField('visibilityStatus', event.target.value)}
            >
              <option value="Draft">Draft</option>
              <option value="Published">Published</option>
            </select>
          </label>
        </div>
      </form>
    </Modal>
  );
}

function toFormValues(product: SellerListing): SellerProductEditFormValues {
  return {
    categoryId: product.categoryId,
    description: product.description,
    imageUrl: product.imageUrl ?? '',
    inventoryQuantity: String(product.inventoryQuantity),
    name: product.productName,
    price: String(product.listingPrice),
    visibilityStatus: product.visibilityStatus,
  };
}
