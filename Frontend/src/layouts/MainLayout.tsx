import { Outlet } from 'react-router-dom';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';

export default function MainLayout() {
  return (
    <div style={{ 
      display: 'flex', 
      flexDirection: 'column', 
      minHeight: '100vh', 
      width: '100%',
      backgroundColor: '#f9fafb'
    }}>
      <Navbar />
      <main style={{ 
        flex: 1, 
        maxWidth: '1280px',     
        margin: '0 auto',
        width: '100%',
        padding: '40px 40px'
      }}>
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}