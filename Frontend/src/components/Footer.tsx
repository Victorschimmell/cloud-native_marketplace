export default function Footer() {
  return (
    <footer style={{
      backgroundColor: '#f3f4f6',
      padding: '24px 0',
      textAlign: 'center',
      fontSize: '14px',
      color: '#6b7280',
      borderTop: '1px solid #e5e7eb'
    }}>
      © 2026 Marketplace Platform • All rights reserved • {' '}
      <a 
        href="/NOTICE.txt" 
        target="_blank" 
        rel="noopener noreferrer"
        style={{ color: '#4b5563', textDecoration: 'underline' }}
      >
        Third-Party Licenses
      </a>
      <br />
      <span style={{ fontSize: '12px' }}>Skeleton built for feature-by-feature development</span>
    </footer>
  );
}