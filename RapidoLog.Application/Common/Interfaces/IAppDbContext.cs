using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RapidoLog.Domain.Entities;


namespace RapidoLog.Application.Common.Interfaces; //namespace used for clean architecture


public interface IAppDbContext
{
    DbSet<Shipment> Shipments {get;} 
    DbSet<PaymentTransaction> PaymentTransactions {get;}
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}