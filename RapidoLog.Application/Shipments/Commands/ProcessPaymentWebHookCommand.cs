using System;
using System.Threading; //threading is for async
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RapidoLog.Application.Common.Interfaces;
using RapidoLog.Domain.Enums;

namespace RapidLog.Application.Common.Interfaces;

//The command where : the exactJSON payload PayNet will send to us
public record ProcessPaymentWebHookCommand(string PayNetReference, string Status) : IRequest<bool>; //<bool> becouse we only need the confirmation (True or False)
// the handler: the saga decision engine checking the data and decide
public class ProcessPaymentWebHookCommandHandler : IRequestHandler<ProcessPaymentWebHookCommand,bool>
{
    private readonly IAppDbContext _context;

    public ProcessPaymentWebHookCommandHandler(IAppDbContext context)
    {
        _context = context ; //Inject real database context
    }

    public async Task<bool> Handle(ProcessPaymentWebHookCommand request, CancellationToken cancellationToken)
    {
        //Find the transaction using the bank's reference number
        var transaction = await _context.PaymentTransactions
        .FirstOrDefaultAsync(t =>t.PayNetReference == request.PayNetReference, cancellationToken);

        if(transaction ==null)
        return false ; //transaction not fund , ignore the webhook

        //find the associated shipment 
        var shipment = await _context.Shipments
        .FirstOrDefaultAsync(s => s.Id == transaction.ShipmentId, cancellationToken);
        
        if(shipment == null) 
        return false;

        //IDEMPOTENCY CHECK: If it's already paid, don't process it again
        if(shipment.Status !=ShipmentStatus.PendingPayment)
        return true;

        //Decision , success of fail
        if(request.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
        {
            transaction.MarkAsSuccess();
            shipment.MarkAsPaid();
            shipment.MarkAsReadyForDispatch(); //trigger the logistics pipeline
        }
        else
        {
            transaction.MarkAsFailed();
            shipment.CancelShipment();
        }

        await _context.SaveChangesAsync(cancellationToken); //save the final state
        return true; //send a success response to PayNet Server
    }
}