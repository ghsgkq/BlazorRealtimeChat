using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorRealtimeChat.Data.Entity;

public class Message
{
    [Key]
    public Guid MessageId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // --- User와의 관계 (N:1) ---
    public Guid UserId { get; set; } // 작성자 외래 키

    [ForeignKey("UserId")]
    public virtual User User { get; set; } // 작성자 정보 (@ManyToOne)
    // ---------------------------

    // --- Channel과의 관계 (N:1) ---
    public Guid ChannelId { get; set; } // 채널 외래 키

    [ForeignKey("ChannelId")]
    public virtual Channel Channel { get; set; } // 채널 정보 (@ManyToOne)


}
