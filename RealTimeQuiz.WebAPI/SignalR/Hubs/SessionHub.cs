using Microsoft.AspNetCore.SignalR;
using RealTimeQuiz.WebAPI.SignalR.HubInterfaces;
using RealTimeQuiz.WebAPI.SignalR.Services;

namespace RealTimeQuiz.WebAPI.SignalR.Hubs;

public class SessionHub : Hub<ISessionClient>
{
    private readonly ISessionNotificationService _notificationService;

    public SessionHub(ISessionNotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    
    public async Task JoinSessionGroup(int sessionId)
    {
        await _notificationService.AddToGroupAsync(Context.ConnectionId, $"Session_{sessionId}");
        await _notificationService.SendInitialLeaderboardAsync(Context.ConnectionId, sessionId);
    }

    public async Task LeaveSessionGroup(int sessionId)
    {
        await _notificationService.RemoveFromGroupAsync(Context.ConnectionId, $"Session_{sessionId}");
    }
}