import PageSkeleton from '../../../components/PageSkeleton';
import { useAuth } from '../../auth/useAuth';
import type { SellerVerificationStatus } from '../../auth/types';
import './SellerDashboardLockedPage.css';

export default function SellerDashboardLockedPage() {
  const { profile } = useAuth();
  const verificationStatus = profile?.sellerVerificationStatus ?? null;
  const copy = getVerificationCopy(verificationStatus);

  return (
    <PageSkeleton
      title="Seller Dashboard"
      titleId="seller-dashboard-locked-title"
      summary="Your seller workspace will unlock after account verification."
    >
      <section className="seller-dashboard-lock" aria-labelledby="seller-dashboard-lock-title">
        <div className="seller-dashboard-lock__header">
          <span className={`seller-dashboard-lock__status seller-dashboard-lock__status--${copy.statusTone}`}>
            {copy.statusLabel}
          </span>
          <h2 id="seller-dashboard-lock-title">{copy.heading}</h2>
          <p>{copy.description}</p>
        </div>

        <div className="seller-dashboard-lock__details" aria-label="Locked seller dashboard areas">
          <div>
            <strong>Product listings</strong>
            <span>Create, edit, publish and remove marketplace listings.</span>
          </div>
          <div>
            <strong>Order management</strong>
            <span>View incoming orders and update fulfillment status.</span>
          </div>
          <div>
            <strong>Seller metrics</strong>
            <span>Track revenue, order volume and active fulfillment work.</span>
          </div>
        </div>

        <p className="seller-dashboard-lock__note">
          This protects buyers and keeps payouts, listings and fulfillment tools limited to approved sellers.
        </p>
      </section>
    </PageSkeleton>
  );
}

function getVerificationCopy(status: SellerVerificationStatus | null) {
  switch (status) {
    case 'Rejected':
      return {
        statusLabel: 'Verification rejected',
        statusTone: 'rejected',
        heading: 'Verification needs attention',
        description:
          'Your seller verification was not approved, so dashboard access is locked until the account is reviewed again.',
      };
    case 'Unverified':
      return {
        statusLabel: 'Verification required',
        statusTone: 'required',
        heading: 'Your seller account is not verified yet',
        description:
          'Marketplace staff must verify your business details before you can manage listings, inventory or orders.',
      };
    case 'Pending':
    default:
      return {
        statusLabel: 'Pending verification',
        statusTone: 'pending',
        heading: 'Your seller account is being reviewed',
        description:
          'The dashboard is ready, but management tools stay locked while marketplace staff review your seller details.',
      };
  }
}
