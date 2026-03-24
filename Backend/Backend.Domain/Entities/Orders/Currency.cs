using Backend.Domain.Base;

namespace Backend.Domain.Entities.Orders;

public sealed class Currency : Entity<Guid>
{
    public Currency()
    {
        Id = Guid.NewGuid();
    }

    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Symbol { get; set; }

    public ICollection<OrderPayment> OrderPayments { get; } = [];
}
