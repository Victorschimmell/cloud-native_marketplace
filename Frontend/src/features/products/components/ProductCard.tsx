import { Link } from 'react-router-dom';
import type { BrowseProduct } from '../types';
import './ProductCard.css';

interface ProductCardProps {
  priceFormatter: Intl.NumberFormat;
  product: BrowseProduct;
}

export default function ProductCard({ priceFormatter, product }: ProductCardProps) {
  const stockText = product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Stock pending';

  return (
    <Link
      aria-label={`View ${product.productName}`}
      className="product-card"
      key={product.listingId}
      to={`/products/${product.productId}?listingId=${product.listingId}`}
    >
      <div className="product-card__category">
        {product.categoryName ?? 'Marketplace'}
      </div>
      <h2 className="product-card__title">
        {product.productName}
      </h2>
      <p className="product-card__description">
        {product.description}
      </p>
      <div className="product-card__meta">
        <strong className="product-card__price">{priceFormatter.format(product.price)}</strong>
        <span className={`product-card__stock${product.stockQuantity === 0 ? ' product-card__stock--empty' : ''}`}>
          {stockText}
        </span>
      </div>
    </Link>
  );
}
