using System;
using System.Threading.Tasks;
using RapidoLog.Application.Common.Interfaces;
using Polly;

namespace RapidoLog.Infrastructure.Services;

public class PayNetSimulatorService : IPayNetService
{
    private static int _requestCounter=0; //to simulate payment realistic back and forth network flakes
    public async Task<PayNetLinkResponse> GeneratePaymentLinkAsync(Guid shipmentId, decimal amount)
    {
        var retryPolicy = Policy //Define retry policy using Polly to avoid connection timeout issues
        .Handle<Exception>()
        .WaitAndRetryAsync( //If an exception occurs, it retries 3 times, waiting longer between each try (2s, 4s, 8s)
            retryCount:3,
            sleepDurationProvider:retryAttempt=>TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)),
            onRetry:(exception,timeSpan,retryCount,context)=>
            {
                Console.ForegroundColor= ConsoleColor.Yellow; //Log warning to console to track transparency during execution flaking
                Console.WriteLine($"[RMiT Warning] PayNet Gateway time out. Retry #{retryCount} after {timeSpan.Seconds}s. Error: {exception.Message}");
                Console.ResetColor();
            });
        return await retryPolicy.ExecuteAsync(async ()=>
        {
            _requestCounter++; 
            if(_requestCounter % 3!=0)
            {
                throw new TimeoutException("Network Connection failed on routing switch rails.");
            }
            var bankReference = $"PAYNET-{DateTime.UtcNow.Ticks}"; //mock bank reference number
            var mockUrl = $"https://sandbox.duitnow.my/checkout?ref={bankReference}&amt={amount}"; //mock DuitNow/FPX Sandbox URL
            await Task.CompletedTask;

            return new PayNetLinkResponse
            {
                PaymentUrl = mockUrl,
                ReferenceId = bankReference
            };
        });   
    }
}