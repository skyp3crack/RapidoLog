using System;
using RapidoLog.Domain.Enums;

namespace RapidoLog.Domain.Entities;

public class Shipment
{
    public Guid Id { get; private set; } //Guid for tracking
    public string TrackingNumber { get; private set; } =string.Empty;
    public string Origin { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public ShipmentStatus Status { get; private set; }

    private Shipment (){} //constructor for entityframework
    public Shipment(string trackingNumber, string origin, string destination, Guid tenantId){

        if(string.IsNullOrWhiteSpace(trackingNumber)) throw new ArgumentException("Tracking Number Required");
        Id =Guid.NewGuid(); //generate random guid for the id
        TrackingNumber = trackingNumber;
        Origin=origin;
        Destination= destination;
        TenantId = tenantId;
        Status = ShipmentStatus.PendingPayment; // Default starting state
    }

    public void MarkAsReadyForDispatch() // Ensure this spelling is exact
    {
     Status = ShipmentStatus.ReadyForDispatch;
    }  

    public void MarkAsPaid()
    {
        if( Status!= ShipmentStatus.PendingPayment)
           throw new InvalidOperationException("Only pending shipments can be marked as paid.");
        Status = ShipmentStatus.Paid; // allow us to change the status.
        
    }

    public void CancelShipment()
    {
        Status = ShipmentStatus.Cancelled;
    }
}