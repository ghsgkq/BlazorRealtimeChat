using System;
using BlazorRealtimeChat.Services;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRealtimeChat.Controllers;

[ApiController]
[Route("api/channel")]
public class ChannelController : ControllerBase
{
    private readonly IChannelService _channelService;

    public ChannelController(IChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetChannels()
    {
        var channels = await _channelService.GetChannelsAsync();
        return Ok(channels);
    }

    [HttpPost]
    public async Task<IActionResult> AddChannel(CreateChannelDto createChannelDto)
    {

        var newChannel = await _channelService.AddChannelAsync(createChannelDto);
        return CreatedAtAction(nameof(GetChannels), new { id = newChannel.ChannelId }, newChannel);
    }
}
