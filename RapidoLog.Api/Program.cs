using MediatR;
using RapidoLog.Application.Shipments.Commands;
using RapidoLog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//  Wire up our Architecture Layers
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddInfrastructure(connectionString!);

//  Scan our Application layer for those Commands we wrote
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateShipmentCommand).Assembly));

var app = builder.Build();


// The Logistics Client requests a new shipment
app.MapPost("/api/shipments", async (CreateShipmentCommand command, IMediator mediator) =>
{
    var paymentUrl = await mediator.Send(command);  // Send the command to Phase 3's CreateShipmentCommandHandler

    return Results.Ok(new 
    { 
        Message = "Shipment created successfully. Pending Payment.", 
        PayNetUrl = paymentUrl 
    });
});

//  Bank Negara / PayNet sends us an asynchronous background update (The Webhook)
app.MapPost("/api/webhooks/paynet", async (ProcessPaymentWebHookCommand command, IMediator mediator) =>
{
    var success = await mediator.Send(command);  //  ProcessPaymentWebHookCommandHandler (The Saga)
    if (success)
        return Results.Ok(new { Status = "Webhook processed and Saga updated." });
        
    return Results.BadRequest(new { Error = "Invalid transaction reference or shipment not found." });
});

app.Run();