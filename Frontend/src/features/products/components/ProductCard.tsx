import { Link } from 'react-router-dom';
import type { BrowseProduct } from '../types';
import './ProductCard.css';

interface ProductCardProps {
  priceFormatter: Intl.NumberFormat;
  product: BrowseProduct;
}

export default function ProductCard({ priceFormatter, product }: ProductCardProps) {
  const stockText = product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Out of stock';
  const stockModifier = product.stockQuantity === 0 ? ' product-card__stock--empty' : '';
  const reviewLabel = product.reviewCount === 1 ? '1 review' : `${product.reviewCount.toLocaleString()} reviews`;
  const ratingLabel = product.averageReviewScore === null
    ? 'No reviews yet'
    : `${product.averageReviewScore.toFixed(1)} out of 5 stars from ${reviewLabel}`;

  return (
    <Link
      aria-label={`View ${product.productName}`}
      className="product-card"
      key={product.listingId}
      to={`/products/${product.productId}?listingId=${product.listingId}`}
    >
      <span className="product-card__media" aria-hidden="true">
        {product.imageUrl ? (
          <img alt="" src={product.imageUrl} />
        ) : (
          'No image'
        )}
      </span>

      <div className="product-card__category">
        {product.categoryName ?? 'Marketplace'}
      </div>

      <h2 className="product-card__title">
        {product.productName}
      </h2>

      <div className="product-card__rating" aria-label={ratingLabel}>
        <RatingStars rating={product.averageReviewScore ?? 0} />
        {product.averageReviewScore === null ? (
          <span className="product-card__rating-empty">No reviews yet</span>
        ) : (
          <>
            <strong>{product.averageReviewScore.toFixed(1)}</strong>
            <span>{reviewLabel}</span>
          </>
        )}
      </div>

      <p className="product-card__description">
        {product.description}
      </p>

      <div className="product-card__meta">
        <strong className="product-card__price">{priceFormatter.format(product.price)}</strong>
        <span className={`product-card__stock${stockModifier}`}>
          {stockText}
        </span>
      </div>
    </Link>
  );
}

function RatingStars({ rating }: { rating: number }) {
  const roundedToHalf = Math.round(rating * 2) / 2;
  const stars = Array.from({ length: 5 }, (_, index) => {
    const starValue = index + 1;
    if (roundedToHalf >= starValue) {
      return 'product-card__star--filled';
    }

    if (roundedToHalf === starValue - 0.5) {
      return 'product-card__star--half';
    }

    return '';
  });

  return (
    <span className="product-card__stars" aria-hidden="true">
      {stars.map((starClass, index) => (
        <span className={`product-card__star ${starClass}`} key={index} />
      ))}
    </span>
  );
}
