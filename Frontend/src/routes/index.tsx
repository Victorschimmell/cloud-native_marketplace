import { createBrowserRouter, Navigate } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';

import HomePage from '../pages/HomePage';
import LoginPage from '../features/auth/pages/LoginPage';
import RegisterPage from '../features/auth/pages/RegisterPage';
import { ProtectedRoute } from '../features/auth/components/ProtectedRoute';
import ProductListPage from '../features/products/pages/ProductListPage';
import ProductDetailsPage from '../features/products/pages/ProductDetailsPage';
import CartPage from '../features/cart/pages/CartPage';
import CheckoutPage from '../features/checkout/pages/CheckoutPage';
import OrdersPage from '../features/orders/pages/OrdersPage';
import OrderDetailsPage from '../features/orders/pages/OrderDetailsPage';
import CustomerPage from '../pages/CustomerPage';
import SellersPage from '../features/seller/pages/SellersPage';
import ReviewsPage from '../pages/ReviewsPage';
import AnalyticsDashboardPage from '../features/admin/pages/AnalyticsDashboardPage';
import SellerDashboard from '../features/seller/components/SellerDashboard';
import SellerVerificationGuard from '../features/seller/components/SellerVerificationGuard';
import SellerOrderDetailsPage from '../features/seller/pages/SellerOrderDetailsPage';
import NotFoundPage from '../pages/NotFoundPage';
import AdminUsersPage from '../features/admin/pages/AdminUsersPage';
import AdminSellerVerificationsPage from '../features/admin/pages/AdminSellerVerificationsPage';
import AdminAuditPage from '../features/admin/pages/AdminAuditPage';
import AdminReportIssuePage from '../features/admin/pages/AdminReportIssuePage';
import AdminCreateIssuePage from '../features/admin/pages/AdminCreateIssuePage';
import AdminPaymentsPage from '../features/admin/pages/AdminPaymentsPage';
import AddProductPage from '../features/seller/pages/AddProductPage';

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
      { path: 'not-found', element: <NotFoundPage /> },
      {
        element: <ProtectedRoute capability="customer" />,
        children: [
          { path: 'cart', element: <CartPage /> },
          { path: 'checkout', element: <CheckoutPage /> },
          { path: 'orders', element: <OrdersPage /> },
          { path: 'orders/:id', element: <OrderDetailsPage /> },
        ],
      },
      {
        element: <ProtectedRoute />,
        children: [
          { path: 'customers/:id', element: <CustomerPage /> },
        ],
      },
      { path: 'sellers/:id', element: <SellersPage /> },
      { path: 'reviews', element: <ReviewsPage /> },
      {
        element: <ProtectedRoute capability="seller" />,
        children: [
          { path: 'seller/verification', element: <Navigate replace to="/seller/products" /> },
          {
            element: <SellerVerificationGuard />,
            children: [
              { path: 'seller/products', element: <SellerDashboard /> },
              { path: 'seller/products/new', element: <AddProductPage /> },
              { path: 'seller/orders', element: <SellerDashboard /> },
              { path: 'seller/orders/:id', element: <SellerOrderDetailsPage /> },
            ],
          },
        ],
      },
      {
        element: <ProtectedRoute capability="admin" />,
        children: [
          { path: 'admin/dashboard', element: <AnalyticsDashboardPage /> },
          { path: 'admin/users', element: <AdminUsersPage /> },
          { path: 'admin/issues', element: <AdminReportIssuePage /> },
          { path: 'admin/issues/new', element: <AdminCreateIssuePage /> },
          { path: 'admin/payments', element: <AdminPaymentsPage /> },
          { path: 'admin/verifications', element: <AdminSellerVerificationsPage /> },
          { path: 'admin/audit', element: <AdminAuditPage /> },
        ],
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]);
