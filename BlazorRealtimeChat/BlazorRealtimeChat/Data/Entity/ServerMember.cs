using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorRealtimeChat.Data.Entity
{
    public class ServerMember
    {
        [Key]
        public int Id {get; set;}

        [Required]
        public Guid ServerId {get; set;}
        
        [Required]
        public Guid UserId {get; set;}

        public DateTime JoinedAt {get; set;} = DateTime.UtcNow;


        // 관계형 데이터 연결 (Navigation Properties)
        [ForeignKey("ServerId")]
        public virtual Server Server { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

}


