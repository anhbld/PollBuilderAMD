using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PollBuilder.API.Hubs
{
    public class PollHub : Hub
    {
        public async Task JoinPollRoom(string pollCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, pollCode);
        }

        public async Task LeavePollRoom(string pollCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, pollCode);
        }
    }
}