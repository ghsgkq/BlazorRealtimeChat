using System;

namespace BlazorRealtimeChat.Shared.DTOs
{
    public class UpdateServerDto
    {
        public string ServerName { get; set; } = string.Empty;

        public string? ProfileImageUrl { get; set; }
    }
}


