using BlazorRealtimeChat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRealtimeChat.Controllers;

[Authorize]
[ApiController]
[Route("api/messages")]
public class MessageController(IMessageService messageService) : ControllerBase
{
    [HttpGet("{channelId}")]
    public async Task<IActionResult> GetMessagesByChannelId(Guid channelId, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        var messages = await messageService.GetMessageByChannelIdAsync(channelId, limit, offset);
        return Ok(messages);
    }
}

