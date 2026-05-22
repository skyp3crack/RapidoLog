using System;
using System.Threading.Tasks;
using RapidoLog.Application.Common.Interfaces;

namespace RapidoLog.Infrastructure.Services;

public class PayNetSimulatorService : IPayNetService
{
    public Task<PayNetLinkResponse> GeneratePaymentLinkAsync(Guid shipmentId, decimal amount)
    {
        var bankReference = $"PAYNET-{DateTime.UtcNow.Ticks}"; //mock bank reference number
        var mockUrl = $"https://sandbox.duitnow.my/checkout?ref={bankReference}&amt={amount}"; //mock DuitNow/FPX Sandbox URL
        var response = new PayNetLinkResponse //create response object 
        {
            PaymentUrl = mockUrl,
            ReferenceId = bankReference
        };
        return Task.FromResult(response); //return the mock response immediately
    }
}