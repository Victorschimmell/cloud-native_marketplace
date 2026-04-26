import { useEffect, useMemo, useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { cartApi } from '../../cart/api/cartApi';
import { productApi } from '../api/productApi';
import type { ProductDetails } from '../types';
import './ProductDetailsPage.css';

export default function ProductDetailsPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const [product, setProduct] = useState<ProductDetails | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isAdding, setIsAdding] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cartMessage, setCartMessage] = useState<string | null>(null);
  const [cartMessageVariant, setCartMessageVariant] = useState<'success' | 'error'>('success');
  const [quantity, setQuantity] = useState(1);

  const listingId = searchParams.get('listingId');
  const priceFormatter = useMemo(
    () => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }),
    [],
  );

  useEffect(() => {
    const abortController = new AbortController();

    async function loadProduct() {
      if (!id) {
        setError('Product could not be found.');
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        setError(null);
        const response = await productApi.getProduct(id, listingId, abortController.signal);
        setProduct(response);
      } catch (requestError) {
        if (requestError instanceof DOMException && requestError.name === 'AbortError') {
          return;
        }

        setError('Product details could not be loaded right now.');
      } finally {
        if (!abortController.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadProduct();

    return () => {
      abortController.abort();
    };
  }, [id, listingId]);

  async function addToCart() {
    if (!product) {
      return;
    }

    try {
      setIsAdding(true);
      setCartMessage(null);
      await cartApi.addItem(product.listingId, quantity);
      setCartMessageVariant('success');
      setCartMessage('Added to cart.');
    } catch {
      setCartMessageVariant('error');
      setCartMessage('Could not add this product to the cart.');
    } finally {
      setIsAdding(false);
    }
  }

  function updateQuantity(value: string) {
    const nextQuantity = Number(value);
    setQuantity(Number.isFinite(nextQuantity) && nextQuantity > 0 ? nextQuantity : 1);
  }

  const title = product?.productName ?? 'Product Details';
  const summary = product?.categoryName ?? (isLoading ? 'Loading product...' : undefined);
  const stockText = product && product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Stock pending';

  return (
    <PageSkeleton summary={summary} title={title} titleId="product-details-page-title">
      <div className="product-details-page">
        <Link className="product-details-page__back-link" to="/products">
          Back to products
        </Link>

        {error ? (
          <div className="product-details-page__notice" role="alert">
            {error}
          </div>
        ) : null}

        {isLoading ? (
          <div className="product-details-page__panel product-details-page__panel--loading" />
        ) : null}

        {!isLoading && product ? (
          <div className="product-details-page__layout">
            <div className="product-details-page__media" aria-hidden="true">
              <span>{product.productPhotosQty > 0 ? `${product.productPhotosQty} photos` : 'No image'}</span>
            </div>

            <section className="product-details-page__panel" aria-labelledby="product-details-page-title">
              <p className="product-details-page__description">{product.description}</p>

              <dl className="product-details-page__facts">
                <div>
                  <dt>Stock</dt>
                  <dd>{stockText}</dd>
                </div>
                <div>
                  <dt>Weight</dt>
                  <dd>{product.productWeightG} g</dd>
                </div>
                <div>
                  <dt>Dimensions</dt>
                  <dd>
                    {product.productLengthCm} x {product.productWidthCm} x {product.productHeightCm} cm
                  </dd>
                </div>
              </dl>

              <div className="product-details-page__purchase">
                <strong className="product-details-page__price">{priceFormatter.format(product.price)}</strong>
                <label className="product-details-page__quantity">
                  Quantity
                  <input
                    min="1"
                    onChange={(event) => updateQuantity(event.target.value)}
                    type="number"
                    value={quantity}
                  />
                </label>
                <button disabled={isAdding} onClick={addToCart} type="button">
                  {isAdding ? 'Adding...' : 'Add to cart'}
                </button>
              </div>

              {cartMessage ? (
                <p
                  className={`product-details-page__cart-message product-details-page__cart-message--${cartMessageVariant}`}
                  role={cartMessageVariant === 'success' ? 'status' : 'alert'}
                >
                  {cartMessage}
                  {cartMessageVariant === 'success' ? <> <Link to="/cart">View cart</Link></> : null}
                </p>
              ) : null}
            </section>
          </div>
        ) : null}
      </div>
    </PageSkeleton>
  );
}
