using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRealtimeChat.Shared.DTOs;

    public class MessageDto
    {
        public Guid MessageId { get; set; }
        public string? Content { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; } = null!;
        public Guid ChannelId { get; set; }
        public string? ChannelName { get; set; } = null;
        
        // -- 추가된 필드 ---
        public string? FileUrl { get; set; } = null;
        public string MessageType { get; set; } = "text";
        public string? ProfileImageUrl { get; set; } = null;
}

