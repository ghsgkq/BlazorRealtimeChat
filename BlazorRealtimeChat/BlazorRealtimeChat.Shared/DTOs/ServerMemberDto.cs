using System;

namespace BlazorRealtimeChat.Shared.DTOs
{
    public class ServerMemberDto
    {
        public Guid UserId {get; set;}
        public string UserName {get; set; } = string.Empty;
        public string? ProfileImageUrl {get; set;}

        public bool IsOnline {get; set;}
    }    
}


