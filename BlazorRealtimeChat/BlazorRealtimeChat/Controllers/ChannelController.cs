using System;
using BlazorRealtimeChat.Services;
using BlazorRealtimeChat.Shared.DTOs;
using Microsoft.AspNetCore.Authorization; // [Authorize] 속성을 사용하기 위해 추가합니다.
using Microsoft.AspNetCore.Mvc;

namespace BlazorRealtimeChat.Controllers;

[Authorize] // 바로 이 한 줄이 Spring Security의 필터와 같은 역할을 합니다.
[ApiController]
[Route("api/channel")]
public class ChannelController(IChannelService channelService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetChannels()
    {
        var channels = await channelService.GetChannelsAsync();
        return Ok(channels);
    }

    [HttpPost]
    public async Task<IActionResult> AddChannel(CreateChannelDto createChannelDto)
    {

        var newChannel = await channelService.AddChannelAsync(createChannelDto);
        return CreatedAtAction(nameof(GetChannels), new { id = newChannel.ChannelId }, newChannel);
    }
}
