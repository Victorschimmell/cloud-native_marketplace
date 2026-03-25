using Backend.Domain.Base;

namespace Backend.UnitTests.Base;

public class DomainBaseTypesTests
{
    [Fact]
    public void Entity_ExposesTypedId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        Assert.Equal(id, entity.Id);
        Assert.IsAssignableFrom<BaseEntity>(entity);
    }

    [Fact]
    public void AuditableEntity_StoresAuditValues()
    {
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        var entity = new TestAuditableEntity(Guid.NewGuid())
        {
            CreatedAtUtc = createdAt,
            UpdatedAtUtc = updatedAt
        };

        Assert.Equal(createdAt, entity.CreatedAtUtc);
        Assert.Equal(updatedAt, entity.UpdatedAtUtc);
    }

    [Fact]
    public void AggregateRoot_IsAnAuditableEntity()
    {
        var id = Guid.NewGuid();
        var aggregate = new TestAggregateRoot(id);

        Assert.Equal(id, aggregate.Id);
        Assert.IsAssignableFrom<AuditableEntity<Guid>>(aggregate);
    }

    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid idValue)
        {
            Id = idValue;
        }
    }

    private sealed class TestAuditableEntity : AuditableEntity<Guid>
    {
        public TestAuditableEntity(Guid idValue)
        {
            Id = idValue;
        }
    }

    private sealed class TestAggregateRoot : AggregateRoot<Guid>
    {
        public TestAggregateRoot(Guid idValue)
        {
            Id = idValue;
        }
    }
}
