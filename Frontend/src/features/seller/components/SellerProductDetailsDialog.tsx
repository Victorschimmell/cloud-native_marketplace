import Modal from '../../../shared/components/Modal';
import type { SellerListing } from '../api/sellerApi';
import './SellerProductDialog.css';

interface SellerProductDetailsDialogProps {
  priceFormatter: Intl.NumberFormat;
  product: SellerListing;
  onClose: () => void;
  onDelete: (product: SellerListing) => void;
  onEdit: (product: SellerListing) => void;
}

export default function SellerProductDetailsDialog({
  priceFormatter,
  product,
  onClose,
  onDelete,
  onEdit,
}: SellerProductDetailsDialogProps) {
  return (
    <Modal
      title={product.productName}
      subtitle={product.categoryName ?? 'Uncategorized'}
      onClose={onClose}
      headerAction={(
        <button
          type="button"
          className="seller-product-dialog__icon-action"
          aria-label="Edit product"
          title="Edit product"
          onClick={() => onEdit(product)}
        >
          <svg aria-hidden="true" focusable="false" viewBox="0 0 24 24">
            <path d="M4 20h4.4L19.7 8.7a2.1 2.1 0 0 0 0-3L18.3 4.3a2.1 2.1 0 0 0-3 0L4 15.6V20Z" />
            <path d="m13.8 5.8 4.4 4.4" />
          </svg>
        </button>
      )}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" onClick={onClose}>
            Close
          </button>
          <button type="button" className="modal__button modal__button--danger" onClick={() => onDelete(product)}>
            Delete
          </button>
        </>
      )}
    >
      <div className="seller-product-dialog__details">
        {product.imageUrl ? (
          <img className="seller-product-dialog__image" src={product.imageUrl} alt="" />
        ) : null}

        <p className="seller-product-dialog__description">{product.description}</p>

        <dl className="seller-product-dialog__grid">
          <div>
            <dt>Price</dt>
            <dd>{priceFormatter.format(product.listingPrice)}</dd>
          </div>
          <div>
            <dt>Stock</dt>
            <dd>{product.inventoryQuantity}</dd>
          </div>
          <div>
            <dt>Status</dt>
            <dd>{product.visibilityStatus}</dd>
          </div>
          <div>
            <dt>Category</dt>
            <dd>{product.categoryName ?? '-'}</dd>
          </div>
        </dl>
      </div>
    </Modal>
  );
}
