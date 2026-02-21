using BlazorRealtimeChat.Data.Entity;
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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var servers = await serverService.GetServersAsync(Guid.Parse(userId));
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

    [HttpPost("{serverId}/join")]
    public async Task<IActionResult> JoinServer(Guid serverId)
    {
        // 토큰에서 현재 사용자 uid 를 가져온다.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // 서비스 계층에 가입 로직을 요청
        var result = await serverService.JoinServerAsync(serverId: serverId, userId: Guid.Parse(userId));

        if (result)
        {
            return Ok(new { Message = "서버에 성공적으로 참가했습니다." });
        }
        else
        {
            return BadRequest("서버를 찾을 수 없거나 참가에 실패했습니다.");
        }
    }

    [HttpGet("{serverId}/preview")]
    public async Task<IActionResult> GetServerPreview(Guid serverId)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized("인증되지 않은 사용자입니다.");
        }

        var preview = await serverService.GetServerPreviewAsync(serverId, userId);
        
        if (preview == null) return NotFound("서버를 찾을 수 없습니다.");

        return Ok(preview);
    }
    
    [HttpGet("{serverId}/members")]
    public async Task<IActionResult> GetServerMembers(Guid serverId)
    {
        var members = await serverService.GetServerMembersAsync(serverId);
        return Ok(members);
    }
}
