# Marketplace Platform
Marketplace Platform is a monorepo with a React/Vite frontend, a .NET 10 backend API, PostgreSQL and Elasticsearch/Kibana.

## Prerequisites
- Docker Desktop
- .NET SDK 10, only if running the backend from source
- Node.js and npm, only if running the frontend from source

## Option 1: Run the Project With Docker and Olist Seeding
This is the customer setup path. It starts the frontend, backend, PostgreSQL, Elasticsearch and Kibana containers, applies database migrations, and imports the Olist dataset on backend startup.

1. Download the Olist dataset from https://www.kaggle.com/datasets/olistbr/brazilian-ecommerce.

2. Create the dataset folder from the repository root:
```powershell
New-Item -ItemType Directory -Force .\Backend\Backend.Api\.data\olist
```

3. Copy these CSV files into `Backend\Backend.Api\.data\olist`:
- `product_category_name_translation.csv`
- `olist_customers_dataset.csv`
- `olist_sellers_dataset.csv`
- `olist_products_dataset.csv`
- `olist_orders_dataset.csv`
- `olist_order_items_dataset.csv`
- `olist_order_payments_dataset.csv`
- `olist_order_reviews_dataset.csv`

4. Start the full Docker system with the Olist seed override:
```powershell
docker compose -f docker-compose.yml -f docker-compose.olist-seed.yml up --build
```

5. In a second PowerShell terminal, create or update the Kibana dashboards:
```powershell
.\docs\observability\create-kibana-dashboards.ps1
```

Open:
- Frontend: http://localhost
- Backend API base URL: http://localhost:8080
- Kibana: http://localhost:5601
- Kibana System Overview dashboard: http://localhost:5601/app/dashboards#/view/dashboard-marketplace-system-overview
- Kibana Checkout Observability dashboard: http://localhost:5601/app/dashboards#/view/dashboard-marketplace-checkout-observability

The backend does not have a page at `/`, so `http://localhost:8080` can show a 404 in the browser. Use the frontend URL for the application.

The dashboard script is idempotent. Run it again whenever Kibana data is reset or the dashboards need to be recreated.

Verify that Olist seeding ran:
```powershell
docker compose logs backend
```

Look for Olist import log messages such as inserted customers, sellers, products, orders, payments and reviews.

Stop the project:
```powershell
docker compose down
```

Reset local Docker data and reseed from a fresh database:
```powershell
docker compose down -v
docker compose -f docker-compose.yml -f docker-compose.olist-seed.yml up --build
```

`docker compose down -v` deletes the local PostgreSQL and Elasticsearch Docker volumes for this project.

If the containers are already running and only the backend needs to be rebuilt/restarted for seeding:
```powershell
docker compose -f docker-compose.yml -f docker-compose.olist-seed.yml up -d --build --force-recreate backend
docker compose logs backend
```

## Option 2: Run the Project Locally
Use this if you want to run the backend and frontend from source.

Start the backing services:
```powershell
docker compose up -d postgres elasticsearch kibana
```

Start the backend API:
```powershell
dotnet run --project .\Backend\Backend.Api\Backend.Api.csproj --launch-profile http
```

The backend runs at http://localhost:5094. Migrations are applied automatically in development.

Start the frontend in a second terminal:
```powershell
cd .\Frontend
npm install
npm run dev
```

The frontend runs at http://localhost:5173 and proxies API calls to http://localhost:5094.

## Olist Import When Running Locally

The project can import Olist CSV data on backend startup.
1. Put the Olist CSV files from https://www.kaggle.com/datasets/olistbr/brazilian-ecommerce in `<repo-root>\.data\olist`.
2. Set `OlistImport:Enabled` to `true` in `Backend\Backend.Api\appsettings.Development.json`.
3. Start the backend API.

The configured default path is `.data\olist`, which is resolved relative to the backend process working directory. If you start the backend from Visual Studio, that may resolve to `<repo-root>\Backend\Backend.Api\.data\olist`. To avoid ambiguity, set `OlistImport:DatasetRootPath` to the absolute path of `<repo-root>\.data\olist` on your machine.

## Relevant Flows

### 1. Customer Checkout
1. Register a customer account.
2. Log in as the customer.
3. Browse products and add one or more products to the cart.
4. Open the cart from the top-right corner.
5. Click "Proceed to checkout".
6. Fill in the required shipping and payment information.
7. Click "Place order".
8. Open the created order and verify that the order details are correct.

### 2. Seller Product Management
1. Register a seller account and remember the credentials.
2. Log in as Admin (email: admin@example.com, password: admin).
3. Go to "User Management" and approve the seller.
4. Log in as the approved seller.
5. Go to "Seller Dashboard".
6. Add a product by filling in the required product information.
7. Publish the product from the product inventory table.
8. Go to "Browse Products" and verify that the product is visible.
9. Return to "Seller Dashboard" and delete the product.
10. Go back to "Browse Products" and verify that the product is no longer visible.

### 3. Seller Order Management
1. Complete the seller product flow and publish a product.
2. Log in as a customer.
3. Add the seller's product to the cart and place an order.
4. Log in as the seller.
5. Go to "Seller Dashboard" and open "Order Management".
6. Open the new order and verify the order details.
7. Use the actions in the order panel to update item and shipment status.
8. Log in as the customer again and verify that the order status has changed.

### 4. Admin Dashboard
1. Log in as Admin (email: admin@example.com, password: admin).
2. Go to "Admin Dashboard".
3. Verify the overview metrics for active users, orders in the last 24 hours, total revenue and unresolved issues.
4. Click "View All" on "Recent Payments".
5. Verify that recent payments are shown in a list.
6. Return to "Admin Dashboard" and click "View All" on "Unresolved Issues".
7. Create a new issue.
8. Verify that the issue appears in the issues table.
9. Assign the issue to yourself and resolve it.

### 5. Admin User Management and Audit Logs
1. Log in as Admin (email: admin@example.com, password: admin).
2. Go to "User Management".
3. Verify that the users table is shown.
4. Block a user and verify that the user status changes to blocked.
5. Unblock the same user and verify that the user becomes active again.
6. Go to "Audit Logs".
7. Verify that the latest admin actions are shown with the correct timestamps.

### 6. Kibana Checkout Observability
1. Start the Docker stack and open Kibana at http://localhost:5601.
2. Create or update the dashboards by running `.\docs\observability\create-kibana-dashboards.ps1`.
3. Run a few checkout attempts in the frontend, or run the k6 load test from `docs\observability\load-tests`.
4. Open the "Marketplace Checkout Observability" dashboard in Kibana.
5. Verify that checkout attempts, failures and operation durations are visible.
6. Copy a correlation ID from a checkout event and filter by it to inspect one complete checkout flow.
