using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorRealtimeChat.Data.Entity;

public class Server
{
    [Key]
    public Guid ServerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ServerName { get; set; } = string.Empty;

    //-- 추가 속성
    [Required]
    public Guid OwnerId { get; set; } // 서버 소유자 ID

    [ForeignKey("OwnerId")]
    public virtual User Owner { get; set; } = null!; // 서버 소유자 정보 (@ManyToOne)

    // 관계 설정 : 하나의 서버는 여러 개의 채널을 가집니다. ( @OneToMany )
    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();
}
