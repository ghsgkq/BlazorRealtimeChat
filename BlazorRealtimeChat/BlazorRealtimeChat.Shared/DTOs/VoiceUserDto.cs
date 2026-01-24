using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRealtimeChat.Shared.DTOs
{
    public class VoiceUserDto
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }
}
