using Blazored.LocalStorage;
using BlazorRealtimeChat.Components;
using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Hubs;
using BlazorRealtimeChat.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ServerSideServices = BlazorRealtimeChat.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RealTimeChatContext>(options => 
    options.UseNpgsql(connectionString));

// Repository 등록
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
builder.Services.AddScoped<IServerRepository, ServerRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IServerMemberRepository, ServerMemberRepository>();

// Service 등록 시, 전체 네임스페이스 또는 별칭을 사용하여 혼동을 방지합니다.
builder.Services.AddScoped<ServerSideServices.IUserService, ServerSideServices.UserService>();
builder.Services.AddScoped<ServerSideServices.IChannelService, ServerSideServices.ChannelService>();
builder.Services.AddScoped<ServerSideServices.ITokenService, ServerSideServices.TokenService>();
builder.Services.AddScoped<ServerSideServices.IServerService, ServerSideServices.ServerService>();
builder.Services.AddScoped<ServerSideServices.IMessageService, ServerSideServices.MessageService>();

// JWT 인증 서비스 등록
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
{
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),

            NameClaimType = "name"
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // SignalR은 토큰을 "access_token" 쿼리 스트링으로 보냅니다.
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // /chathub 경로로 들어오는 요청인 경우 토큰을 인증 정보에 할당합니다.
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });


// SignalIR 서비스 추가
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 100 * 1024 * 1024; // 10MB로 확장
});

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

// Kestrel 서버 제한 해제 (예: 100MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

// FormOptions 제한 해제
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
});


var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
    app.UseDeveloperExceptionPage();
}
else
{
    // 응답 압축 미들웨어 사용
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// [추가] 외부 폴더인 ExternalUploads를 /user-files 경로로 서빙합니다.
var externalPath = Path.Combine(Directory.GetCurrentDirectory(), "ExternalUploads");
if (!Directory.Exists(externalPath)) Directory.CreateDirectory(externalPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(externalPath),
    RequestPath = "/user-files"
});

// 인증 및 권한 부여 미들웨어
app.UseAuthentication();
app.UseAuthorization();

// ChatHub 엔트포인트 매핑
app.MapHub<ChatHub>("/chathub"); // "/chathub" 경로로 Hub에 접속할 수 있게 됩니다.

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorRealtimeChat.Client._Imports).Assembly);

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RealTimeChatContext>();
    db.Database.Migrate(); // 앱 시작 시 자동으로 DB 테이블 생성
}

app.Run();
