


namespace RapidoLog.Application.Shipment.Commands;

//data we need from the user
public record CreateShipmentCommand(string Origin,string Destination, Guid TenantId)
:IRequest<string>;

//handler
public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, string>
{

    private readonly IAppDbContext _context;
    private readonly IPayNetService _payNetService; // Inject PayNet Service

//Inject real interfaces, not real database
public CreateShipmentCommandHandler(IAppDbContext context, IPayNetService payNetService)
   {
    _context = context;
    _payNetService = payNetService;
   } 

   public async Task<string> Handle (CreateShipmentCommand request, CancellationToken cancellationToken)
   {
    //Generat a fake tracking number
     var trackingNumber = $"MY-{Guid.NewGuid().ToString().Substring(0,8).ToUpper()}";

    //Create the domain entity using the rules
    var shipment = new Shipment(trackingNumber, request.Origin, request.Destination, request.TenantId);
    _context.Shipments.Add(shipment); 

    //Ask paynet for a payment link (cost is hardcoded for now to RM15.00)
    var payNetResponse = await _payNetService.GeneratePaymentLinkAsync(shipment.Id,15.00m);

    //Record the payment transaction
    var transaction = new PaymentTransaction(shipment.Id, 15.00m, payNetResponse.ReferenceId);
    _context.PaymentTransactions.Add(transaction);

    //Save to database
    await _context.SaveChangesAsync(cancellationToken);

    return trackingNumber; //return the url to the user so they can pay


   }

}
    