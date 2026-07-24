using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace talentacquisition_jobplacement_mvc.Hubs
{
    [Authorize(Roles = "Candidate,Recruiter,Administrator")]
    public class DiscussionHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Broadcast message to all connected clients
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.UtcNow.ToString("g"));
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"✅ Client connected: {Context.ConnectionId}");
            await Clients.All.SendAsync("UserJoined", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"❌ Client disconnected: {Context.ConnectionId}");
            if (exception != null)
            {
                Console.WriteLine($"Disconnection reason: {exception.Message}");
            }
            await Clients.All.SendAsync("UserLeft", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}