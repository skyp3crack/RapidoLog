using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RapidoLog.Application.Common.Interfaces;
using RapidoLog.Domain.Entities;

namespace RapidoLog.Application.Shipments.Commands;

public record CreateShipmentCommand(string Origin, string Destination, Guid TenantId) : IRequest<string>;

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IPayNetService _payNetService;

    public CreateShipmentCommandHandler(IAppDbContext context, IPayNetService payNetService)
    {
        _context = context;
        _payNetService = payNetService;
    }

    public async Task<string> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var trackingNumber = $"MY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        var shipment = new Shipment(trackingNumber, request.Origin, request.Destination, request.TenantId);
        
        _context.Shipments.Add(shipment);

        // Fetch the payment link using our new Response class
        var payNetResponse = await _payNetService.GeneratePaymentLinkAsync(shipment.Id, 15.00m);

        var transaction = new PaymentTransaction(shipment.Id, 15.00m, payNetResponse.ReferenceId);
        _context.PaymentTransactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);

        // Return the URL
        return payNetResponse.PaymentUrl;
    }
}