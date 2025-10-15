using System;
using BlazorRealtimeChat.Services;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRealtimeChat.Controllers;

[Authorize]
[ApiController]
[Route("api/servers/{serverId}/channels")]
public class ChannelController(IChannelService channelService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetChannels(Guid serverId)
    {
        var channels = await channelService.GetChannelsAsync(serverId);
        return Ok(channels);
    }

    [HttpPost]
    public async Task<IActionResult> AddChannel(Guid serverId, CreateChannelDto createChannelDto)
    {
        var newChannel = await channelService.AddChannelAsync(createChannelDto);
        return CreatedAtAction(nameof(GetChannels), new { serverId = serverId, channelId = newChannel.ChannelId }, newChannel);
    }
}
