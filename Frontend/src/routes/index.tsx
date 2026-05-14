import { createBrowserRouter } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';

import HomePage from '../pages/HomePage';
import LoginPage from '../features/auth/pages/LoginPage';
import RegisterPage from '../features/auth/pages/RegisterPage';
import { ProtectedRoute } from '../features/auth/components/ProtectedRoute';
import ProductListPage from '../features/products/pages/ProductListPage';
import ProductDetailsPage from '../features/products/pages/ProductDetailsPage';
import CategoriesPage from '../features/products/pages/CategoriesPage';
import CartPage from '../features/cart/pages/CartPage';
import CheckoutPage from '../features/checkout/pages/CheckoutPage';
import OrdersPage from '../features/orders/pages/OrdersPage';
import OrderDetailsPage from '../features/orders/pages/OrderDetailsPage';
import CustomerPage from '../pages/CustomerPage';
import SellersPage from '../features/seller/pages/SellersPage';
import ReviewsPage from '../pages/ReviewsPage';
import AnalyticsDashboardPage from '../features/admin/pages/AnalyticsDashboardPage';
import SellerProductsPage from '../features/seller/pages/SellerProductsPage';
import SellerOrdersPage from '../features/seller/pages/SellerOrdersPage';
import SellerOrderDetailsPage from '../features/seller/pages/SellerOrderDetailsPage';
import SellerVerificationPage from '../features/seller/pages/SellerVerificationPage';
import NotFoundPage from '../pages/NotFoundPage';
import AdminUsersPage from '../features/admin/pages/AdminUsersPage';
import AdminSellerVerificationsPage from '../features/admin/pages/AdminSellerVerificationsPage';
import AdminAuditPage from '../features/admin/pages/AdminAuditPage';
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
      { path: 'categories', element: <CategoriesPage /> },
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
        element: <ProtectedRoute capability="sellerVerification" />,
        children: [
          { path: 'seller/verification', element: <SellerVerificationPage /> },
        ],
      },
      {
        element: <ProtectedRoute capability="verifiedSeller" />,
        children: [
          { path: 'seller/products', element: <SellerProductsPage /> },
          { path: 'seller/products/new', element: <AddProductPage /> },
          { path: 'seller/orders', element: <SellerOrdersPage /> },
          { path: 'seller/orders/:id', element: <SellerOrderDetailsPage /> },
        ],
      },
      {
        element: <ProtectedRoute capability="admin" />,
        children: [
          { path: 'analytics', element: <AnalyticsDashboardPage /> },
          { path: 'admin/users', element: <AdminUsersPage /> },
          { path: 'admin/verifications', element: <AdminSellerVerificationsPage /> },
          { path: 'admin/audit', element: <AdminAuditPage /> },
        ],
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]);
