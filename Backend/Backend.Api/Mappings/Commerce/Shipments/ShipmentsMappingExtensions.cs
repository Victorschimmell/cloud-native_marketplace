using Backend.Api.Contracts.Commerce.Shipments;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Commerce.Shipments;

public static class ShipmentsMappingExtensions
{
    public static ShipmentModel ToModel(this App.ShipmentDto shipment) =>
        new()
        {
            Id = shipment.Id,
            OrderId = shipment.OrderId,
            SellerId = shipment.SellerId,
            CarrierName = shipment.CarrierName,
            TrackingNumber = shipment.TrackingNumber,
            ShipmentStatus = (ShipmentStatus)shipment.ShipmentStatus,
            ShippedAtUtc = shipment.ShippedAtUtc,
            DeliveredAtUtc = shipment.DeliveredAtUtc,
            ReturnedAtUtc = shipment.ReturnedAtUtc,
        };

    public static ShipmentResponse ToResponse(this App.ShipmentDto shipment) =>
        new()
        {
            Id = shipment.Id,
            OrderId = shipment.OrderId,
            SellerId = shipment.SellerId,
            CarrierName = shipment.CarrierName,
            TrackingNumber = shipment.TrackingNumber,
            ShipmentStatus = (ShipmentStatus)shipment.ShipmentStatus,
            ShippedAtUtc = shipment.ShippedAtUtc,
            DeliveredAtUtc = shipment.DeliveredAtUtc,
            ReturnedAtUtc = shipment.ReturnedAtUtc,
        };
}
