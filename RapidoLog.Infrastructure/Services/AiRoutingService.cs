using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using RapidoLog.Application.Common.Interfaces;

namespace RapidoLog.Infrastructure.Services;

public class AiRoutingService: IAiRoutingService
{
    private readonly HttpClient _httpClient;  
    public AiRoutingService(HttpClient httpClient) //Receives an HttpClient instance via Dependency Injection 
    {
        _httpClient = httpClient;
    }
    public async Task<StructuredAddress> ExtractAddressAsync (string rawAddress)
    {
        var requestBody = new { rawAddress = rawAddress}; 
        var writeOptions = new JsonSerializerOptions { PropertyNamingPolicy = null}; // ensure the request body is sent exactly as it is
        var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/extract-address", requestBody,writeOptions); //POST and automatically serializes 'requestBody' into JSON format for the request body.
        response.EnsureSuccessStatusCode(); // throws an exception if the response status code is an error 
        
        var readOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive=true}; //(from the Python/LLM API) into standard C# property names.
        var result = await response.Content.ReadFromJsonAsync<StructuredAddress>(readOptions); // it uses the jsonOptions to read
        return result ?? new StructuredAddress(rawAddress, "Unknown", "00000", "Unknown"); //returns the extracted address if result is null return default address
    }
}
