using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

public class PollHub : Hub
{
    public async Task JoinPollRoom(string code)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, code);
    }

    // And check your leave method while you're there:
    public async Task LeavePollRoom(string code)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
    }
}