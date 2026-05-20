import { Link } from 'react-router-dom';
import PageSkeleton from '../components/PageSkeleton';
import './HomePage.css';

export default function HomePage() {
  const featureCards = [
    {
      description: 'Browse products, add items to cart, and move through checkout and order tracking.',
      title: 'Shopping',
    },
    {
      description: 'Verified sellers can maintain listings and follow incoming orders from one focused area.',
      title: 'Selling',
    },
    {
      description: 'Admins can review users, seller verification, analytics, payments, and audit activity.',
      title: 'Operations',
    },
  ];

  return (
    <PageSkeleton
      summary="Browse products, place orders, manage sellers, and keep platform operations visible."
      title="Marketplace Platform"
      titleId="home-page-title"
    >
      <div className="home-page">
        <section className="home-page__hero" aria-label="Marketplace overview">
          <div className="home-page__hero-copy">
            <h2>Clean commerce flows for customers, sellers and admins.</h2>
            <p>
              A focused marketplace experience for customers, sellers and admins.
            </p>
            <Link className="home-page__button" to="/products">
              Browse Products
            </Link>
          </div>

          <div className="home-page__visual" aria-hidden="true">
            <div className="home-page__market-card">
              <span className="home-page__market-bar" />
              <div className="home-page__market-grid">
                <span />
                <span />
                <span />
                <span />
              </div>
              <div className="home-page__market-order">
                <span />
                <span />
              </div>
            </div>
          </div>
        </section>

        <section className="home-page__section" aria-labelledby="home-features-title">
          <div className="home-page__section-heading">
            <h2 id="home-features-title">Features</h2>
          </div>

          <div className="home-page__feature-grid">
            {featureCards.map((feature) => (
              <article className="home-page__feature-card" key={feature.title}>
                <h3>{feature.title}</h3>
                <p>{feature.description}</p>
              </article>
            ))}
          </div>
        </section>
      </div>
    </PageSkeleton>
  );
}
