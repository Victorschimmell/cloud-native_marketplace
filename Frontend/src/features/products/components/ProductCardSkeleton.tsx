import './ProductCardSkeleton.css';

export default function ProductCardSkeleton() {
  return (
    <article className="product-card-skeleton">
      <div className="product-card-skeleton__block product-card-skeleton__block--media" />
      <div className="product-card-skeleton__block product-card-skeleton__block--category" />
      <div className="product-card-skeleton__block product-card-skeleton__block--title" />
      <div className="product-card-skeleton__block product-card-skeleton__block--rating" />
      <div className="product-card-skeleton__block product-card-skeleton__block--text" />
      <div className="product-card-skeleton__block product-card-skeleton__block--text product-card-skeleton__block--text-short" />
      <div className="product-card-skeleton__meta">
        <div className="product-card-skeleton__block product-card-skeleton__block--price" />
        <div className="product-card-skeleton__block product-card-skeleton__block--stock" />
      </div>
    </article>
  );
}
