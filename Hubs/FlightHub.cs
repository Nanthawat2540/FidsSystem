using Microsoft.AspNetCore.SignalR;

namespace FidsSystem.Hubs
{
    public class FlightHub : Hub
    {
        public async Task JoinGroup(string group) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

        public async Task LeaveGroup(string group) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "display");
            await base.OnConnectedAsync();
        }
    }
}
