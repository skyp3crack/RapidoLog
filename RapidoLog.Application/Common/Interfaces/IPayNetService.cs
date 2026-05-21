using System;
using System.Threading.Tasks;

namespace RapidoLog.Application.Common.Interfaces;

public interfaces IpayNetService
{
    Task<(string PaymentUrl, string ReferenceId)> GeneratePaymentLinkAsync(Guid shipmentId, decimal amount);
}
