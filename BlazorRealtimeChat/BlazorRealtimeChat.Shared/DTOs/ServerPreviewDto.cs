using System;

namespace BlazorRealtimeChat.Shared.DTOs
{
    public class ServerPreviewDto
    {
        public Guid ServerId {get; set;}
        public string ServerName {get; set; } = string.Empty;

        public bool IsAlreadyMember {get; set;}

    }    
}


