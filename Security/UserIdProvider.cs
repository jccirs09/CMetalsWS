using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public sealed class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext c) =>
        c.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? c.User?.FindFirstValue("sub")
        ?? c.User?.Identity?.Name;
}
