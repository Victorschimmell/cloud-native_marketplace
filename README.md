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
Text

### 1. Customer add items to cart and checkout
1. Register a customer
2. Log-in as customer
3. Browse products and add some products to your cart
4. In the top-right corner, click on the cart
5. Click "Proceed to checkout"
6. Fill in required information
7. Click "Place order"
8. Order has been placed and you can now verify order content

### 2. Register Seller and Perform CRUD-operation on Products
1. Register a seller (remember the credentials)
2. Seller needs to be verified. Log in as Admin (email: admin@example.com, password: admin) to do this.
3. Go to "User Management" and approve the registered seller
4. Log in as registered seller
5. Go to "Seller Dashboard"
6. Click add product and fill in required information and create product
7. In the product inventory table on the seller dashboard, click "Publish" on the newly created product.
8. Go to "Browse Products" and verify that your newly created product is there
9. Go back to "Seller Dashboard" in the product inventory table and delete the product
10. Verify that the product does not appear on the "Browse Products" page

### 3. Seller Order Management
1. Do flow 3.
2. Log in as a customer and select the newly created product from flow (3)
3. Add this product to your cart and go through the whole checkout-flow to place an order on that item.
4. Now, log in as the seller of this product
5. Go to "Seller Dashboard" and click "Order Management"
6. Click on the created order from step 4 and verify order content
7. On the actions in the right side panel, click through these actions to change item-status (and order status if item is the only item on that order)
8. Log in as the same customer from step 2 to verify the order changes

### 4. Admin Dashboard
1. Log in as Admin (email: admin@example.com, password: admin).
2. Go to "Admin Dashboard"
3. Verify that there is metrics in "Active Users", "Orders Last 24h", "Total Revenue" and "Unresolved Issues".
4. Click "View All" on "Recent Payments".
5. Verify that you can see all the recent payments in a list.
6. Go back to Admin Dashboard and click "View All" on "Unresolved Issues".
7. Click "Create New Issue" and fill in the content to create a new issue
8. Verify that the issue is represented in the "All Issues" table
9. Assign yourself to the issue and resolve the issue. Verify that the state of the issue changes.

### 5. Admin User Management and Audit Logs
1. Log in as Admin (email: admin@example.com, password: admin).
2. Go to "User Management" in the navbar.
3. Verify that you can see a list of users
4. Choose a user and click "Block" in the actions of that user and verify status of that user is Blocked.
5. Now, unblock the same user and verify, that status of that user is no active.
6. Go to "Audit Logs" in the navbar.
7. Verify that you can see the latest actions in the table with correct time.
