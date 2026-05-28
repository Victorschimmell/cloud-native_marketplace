import Modal from '../../../shared/components/Modal';
import type { SellerListing } from '../api/sellerApi';
import './SellerProductDialog.css';

interface SellerProductDeleteDialogProps {
  error: string | null;
  isPending: boolean;
  product: SellerListing;
  onClose: () => void;
  onConfirm: () => void;
}

export default function SellerProductDeleteDialog({
  error,
  isPending,
  product,
  onClose,
  onConfirm,
}: SellerProductDeleteDialogProps) {
  return (
    <Modal
      title="Delete product"
      subtitle={product.productName}
      onClose={onClose}
      footer={(
        <>
          <button type="button" className="modal__button modal__button--secondary" disabled={isPending} onClick={onClose}>
            Cancel
          </button>
          <button type="button" className="modal__button modal__button--danger" disabled={isPending} onClick={onConfirm}>
            {isPending ? 'Deleting...' : 'Delete'}
          </button>
        </>
      )}
    >
      {error ? <p className="seller-product-dialog__error">{error}</p> : null}
      <p className="seller-product-dialog__confirm-text">
        Delete this product listing? This action cannot be undone.
      </p>
    </Modal>
  );
}
