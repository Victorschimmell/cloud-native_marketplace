import './OrderDetailsSkeleton.css';

export default function OrderDetailsSkeleton() {
  return (
    <div className="order-details__skeleton" aria-hidden="true">
      <section className="order-details__section order-details__section--skeleton">
        <div className="order-details__skeleton-header">
          <div>
            <div className="order-details__skeleton-block order-details__skeleton-block--title" />
            <div className="order-details__skeleton-block order-details__skeleton-block--meta" />
          </div>
          <div className="order-details__skeleton-pill" />
        </div>
        <div className="order-details__skeleton-block order-details__skeleton-block--note" />
      </section>

      <section className="order-details__section order-details__section--skeleton">
        <div className="order-details__skeleton-heading">
          <div className="order-details__skeleton-block order-details__skeleton-block--heading" />
          <div className="order-details__skeleton-block order-details__skeleton-block--count" />
        </div>
        <div className="order-details__skeleton-lines">
          <div className="order-details__skeleton-line">
            <div className="order-details__skeleton-block order-details__skeleton-block--line" />
            <div className="order-details__skeleton-block order-details__skeleton-block--price" />
          </div>
          <div className="order-details__skeleton-line">
            <div className="order-details__skeleton-block order-details__skeleton-block--line" />
            <div className="order-details__skeleton-block order-details__skeleton-block--price" />
          </div>
        </div>
        <div className="order-details__skeleton-totals">
          <div className="order-details__skeleton-block order-details__skeleton-block--total" />
          <div className="order-details__skeleton-block order-details__skeleton-block--total" />
          <div className="order-details__skeleton-block order-details__skeleton-block--total" />
        </div>
      </section>

      <section className="order-details__section order-details__section--skeleton">
        <div className="order-details__skeleton-heading">
          <div className="order-details__skeleton-block order-details__skeleton-block--heading" />
        </div>
        <div className="order-details__skeleton-status">
          <div className="order-details__skeleton-block order-details__skeleton-block--status" />
          <div className="order-details__skeleton-block order-details__skeleton-block--status" />
          <div className="order-details__skeleton-block order-details__skeleton-block--status" />
        </div>
      </section>
    </div>
  );
}
