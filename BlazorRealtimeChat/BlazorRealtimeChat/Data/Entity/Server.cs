using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorRealtimeChat.Data.Entity;

public class Server
{
    [Key]
    public Guid ServerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ServerName { get; set; } = string.Empty;

    // 관계 설정 : 하나의 서버는 여러 개의 채널을 가집니다. ( @OneToMany )
    public virtual ICollection<Channel> Channels { get; set; }
}
