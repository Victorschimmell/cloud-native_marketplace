import { createBrowserRouter } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';

import HomePage from '../pages/HomePage';
import LoginPage from '../features/auth/pages/LoginPage';
import RegisterPage from '../features/auth/pages/RegisterPage';
import { ProtectedRoute } from '../features/auth/components/ProtectedRoute';
import ProductListPage from '../features/products/pages/ProductListPage';
import ProductDetailsPage from '../features/products/pages/ProductDetailsPage';
import CategoriesPage from '../pages/CategoriesPage';
import CartPage from '../features/cart/pages/CartPage';
import CheckoutPage from '../features/checkout/pages/CheckoutPage';
import OrdersPage from '../features/orders/pages/OrdersPage';
import OrderDetailsPage from '../features/orders/pages/OrderDetailsPage';
import CustomerPage from '../pages/CustomerPage';
import SellersPage from '../pages/SellersPage';
import ReviewsPage from '../pages/ReviewsPage';
import AnalyticsDashboardPage from '../pages/AnalyticsDashboardPage';
import SellerProductsPage from '../pages/SellerProductsPage';
import SellerOrdersPage from '../pages/SellerOrdersPage';
import SellerOrderDetailsPage from '../pages/SellerOrderDetailsPage';
import SellerVerificationPage from '../pages/SellerVerificationPage';
import NotFoundPage from '../pages/NotFoundPage';
import AdminUsersPage from '../pages/AdminUsersPage';
import AdminSellerVerificationsPage from '../pages/AdminSellerVerificationsPage';
import AdminAuditPage from '../pages/AdminAuditPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      { path: '', element: <HomePage /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'register', element: <RegisterPage /> },
      { path: 'products', element: <ProductListPage /> },
      { path: 'products/:id', element: <ProductDetailsPage /> },
      { path: 'categories', element: <CategoriesPage /> },
      {
        element: <ProtectedRoute />,
        children: [
          { path: 'cart', element: <CartPage /> },
          { path: 'checkout', element: <CheckoutPage /> },
          { path: 'orders', element: <OrdersPage /> },
          { path: 'orders/:id', element: <OrderDetailsPage /> },
        ],
      },
      { path: 'customers/:id', element: <CustomerPage /> },
      { path: 'sellers/:id', element: <SellersPage /> },
      { path: 'reviews', element: <ReviewsPage /> },
      { path: 'analytics', element: <AnalyticsDashboardPage /> },
      { path: 'seller/products', element: <SellerProductsPage /> },
      { path: 'seller/orders', element: <SellerOrdersPage /> },
      { path: 'seller/orders/:id', element: <SellerOrderDetailsPage /> },
      { path: 'seller/verification', element: <SellerVerificationPage /> },
      { path: 'admin/users', element: <AdminUsersPage /> },
      { path: 'admin/verifications', element: <AdminSellerVerificationsPage /> },
      { path: 'admin/audit', element: <AdminAuditPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]);
