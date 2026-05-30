using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RapidoLog.Application.Common.Interfaces;
using RapidoLog.Infrastructure.Persistence;
using RapidoLog.Infrastructure.Services;

namespace RapidoLog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options=>options.UseSqlServer(connectionString));//Register database
        services.AddScoped<IAppDbContext>(provider =>provider.GetRequiredService<AppDbContext>());//Bind Interface with actual database context
        services.AddTransient<IPayNetService, PayNetSimulatorService>(); //Bind interface to our simulator
        
        services.AddHttpClient<IAiRoutingService, AiRoutingService>(client => 
        {
            var baseUrl = configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}
