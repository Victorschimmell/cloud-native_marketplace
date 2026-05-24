import Modal from '../../../shared/components/Modal';
import type { SellerListing } from '../api/sellerApi';
import './SellerProductDialog.css';

interface SellerProductPublishDialogProps {
  error: string | null;
  isPending: boolean;
  product: SellerListing;
  onClose: () => void;
  onConfirm: () => void;
}

export default function SellerProductPublishDialog({
  error,
  isPending,
  product,
  onClose,
  onConfirm,
}: SellerProductPublishDialogProps) {
  return (
    <Modal
      title="Publish product"
      subtitle={product.productName}
      onClose={onClose}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" disabled={isPending} onClick={onClose}>
            Cancel
          </button>
          <button type="button" className="modal__button modal__button--success" disabled={isPending} onClick={onConfirm}>
            {isPending ? 'Publishing...' : 'Publish'}
          </button>
        </>
      )}
    >
      {error ? <p className="seller-product-dialog__error">{error}</p> : null}
      <p className="seller-product-dialog__confirm-text">
        Publish this product listing? Customers will be able to see it in the marketplace.
      </p>
    </Modal>
  );
}
