using Backend.Domain.Entities.Location;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Enums;

namespace Backend.UnitTests.Entities;

public class LocationAndOperationsEntitiesTests
{
    [Fact]
    public void Address_StoresLatitudeLongitudeAndCollections()
    {
        var entity = new Address
        {
            PostalCode = "1050",
            City = "Copenhagen",
            State = "Capital Region",
            CountryCode = "DK",
            Latitude = 55.6761m,
            Longitude = 12.5683m
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(55.6761m, entity.Latitude);
        Assert.Equal(12.5683m, entity.Longitude);
        Assert.Empty(entity.DefaultForCustomers);
        Assert.Empty(entity.DefaultForSellers);
        Assert.Empty(entity.ShippingOrders);
    }

    [Fact]
    public void AuditLog_InitializesIdentityAndOutcome()
    {
        var entity = new AuditLog
        {
            TargetEntityType = "Order",
            TargetEntityId = "123",
            Details = "Order created",
            ActionType = AuditActionType.Created,
            Outcome = AuditOutcome.Succeeded
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(AuditOutcome.Succeeded, entity.Outcome);
    }
}
