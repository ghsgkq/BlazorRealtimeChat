using System;
using BlazorRealtimeChat.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Data;

public class RealTimeChatContext : DbContext
{
    public RealTimeChatContext(DbContextOptions<RealTimeChatContext> options)
        : base(options)
    {
    }

        // 이 DbSet들이 데이터베이스의 테이블이 됩니다.
    public DbSet<User> Users { get; set; }
    public DbSet<Server> Servers { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Message> Messages { get; set; }
    
}
