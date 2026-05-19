import './OrderSummarySkeleton.css';

export default function OrderSummarySkeleton() {
  return (
    <article className="orders-page__card orders-page__card--skeleton" aria-hidden="true">
      <header className="orders-page__card-header">
        <div className="orders-page__card-main">
          <div className="orders-page__skeleton-block orders-page__skeleton-block--eyebrow" />
          <div className="orders-page__skeleton-block orders-page__skeleton-block--date" />
        </div>
        <div className="orders-page__card-status">
          <div className="orders-page__skeleton-pill" />
        </div>
      </header>

      <div className="orders-page__skeleton-items">
        <div className="orders-page__skeleton-line">
          <div className="orders-page__skeleton-block orders-page__skeleton-block--item" />
          <div className="orders-page__skeleton-block orders-page__skeleton-block--price" />
        </div>
        <div className="orders-page__skeleton-line">
          <div className="orders-page__skeleton-block orders-page__skeleton-block--item" />
          <div className="orders-page__skeleton-block orders-page__skeleton-block--price" />
        </div>
      </div>

      <div className="orders-page__skeleton-block orders-page__skeleton-block--shipment" />

      <footer className="orders-page__footer">
        <div className="orders-page__skeleton-block orders-page__skeleton-block--total" />
        <div className="orders-page__actions">
          <div className="orders-page__skeleton-button" />
          <div className="orders-page__skeleton-button orders-page__skeleton-button--ghost" />
        </div>
      </footer>
    </article>
  );
}
