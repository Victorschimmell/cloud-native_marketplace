using Backend.Domain.Enums;

namespace Backend.UnitTests.Enums;

public class DomainEnumsTests
{
    [Theory]
    [MemberData(nameof(AllDomainEnumTypes))]
    public void DomainEnums_ExposeDefinedValues(Type enumType)
    {
        var values = Enum.GetValues(enumType);

        Assert.NotEmpty(values);
        Assert.All(values.Cast<object>(), value => Assert.True(Enum.IsDefined(enumType, value)));
    }

    public static TheoryData<Type> AllDomainEnumTypes =>
    [
        typeof(AccountStatus),
        typeof(AuditActionType),
        typeof(AuditOutcome),
        typeof(CartStatus),
        typeof(IncidentSeverity),
        typeof(IncidentStatus),
        typeof(IncidentType),
        typeof(ListingVisibilityStatus),
        typeof(NotificationType),
        typeof(OrderStatus),
        typeof(PaymentStatus),
        typeof(PaymentType),
        typeof(SellerVerificationRequestStatus),
        typeof(ShipmentStatus),
        typeof(UserRole),
        typeof(VerificationStatus)
    ];
}
