using Microsoft.AspNetCore.SignalR;

namespace RapidoLog.Api.Hubs;

/// <summary>
/// SignalR hub for real-time shipment tracking updates.
/// Frontend clients connect here to receive live status transitions
/// (e.g., PendingPayment → Paid → ReadyForDispatch) pushed from the Saga engine.
/// </summary>
public class ShipmentTrackingHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[SignalR] Client connected: {Context.ConnectionId}");
        Console.ResetColor();
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Allows a frontend client to subscribe to updates for a specific shipment.
    /// Usage: hub.invoke("TrackShipment", shipmentId)
    /// </summary>
    public async Task TrackShipment(string shipmentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, shipmentId);
        Console.WriteLine($"[SignalR] Client {Context.ConnectionId} now tracking shipment: {shipmentId}");
    }
}
