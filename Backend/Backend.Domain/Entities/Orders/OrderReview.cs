using Backend.Domain.Base;

namespace Backend.Domain.Entities.Orders;

public sealed class OrderReview : Entity<Guid>
{
    public OrderReview()
    {
        Id = Guid.NewGuid();
    }

    public Guid OrderId { get; set; }
    public int ReviewScore { get; set; }
    public string? ReviewCommentTitle { get; set; }
    public string? ReviewCommentMessage { get; set; }
    public DateTimeOffset ReviewCreationDateUtc { get; set; }
    public DateTimeOffset? ReviewAnswerTimestampUtc { get; set; }

    public Order? Order { get; set; }
}
