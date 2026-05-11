using Backend.Domain.Base;

namespace Backend.Domain.Entities.Orders;

public sealed class OrderReview : Entity<Guid>
{
    public OrderReview()
    {
        Id = Guid.NewGuid();
    }

    public Guid OrderId { get; set; }
    public int? OrderItemId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public string? OlistReviewId { get; set; }
    public int ReviewScore { get; set; }
    public string? ReviewCommentTitle { get; set; }
    public string? ReviewCommentMessage { get; set; }
    public DateTimeOffset ReviewCreationDateUtc { get; set; }
    public DateTimeOffset? ReviewAnswerTimestampUtc { get; set; }

    public Order? Order { get; set; }
    public OrderItem? OrderItem { get; set; }
    public IdentityAccess.Customer? Customer { get; set; }
    public Catalog.Product? Product { get; set; }
}
