using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Result<OrderDto>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderDto>>> GetByCustomerAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<OrderItemDto>>> GetOrderItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> CancelAsync(CancelOrderRequest request, CancellationToken cancellationToken = default);
}

public interface IPaymentService
{
    Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default);
}

public interface IReviewService
{
    Task<Result<IReadOnlyList<ReviewDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ReviewDto>>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken = default);
}

public interface ICartService
{
    Task<Result<CartDto>> GetCartAsync(GetCartRequest request, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> RemoveItemAsync(RemoveCartItemRequest request, CancellationToken cancellationToken = default);
}

public interface ICheckoutService
{
    Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CheckoutLineDto>>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, CancellationToken cancellationToken = default);
}

public interface IShipmentService
{
    Task<Result<IReadOnlyList<ShipmentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<ShipmentDto>> RecordShipmentAsync(RecordShipmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<ShipmentDto>> UpdateShipmentStatusAsync(UpdateShipmentStatusRequest request, CancellationToken cancellationToken = default);
}
