using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using RapidoLog.Aplication.Common.Interface;

namespace RapidoLog.Infrastracture.Services;

public class AiRoutingService: IAiRoutingService
{
    private readonly HttpClient _httpClient;  
    public AiRoutingService(HttpClient httpClient) //Receives an HttpClient instance via Dependency Injection 
    {
        _httpClient = httpClient;
    }
    public async Task<StructuredAddress> ExtractAddressAsync (string rawAddress)
    {
        var requestBody = new { raw_address = rawAddress}; 
        var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/api/extract-address", requestBody); //POST and automatically serializes 'requestBody' into JSON format for the request body.
        response.EnsureSuccessStatusCode(); // throws an exception if the response status code is an error 
        
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase}; //(from the Python/LLM API) into standard C# property names.
        var result = await response.Content.ReadFromJsonAsync<StructuredAddress>(jsonOptions); // it uses the jsonOptions to read
        return result ?? new StructuredAddress(rawAddress, "Unknown", "00000", "Unknown"); //returns the extracted address if result is null return default address
    }
}
