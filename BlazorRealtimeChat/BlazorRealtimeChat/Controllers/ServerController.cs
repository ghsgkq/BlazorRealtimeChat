using BlazorRealtimeChat.Services;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRealtimeChat.Controllers;

[Authorize]
[ApiController]
[Route("api/servers")] 
public class ServerController(IServerService serverService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetServers()
    {
        var servers = await serverService.GetServersAsync();
        return Ok(servers);
    }

    [HttpPost]
    public async Task<IActionResult> AddServer(CreateServerDto serverDto)
    {
        var server = await serverService.AddServerAsync(serverDto);
        return Ok(server);
    }
}
