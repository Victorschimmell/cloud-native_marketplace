# Data Model Entities and Attributes

## user_account
- user_id (PK)
- email (UQ)
- password_hash
- is_admin
- is_blocked
- account_status
- failed_login_attempts
- locked_until (nullable)
- created_at
- updated_at
- last_login_at (nullable)

## customer
- customer_id (PK)
- user_id (FK, UQ)
- first_name
- last_name
- phone
- default_address_id (FK, nullable)
- olist_customer_id (nullable)
- olist_customer_unique_id (nullable)
- created_at
- updated_at

## seller
- seller_id (PK)
- user_id (FK, UQ)
- business_name
- registration_number
- payout_information
- default_address_id (FK, nullable)
- verification_status
- verified_at (nullable)
- olist_seller_id (nullable)
- created_at
- updated_at

## user_session
- session_id (PK)
- user_id (FK)
- ip_address
- user_agent
- started_at
- expires_at
- ended_at (nullable)
- is_active

## seller_verification_request
- verification_request_id (PK)
- seller_id (FK)
- submitted_at
- status
- business_name_snapshot
- registration_number_snapshot
- submitted_details
- review_notes (nullable)
- reviewed_by_user_id (FK, nullable)
- reviewed_at (nullable)
- rejection_reason (nullable)

## address
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
- updated_at

## product_category
- category_id (PK)
- category_name_pt
- category_name_en (nullable)

## product
- product_id (PK)
- category_id (FK)
- product_name
- description
- image_url (nullable)
- product_name_length
- product_description_length
- product_photos_qty
- product_weight_g
- product_length_cm
- product_height_cm
- product_width_cm
- olist_product_id (nullable, indexed)
- created_at
- updated_at

## product_listing
- listing_id (PK)
- seller_id (FK)
- product_id (FK)
- sku (UQ)
- listing_price
- inventory_quantity
- visibility_status
- is_deleted
- published_at (nullable)
- created_at
- updated_at

## shopping_cart
- cart_id (PK)
- user_id (FK, nullable)
- session_id (FK, nullable)
- status
- expires_at
- recovered_from_cart_id (FK, nullable, self-reference)
- created_at
- updated_at

## cart_item
- cart_item_id (PK)
- cart_id (FK)
- listing_id (FK)
- quantity
- unit_price_at_addition
- added_at
- updated_at

## currency
- currency_id (PK)
- code (UQ)
- name
- symbol (nullable)

## order
- order_id (PK)
- customer_id (FK)
- shipping_address_id (FK)
- order_status
- order_status_description (nullable)
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
- created_at
- updated_at

## order_item
- order_id (PK, FK)
- order_item_id (PK)
- listing_id (FK)
- product_id (FK)
- seller_id (FK)
- quantity
- unit_price
- freight_value
- shipping_limit_date (nullable)
- fulfillment_status
- fulfillment_approved_at (nullable)
- fulfillment_processing_at (nullable)
- fulfillment_shipped_at (nullable)

## order_payment
- order_id (PK, FK)
- payment_sequential (PK)
- currency_id (FK)
- payment_type
- payment_installments
- payment_value
- payment_status
- external_payment_reference (nullable)
- paid_at (nullable)

## order_review
- review_id (PK)
- order_id (FK)
- order_item_id (FK, nullable)
- customer_id (FK)
- product_id (FK)
- olist_review_id (UQ, nullable)
- review_score
- review_comment_title (nullable)
- review_comment_message (nullable)
- review_creation_date
- review_answer_timestamp (nullable)

## shipment
- shipment_id (PK)
- order_id (FK)
- seller_id (FK)
- carrier_name
- tracking_number (indexed)
- shipment_status
- shipped_at (nullable)
- delivered_at (nullable)
- returned_at (nullable)
- created_at
- updated_at

## number_sequence
- sequence_id (PK)
- sequence_key (UQ)
- last_value

## admin_issue
- issue_id (PK)
- title
- description
- type
- priority (indexed)
- status (indexed)
- reported_by_user_id (FK, indexed)
- assigned_to_user_id (FK, nullable)
- resolved_by_user_id (FK, nullable)
- resolved_at (nullable)
- resolution (nullable)
- created_at
- updated_at

## audit_log
- audit_log_id (PK)
- actor_user_id (FK, nullable)
- actor_ip_address (nullable)
- action_type
- target_entity_type
- target_entity_id
- outcome
- details
- created_at

## order_cancellation (future)
- cancellation_id (PK)
- order_id (FK)
- cancelled_by_seller_id (FK)
- cancellation_reason
- comment_to_customer
- cancelled_at
- refund_status

## refund (future)
- refund_id (PK)
- cancellation_id (FK)
- order_id (FK)
- amount
- refund_status
- external_refund_reference (nullable)
- processed_at (nullable)
