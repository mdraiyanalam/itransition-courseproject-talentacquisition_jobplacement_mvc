using Microsoft.AspNetCore.SignalR;

namespace talentacquisition_jobplacement_mvc.Hubs
{
    public class DiscussionHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.UtcNow.ToString("g"));
        }
    }
}