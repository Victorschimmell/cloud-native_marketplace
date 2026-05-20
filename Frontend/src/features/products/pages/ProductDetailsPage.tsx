import { useEffect, useMemo, useState } from 'react';
import { Link, useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { getCurrencyLocale } from '../../../shared/currency/currency';
import { useCurrency } from '../../../shared/currency/useCurrency';
import { useAuth } from '../../auth/useAuth';
import { cartApi } from '../../cart/api/cartApi';
import { productApi } from '../api/productApi';
import type { ProductDetails, ProductReview } from '../types';
import './ProductDetailsPage.css';

export default function ProductDetailsPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const [product, setProduct] = useState<ProductDetails | null>(null);
  const [reviews, setReviews] = useState<ProductReview[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isAdding, setIsAdding] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reviewsError, setReviewsError] = useState<string | null>(null);
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
        setReviewsError(null);
        setReviews([]);
        const response = await productApi.getProduct(id, listingId, currency, abortController.signal);
        setProduct(response);

        try {
          const reviewResponse = await productApi.getProductReviews(id, abortController.signal);
          setReviews(reviewResponse);
        } catch (reviewRequestError) {
          if (reviewRequestError instanceof DOMException && reviewRequestError.name === 'AbortError') {
            return;
          }

          setReviewsError('Reviews could not be loaded right now.');
        }
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
  const buyerRestrictionMessage = isAuthenticated && !capabilities.isCustomer
    ? 'Only customer accounts can add products to the cart.'
    : null;
  const stockText = isInStock && product ? `${product.stockQuantity} in stock` : 'Out of stock';
  const ratingSummary = useMemo(() => getRatingSummary(reviews), [reviews]);

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
            <div className="product-details-page__media">
              {product.imageUrl ? (
                <img alt={product.productName} src={product.imageUrl} />
              ) : (
                <span>No image</span>
              )}
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
                <span className="product-details-page__rating" aria-label={ratingSummary.ariaLabel}>
                  <RatingStars rating={ratingSummary.average} />
                  <strong>{ratingSummary.averageLabel}</strong>
                  <span className="product-details-page__rating-count">{ratingSummary.countLabel}</span>
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
                {!cartMessage && buyerRestrictionMessage ? (
                  <p className="product-details-page__cart-message product-details-page__cart-message--error" role="status">
                    {buyerRestrictionMessage}
                  </p>
                ) : null}
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

            <section className="product-details-page__reviews" aria-labelledby="product-reviews-title">
              <div className="product-details-page__reviews-heading">
                <div>
                  <h2 id="product-reviews-title">Reviews</h2>
                  <span>{ratingSummary.headerLabel}</span>
                </div>
              </div>

              {reviewsError ? (
                <p className="product-details-page__reviews-notice" role="alert">{reviewsError}</p>
              ) : null}

              {reviews.length > 0 ? (
                <div className="product-details-page__review-list">
                  {reviews.map((review) => (
                    <article className="product-details-page__review" key={review.id}>
                      <div className="product-details-page__review-meta">
                        <strong>{review.reviewerDisplayName || 'Customer'}</strong>
                        <span>{formatReviewDate(review.reviewCreationDateUtc)}</span>
                      </div>
                      <div className="product-details-page__review-score" aria-label={`${review.reviewScore} out of 5 stars`}>
                        <RatingStars rating={review.reviewScore} />
                        <span>{review.reviewScore}/5</span>
                      </div>
                      {review.reviewCommentTitle ? <h3>{review.reviewCommentTitle}</h3> : null}
                      {review.reviewCommentMessage ? <p>{review.reviewCommentMessage}</p> : null}
                    </article>
                  ))}
                </div>
              ) : null}
            </section>
          </div>
        ) : null}
      </div>
    </PageSkeleton>
  );
}

function RatingStars({ rating }: { rating: number }) {
  const roundedToHalf = Math.round(rating * 2) / 2;
  const stars = Array.from({ length: 5 }, (_, index) => {
    const starValue = index + 1;
    if (roundedToHalf >= starValue) {
      return 'product-details-page__star--filled';
    }

    if (roundedToHalf === starValue - 0.5) {
      return 'product-details-page__star--half';
    }

    return '';
  });

  return (
    <span className="product-details-page__stars" aria-hidden="true">
      {stars.map((starClass, index) => (
        <span className={`product-details-page__star ${starClass}`} key={index} />
      ))}
    </span>
  );
}

function getRatingSummary(reviews: ProductReview[]) {
  const count = reviews.length;
  const average = count === 0
    ? 0
    : reviews.reduce((total, review) => total + review.reviewScore, 0) / count;

  return {
    average,
    averageLabel: count === 0 ? 'New' : average.toFixed(1),
    headerLabel: count === 0
      ? 'No reviews yet'
      : `${average.toFixed(1)} out of 5 from ${count} ${count === 1 ? 'review' : 'reviews'}`,
    ariaLabel: count === 0
      ? 'No product reviews yet'
      : `Review summary: ${average.toFixed(1)} out of 5 stars from ${count} ${count === 1 ? 'review' : 'reviews'}`,
    count,
    countLabel: count === 0 ? 'No reviews' : `(${count.toLocaleString()})`,
  };
}

function formatReviewDate(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  }).format(new Date(value));
}
