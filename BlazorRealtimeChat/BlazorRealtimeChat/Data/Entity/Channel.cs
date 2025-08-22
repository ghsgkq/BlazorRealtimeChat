using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorRealtimeChat.Data.Entity;

public class Channel
{
    [Key]
    public Guid ChannelId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ChannelName { get; set; } = string.Empty;

    // --- 1:N 관계 설정 ---
    // 관계의 주인(N 쪽)임을 명시합니다.
    public Guid ServerId { get; set; }

    [ForeignKey("ServerId")]
    public virtual Server Server { get; set; } = null!; // 서버와의 관계 설정
    // --- N:1 관계 설정 ---

    // 관계 설정: 하나의 채널에는 여러 개의 메시지가 있습니다. (@OneToMany)
    public virtual ICollection<Message> Messages { get; set; }
}
