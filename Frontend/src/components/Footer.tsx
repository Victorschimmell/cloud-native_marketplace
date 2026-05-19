import './Footer.css';

export default function Footer() {
  return (
    <footer className="footer">
      <div className="footer__inner">
        <div className="footer__legal">
          <span>&copy; 2026 Marketplace Platform</span>
          <span className="footer__separator" aria-hidden="true">|</span>
          <span>All rights reserved</span>
          <span className="footer__separator" aria-hidden="true">|</span>
          <a
            href="/NOTICE.txt"
            target="_blank"
            rel="noopener noreferrer"
          >
            Third-Party Licenses
          </a>
        </div>
        <p className="footer__description">
          Marketplace tools for browsing, selling and order management
        </p>
      </div>
    </footer>
  );
}
