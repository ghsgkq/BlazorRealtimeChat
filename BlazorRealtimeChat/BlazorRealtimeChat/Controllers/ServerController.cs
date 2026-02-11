using BlazorRealtimeChat.Services;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        // 토큰에서 현재 사용자 uid 를 가져온다.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var server = await serverService.AddServerAsync(createServerDto: serverDto, ownerId: Guid.Parse(userId));
        return Ok(server);
    }
}
