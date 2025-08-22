using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorRealtimeChat.Data.Entity;

public class User
{
    [Key] // 이 속성이 기본 키 (Primary Key)임을 명시합니다. (@Id)
    public Guid UserId { get; set; }

    [Required] // 이 컬럼 Not Null 입니다 (@NotNull)
    [MaxLength(50)] // 최대 길이를 50으로 제한합니다.
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty; // 비밀번호 해시해서 저장합니다.

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 관계 설정: 한 명의 유저는 여러 개의 메시지를 작성할 수 있습니다. ( @OneToMany )
    public virtual ICollection<Message> Messages { get; set; }
}
