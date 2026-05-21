using System;
using System.Threading.Tasks;

namespace RapidoLog.Application.Common.Interfaces;

// 1. A highly safe standard class for the response
public class PayNetLinkResponse
{
    public string PaymentUrl { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
}

// 2. The Interface contract
public interface IPayNetService
{
    Task<PayNetLinkResponse> GeneratePaymentLinkAsync(Guid shipmentId, decimal amount);
}