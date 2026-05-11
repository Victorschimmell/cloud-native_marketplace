import { useEffect, useMemo, useState } from 'react';
import { Link, useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { useAuth } from '../../auth/useAuth';
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
  const { currency } = useCurrency();
  const { capabilities, isAuthenticated } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const priceFormatter = useMemo(
    () => new Intl.NumberFormat(getCurrencyLocale(product?.currencyCode ?? currency), { style: 'currency', currency: product?.currencyCode ?? currency }),
    [currency, product?.currencyCode],
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
        const response = await productApi.getProduct(id, listingId, currency, abortController.signal);
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
  }, [currency, id, listingId]);

  useEffect(() => {
    if (!product) {
      return;
    }

    setQuantity((currentQuantity) => {
      if (product.stockQuantity <= 0) {
        return 1;
      }

      return Math.min(Math.max(currentQuantity, 1), product.stockQuantity);
    });
  }, [product]);

  async function addToCart() {
    if (!product || product.stockQuantity <= 0) {
      return;
    }

    if (!isAuthenticated) {
      const returnTo = `${location.pathname}${location.search}${location.hash}`;
      navigate(`/login?returnTo=${encodeURIComponent(returnTo)}`);
      return;
    }

    if (!capabilities.isCustomer) {
      setCartMessageVariant('error');
      setCartMessage('Only customer accounts can add products to the cart.');
      return;
    }

    try {
      setIsAdding(true);
      setCartMessage(null);
      await cartApi.addItem(product.listingId, quantity, currency);
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
    if (!Number.isFinite(nextQuantity) || nextQuantity < 1) {
      setQuantity(1);
      return;
    }

    setQuantity(product?.stockQuantity ? Math.min(nextQuantity, product.stockQuantity) : nextQuantity);
  }

  const title = product?.productName ?? 'Product Details';
  const summary = product?.categoryName ?? (isLoading ? 'Loading product...' : undefined);
  const isInStock = Boolean(product && product.stockQuantity > 0);
  const canBuyProduct = !isAuthenticated || capabilities.isCustomer;
  const stockText = isInStock && product ? `${product.stockQuantity} in stock` : 'Out of stock';

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
              <span>No image</span>
            </div>

            <section className="product-details-page__panel" aria-labelledby="product-details-page-title">
              <section className="product-details-page__description-section">
                <h2>Description</h2>
                <p className="product-details-page__description">{product.description}</p>
              </section>

              <div className="product-details-page__seller-review">
                <span className="product-details-page__seller">
                  Sold by <strong>{product.sellerName}</strong>
                </span>
                {/* Temporary placeholder until a product review summary endpoint is implemented. */}
                <span className="product-details-page__rating" aria-label="Review summary placeholder: 4.6 out of 5 stars from 4,009 reviews">
                  <strong>4.6</strong>
                  <span className="product-details-page__stars" aria-hidden="true">
                    <span className="product-details-page__star product-details-page__star--filled" />
                    <span className="product-details-page__star product-details-page__star--filled" />
                    <span className="product-details-page__star product-details-page__star--filled" />
                    <span className="product-details-page__star product-details-page__star--filled" />
                    <span className="product-details-page__star product-details-page__star--half" />
                  </span>
                  <span className="product-details-page__rating-caret" aria-hidden="true" />
                  <span className="product-details-page__rating-count">(4,009)</span>
                </span>
              </div>

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
                    disabled={!isInStock || !canBuyProduct}
                    max={product.stockQuantity > 0 ? product.stockQuantity : undefined}
                    min="1"
                    onChange={(event) => updateQuantity(event.target.value)}
                    type="number"
                    value={quantity}
                  />
                </label>
                <button
                  className={isAdding ? 'product-details-page__add-button--loading' : undefined}
                  disabled={isAdding || !isInStock || !canBuyProduct}
                  onClick={addToCart}
                  type="button"
                >
                  {isAdding ? 'Adding...' : isInStock && canBuyProduct ? 'Add to cart' : isInStock ? 'Customer accounts only' : 'Out of stock'}
                </button>
              </div>

              <div className="product-details-page__cart-status">
                {cartMessage ? (
                  <p
                    className={`product-details-page__cart-message product-details-page__cart-message--${cartMessageVariant}`}
                    role={cartMessageVariant === 'success' ? 'status' : 'alert'}
                  >
                    {cartMessage}
                    {cartMessageVariant === 'success' ? <> <Link to="/cart">View cart</Link></> : null}
                  </p>
                ) : null}
              </div>
            </section>
          </div>
        ) : null}
      </div>
    </PageSkeleton>
  );
}
