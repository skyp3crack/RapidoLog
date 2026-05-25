using System.Threading.Tasks;
namespace RapidoLog.Application.Common.Interfaces;

public record  StructuredAddress(string Street, string City, string Postcode, string State);

public interface IAiRoutingService
{
    Task<StructuredAddress>  ExtractAddressAsync(string rawAddress);   //LLm accept rawAddress and return it in the structuredAddress format 
}