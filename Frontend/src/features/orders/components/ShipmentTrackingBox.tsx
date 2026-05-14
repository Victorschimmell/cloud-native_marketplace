import type { Shipment } from '../types';
import { formatDateTime } from '../utils';
import './ShipmentTrackingBox.css';

interface ShipmentTrackingBoxProps {
  shipment: Shipment;
}

export default function ShipmentTrackingBox({ shipment }: ShipmentTrackingBoxProps) {
  const note = getShipmentNote(shipment);

  return (
    <div className="tracking-box">
      <div className="tracking-box__main">
        <span className="tracking-box__label">Tracking</span>
        <span className="tracking-box__carrier">{shipment.carrierName}</span>
        <span className="tracking-box__number">{shipment.trackingNumber}</span>
        {note ? <span className="tracking-box__note">{note}</span> : null}
      </div>
      <span className="tracking-box__status" data-tone={getShipmentTone(shipment.shipmentStatus)}>
        {getShipmentStatusLabel(shipment.shipmentStatus)}
      </span>
    </div>
  );
}

function getShipmentStatusLabel(status: Shipment['shipmentStatus']) {
  switch (status) {
    case 'ReadyForPickup':
      return 'Ready for pickup';
    case 'InTransit':
      return 'In transit';
    default:
      return status;
  }
}

function getShipmentTone(status: Shipment['shipmentStatus']) {
  if (status === 'Delivered') {
    return 'success';
  }

  if (status === 'Returned' || status === 'Lost' || status === 'Cancelled') {
    return 'danger';
  }

  if (status === 'InTransit' || status === 'ReadyForPickup') {
    return 'info';
  }

  return 'neutral';
}

function getShipmentNote(shipment: Shipment) {
  if (shipment.deliveredAtUtc) {
    return `Delivered ${formatDateTime(shipment.deliveredAtUtc)}`;
  }

  if (shipment.returnedAtUtc) {
    return `Returned ${formatDateTime(shipment.returnedAtUtc)}`;
  }

  if (shipment.shippedAtUtc) {
    return `Shipped ${formatDateTime(shipment.shippedAtUtc)}`;
  }

  return '';
}
