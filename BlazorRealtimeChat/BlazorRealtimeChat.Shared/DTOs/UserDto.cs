using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRealtimeChat.Shared.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string LoginId { get; set; } = string.Empty; // 로그인용 아이디
        public string UserName { get; set; } = string.Empty; // 표시용 닉네임
        public string? ProfileImageUrl { get; set; }
    }
}
