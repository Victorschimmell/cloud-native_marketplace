# Data Model Entities and Attributes

## user_accounts
- user_id (PK)
- email (UQ)
- password_hash
- role
- account_status
- failed_login_attempts
- locked_until
- created_at
- updated_at
- last_login_at

## customers
- customer_id (PK)
- user_id (FK, UQ)
- first_name
- last_name
- phone
- default_address_id (FK, nullable)
- olist_customer_id (UQ, nullable)
- olist_customer_unique_id (nullable)
- created_at

## sellers
- seller_id (PK)
- user_id (FK, UQ)
- business_name
- registration_number
- payout_information
- default_address_id (FK, nullable)
- verification_status
- verified_at (nullable)
- olist_seller_id (UQ, nullable)
- created_at

## admins
- admin_id (PK)
- user_id (FK, UQ)
- display_name
- created_at

## user_sessions
- session_id (PK)
- user_id (FK)
- ip_address
- user_agent
- started_at
- expires_at
- ended_at (nullable)
- is_active

## user_blocks
- block_id (PK)
- user_id (FK, nullable)
- ip_address (nullable)
- blocked_by_admin_id (FK)
- reason
- starts_at
- ends_at
- is_active
- created_at

## seller_verification_requests
- verification_request_id (PK)
- seller_id (FK)
- submitted_at
- status
- business_name_snapshot
- registration_number_snapshot
- submitted_details
- review_notes (nullable)
- reviewed_by_admin_id (FK, nullable)
- reviewed_at (nullable)
- rejection_reason (nullable)

## addresses
- address_id (PK)
- postal_code
- city
- state
- address_line_1 (nullable)
- address_line_2 (nullable)
- country_code
- latitude (nullable)
- longitude (nullable)
- created_at

## geolocations
- geolocation_zip_code_prefix
- geolocation_lat
- geolocation_lng
- geolocation_city
- geolocation_state

## product_categories
- category_id (PK)
- category_name_pt (UQ)
- category_name_en (nullable)

## products
- product_id (PK)
- category_id (FK)
- product_name
- description
- product_name_length
- product_description_length
- product_photos_qty
- product_weight_g
- product_length_cm
- product_height_cm
- product_width_cm
- created_at
- updated_at
- olist_product_id (UQ, nullable)

## product_listings
- listing_id (PK)
- seller_id (FK)
- product_id (FK)
- sku
- listing_price
- inventory_quantity
- visibility_status
- is_deleted
- published_at (nullable)
- created_at
- updated_at

## shopping_carts
- cart_id (PK)
- user_id (FK, nullable)
- session_id (FK, nullable)
- status
- created_at
- updated_at
- expires_at
- recovered_from_cart_id (FK, nullable)

## cart_items
- cart_item_id (PK)
- cart_id (FK)
- listing_id (FK)
- quantity
- unit_price_at_addition
- added_at
- updated_at

## orders
- order_id (PK)
- customer_id (FK)
- shipping_address_id (FK)
- order_status
- order_purchase_timestamp
- order_approved_at (nullable)
- order_delivered_carrier_date (nullable)
- order_delivered_customer_date (nullable)
- order_estimated_delivery_date (nullable)
- subtotal_amount
- freight_amount
- total_amount
- placed_from_cart_id (FK, nullable)
- order_number (UQ)

## order_items
- order_id (PK, FK)
- order_item_id (PK)
- listing_id (FK)
- product_id (FK)
- seller_id (FK)
- quantity
- unit_price
- freight_value
- shipping_limit_date (nullable)

## order_payments
- order_id (PK, FK)
- payment_sequential (PK)
- payment_type
- payment_installments
- payment_value
- payment_status
- external_payment_reference (nullable)
- paid_at (nullable)

## order_reviews
- review_id (PK)
- order_id (FK)
- review_score
- review_comment_title (nullable)
- review_comment_message (nullable)
- review_creation_date
- review_answer_timestamp (nullable)

## shipments
- shipment_id (PK)
- order_id (FK)
- seller_id (FK)
- carrier_name
- tracking_number
- shipment_status
- shipped_at (nullable)
- delivered_at (nullable)
- returned_at (nullable)
- created_at
- updated_at

## notifications
- notification_id (PK)
- user_id (FK)
- order_id (FK, nullable)
- notification_type
- title
- body
- created_at
- read_at (nullable)

## system_incidents
- incident_id (PK)
- incident_type
- severity
- status
- component_name
- message
- created_at
- resolved_at (nullable)

## audit_logs
- audit_log_id (PK)
- actor_user_id (FK, nullable)
- actor_ip_address (nullable)
- action_type
- target_entity_type
- target_entity_id
- outcome
- details
- created_at

## order_cancellations (future)
- cancellation_id (PK)
- order_id (FK)
- cancelled_by_seller_id (FK)
- cancellation_reason
- comment_to_customer
- cancelled_at
- refund_status

## refunds (future)
- refund_id (PK)
- cancellation_id (FK)
- order_id (FK)
- amount
- refund_status
- external_refund_reference (nullable)
- processed_at (nullable)