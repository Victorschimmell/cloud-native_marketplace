import { Link } from 'react-router-dom';
import PageSkeleton from '../components/PageSkeleton';
import './HomePage.css';

export default function HomePage() {
  const featureCards = [
    {
      accent: 'customer',
      description: 'Visitors and customers can move through the basic buying journey from browsing to order follow-up.',
      items: ['Product catalog', 'Cart & checkout', 'Order tracking'],
      label: 'Customer',
      primaryLink: '/products',
      primaryText: 'Browse',
      title: 'Buying experience',
    },
    {
      accent: 'seller',
      description: 'Sellers have a dedicated area for maintaining their products and handling incoming orders.',
      items: ['Product listings', 'Order overview', 'Shipment status'],
      label: 'Seller',
      primaryLink: '/seller/products',
      primaryText: 'Seller tools',
      title: 'Seller management',
    },
    {
      accent: 'admin',
      description: 'Administrators can review platform activity and manage users, verification and audit information.',
      items: ['Seller verification', 'User access', 'Audit & analytics'],
      label: 'Admin',
      primaryLink: '/admin/users',
      primaryText: 'Admin panel',
      title: 'Platform administration',
    },
  ];

  const includedFeatures = [
    'Role-based customer, seller and admin pages',
    'Product, cart, checkout and order workflows',
    'Seller verification, analytics and audit log support',
  ];

  const flowSteps = ['Catalog', 'Cart', 'Checkout', 'Orders', 'Audit'];

  return (
    <PageSkeleton
      summary="A short overview of the features implemented in the B2C marketplace platform."
      title="Cloud-Native Marketplace Platform"
      titleId="home-page-title"
    >
      <div className="home-page">
        <section className="home-page__hero-panel" aria-label="Marketplace summary">
          <div className="home-page__hero-copy">
            <h2>Project Scope</h2>
            <p>
              This application demonstrates a marketplace with three main roles. Customers can
              browse and place orders, sellers can manage products and order status, and admins
              can oversee users, seller verification, analytics and audit logs.
            </p>

            <div className="home-page__actions" aria-label="Primary actions">
              <Link className="home-page__button home-page__button--primary" to="/products">
                Browse Products
              </Link>
              <Link className="home-page__button home-page__button--secondary" to="/register">
                Create Account
              </Link>
            </div>
          </div>

          <aside className="home-page__included" aria-label="Included in this project">
            <h3>Included in project</h3>
            <ul>
              {includedFeatures.map((feature) => (
                <li key={feature}>{feature}</li>
              ))}
            </ul>
          </aside>
        </section>

        <section className="home-page__flow-strip" aria-label="End-to-end commerce flow">
          <strong>End-to-end flow</strong>
          <div className="home-page__flow-steps">
            {flowSteps.map((step, index) => (
              <div className="home-page__flow-step" key={step}>
                <span>{index + 1}</span>
                {step}
              </div>
            ))}
          </div>
        </section>

        <section className="home-page__section" aria-labelledby="home-features-title">
          <div className="home-page__section-heading">
            <h2 id="home-features-title">Implemented Areas</h2>
          </div>

          <div className="home-page__feature-grid">
            {featureCards.map((feature) => (
              <article className="home-page__feature-card" data-accent={feature.accent} key={feature.label}>
                <h3>{feature.title}</h3>
                <span className="home-page__feature-description">{feature.description}</span>
                <ul>
                  {feature.items.map((item) => (
                    <li key={item}>{item}</li>
                  ))}
                </ul>
                <Link to={feature.primaryLink}>{feature.primaryText}</Link>
              </article>
            ))}
          </div>
        </section>
      </div>
    </PageSkeleton>
  );
}
