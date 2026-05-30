using System.Threading.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using RapidoLog.Api.Hubs;
using RapidoLog.Application.Shipments.Commands;
using RapidoLog.Infrastructure;
using RapidoLog.Infrastructure.Persistence; 

var builder = WebApplication.CreateBuilder(args);

//  Wire up our Architecture Layers
builder.Services.AddInfrastructure(builder.Configuration);

//  Scan our Application layer for those Commands we wrote
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateShipmentCommand).Assembly));

//  Register SignalR for real-time shipment tracking
builder.Services.AddSignalR();

//  Register Rate Limiting middleware (DDoS mitigation / RMiT compliance)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed Window: 10 requests per 60-second window per client IP
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(60);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});

var app = builder.Build();

//this will auto create the database and tables on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

//  Activate Rate Limiting middleware in the pipeline
app.UseRateLimiter();


// The Logistics Client requests a new shipment
app.MapPost("/api/shipments", async (CreateShipmentCommand command, IMediator mediator) =>
{
    var paymentUrl = await mediator.Send(command);  // Send the command to Phase 3's CreateShipmentCommandHandler

    return Results.Ok(new 
    { 
        Message = "Shipment created successfully. Pending Payment.", 
        PayNetUrl = paymentUrl 
    });
}).RequireRateLimiting("FixedPolicy");

//  Bank Negara / PayNet sends us an asynchronous background update (The Webhook)
app.MapPost("/api/webhooks/paynet", async (ProcessPaymentWebHookCommand command, IMediator mediator, IHubContext<ShipmentTrackingHub> hubContext) =>
{
    var success = await mediator.Send(command);  //  ProcessPaymentWebHookCommandHandler (The Saga)
    if (success)
    {
        //  Broadcast real-time status update to connected frontend clients
        await hubContext.Clients.Group(command.PayNetReference).SendAsync("ShipmentStatusUpdated", new
        {
            Reference = command.PayNetReference,
            Status = command.Status,
            Timestamp = DateTime.UtcNow
        });

        return Results.Ok(new { Status = "Webhook processed and Saga updated." });
    }
        
    return Results.BadRequest(new { Error = "Invalid transaction reference or shipment not found." });
}).RequireRateLimiting("FixedPolicy");

//  Map SignalR hub for real-time shipment tracking
app.MapHub<ShipmentTrackingHub>("/hubs/shipment-tracking");

app.Run();