using System;
using Microsoft.EntityFrameworkCore;

namespace BlazorRealtimeChat.Data;

public class RealTimeChatContextFactory
{
    public RealTimeChatContext CreateDbContext(string[] args)
    {
        // 1. appsettings.json 파일 경로를 찾습니다.
        var Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // 2. appsettings.json에서 연결 문자열을 읽어옵니다.
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        // 3. DbContextOptions를 직접 설정합니다.
        var optionsBuilder = new DbContextOptionsBuilder<RealTimeChatContext>();
        optionsBuilder.UseNpgsql(connectionString);

        // 4. 설정된 Options를 사용하여 DbContext 인스턴스를 생성하고 반환합니다.
        return new RealTimeChatContext(optionsBuilder.Options);
    }
}
