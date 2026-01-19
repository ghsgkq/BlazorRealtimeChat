using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRealtimeChat.Shared.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? ProfileImageUrl { get; set; } = null;
    }
}
