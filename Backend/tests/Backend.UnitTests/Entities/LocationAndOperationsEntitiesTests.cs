using Backend.Domain.Entities.Location;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;

namespace Backend.UnitTests.Entities;

public class LocationAndOperationsEntitiesTests
{
    [Fact]
    public void Address_StoresCoordinateValueObjectAndCollections()
    {
        var entity = new Address
        {
            PostalCode = "1050",
            City = "Copenhagen",
            State = "Capital Region",
            CountryCode = "DK",
            Coordinates = new GeoCoordinate(55.6761m, 12.5683m)
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(new GeoCoordinate(55.6761m, 12.5683m), entity.Coordinates);
        Assert.Empty(entity.DefaultForCustomers);
        Assert.Empty(entity.DefaultForSellers);
        Assert.Empty(entity.ShippingOrders);
    }

    [Fact]
    public void Geolocation_StoresZipAreaValues()
    {
        var entity = new Geolocation
        {
            ZipCodePrefix = 105,
            Latitude = 55.6761m,
            Longitude = 12.5683m,
            City = "Copenhagen",
            State = "Capital Region"
        };

        Assert.Equal(105, entity.ZipCodePrefix);
        Assert.Equal("Copenhagen", entity.City);
    }

    [Fact]
    public void Notification_InitializesIdentityAndType()
    {
        var entity = new Notification
        {
            Title = "Order update",
            Body = "Your order shipped.",
            NotificationType = NotificationType.OrderStatusChanged
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(NotificationType.OrderStatusChanged, entity.NotificationType);
    }

    [Fact]
    public void SystemIncident_InitializesIdentityAndSeverity()
    {
        var entity = new SystemIncident
        {
            ComponentName = "Checkout",
            Message = "Latency spike",
            IncidentType = IncidentType.Performance,
            Severity = IncidentSeverity.High,
            Status = IncidentStatus.Open
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(IncidentSeverity.High, entity.Severity);
        Assert.Equal(IncidentStatus.Open, entity.Status);
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
