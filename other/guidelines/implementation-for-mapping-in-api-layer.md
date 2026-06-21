# Implementation Notes for *MappingExtensions.cs in API Layer

## General

- Mapping all related **API layer contracts** to **Application layer DTOs** and vice versa.
- For example: If application layer needs a `CreateProductRequest`, create a `ToDto` extension method that maps from the API contract to the Application DTO.
- Similarly, for responses, create `ToResponse` methods that map from application DTOs to API response contracts.

## Nested Objects

- If there are nested objects, create a mapping `To{Model name}Response` for those as well, even if they are identical objects in the application layer.
  - **Example**: `App.CheckoutResponse` contains a list of `App.PaymentDto` and `App.PaymentDto` is also a response model for `App.PaymentResponse`. So there will have a `ToCheckoutResponse` method that maps from `App.PaymentDto` to `PaymentModel`, and a `ToPaymentResponse` method that maps from `App.PaymentDto` to `PaymentResponse`. Even if they have the same properties right now.

## Code Examples

For more code examples, see `Backend.Api/Mappings/Catalog/Categories/CategoriesMappingExtensions.cs` and `Backend.Api/Mappings/Common/PageMappingExtensions.cs`.
