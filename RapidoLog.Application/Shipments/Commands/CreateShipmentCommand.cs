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
    private readonly IAiRoutingService _aiRoutingService;

    public CreateShipmentCommandHandler(IAppDbContext context, IPayNetService payNetService, IAiRoutingService aiRoutingService)
    {
        _context = context;
        _payNetService = payNetService;
        _aiRoutingService = aiRoutingService; //injected AiRoutingService via Constructor
    }

    public async Task<string> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var structuredAddress = await _aiRoutingService.ExtractAddressAsync(request.Destination); //Extract structured address components from AI microservice
        var cleanDestination = $"{structuredAddress.Street}, { structuredAddress.Postcode}, {structuredAddress.City}, {structuredAddress.State}";//ormat the components into an enterprise-standard Malaysian address format.

        var trackingNumber = $"MY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        var shipment = new Shipment(trackingNumber, request.Origin,cleanDestination, request.TenantId);
        
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