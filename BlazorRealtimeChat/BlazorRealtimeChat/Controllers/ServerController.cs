using BlazorRealtimeChat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRealtimeChat.Controllers;

[Authorize]
[ApiController]
[Route("api/server")]
public class ServerController(IServerService serverService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetServers()
    {
        var servers = await serverService.GetServersAsync();
        return Ok(servers);
    }
}