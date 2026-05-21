//track handshake with Paynet/DuitNow

using System;
using RapidoLog.Domain.Enums;

namespace RapidoLog.Domain.Entities;

public class PaymentTransaction
{
    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public decimal Amount { get; private set; }
    public string PayNetReference { get; private set; }
    public PaymentStatus Status { get; private set; }

    private PaymentTransaction(){}

    public PaymentTransaction ( Guid shipmentId, decimal amount, string payNetReference)
    {

        if(amount <=0) throw new ArgumentException("Amount must be greater than zero")

        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        Amount = amount;
        PayNetReference = payNetReference;
        Status = PaymentStatus.Pending;
    }

    public void MarkAsSuccess()
    {
        Status = PaymentStatus.Success;
    }
    
    public void MarkAsFailed()
    {
        Status = PaymentStatus.Failed;
    }
}