using System.Text;
using Blazored.LocalStorage;
using BlazorRealtimeChat.Components;
using BlazorRealtimeChat.Data;
using BlazorRealtimeChat.Hubs;
using BlazorRealtimeChat.Repositories;
using ServerSideServices = BlazorRealtimeChat.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

// Service 등록 시, 전체 네임스페이스 또는 별칭을 사용하여 혼동을 방지합니다.
builder.Services.AddScoped<ServerSideServices.IUserService, ServerSideServices.UserService>();
builder.Services.AddScoped<ServerSideServices.IChannelService, ServerSideServices.ChannelService>();
builder.Services.AddScoped<ServerSideServices.ITokenService, ServerSideServices.TokenService>();
builder.Services.AddScoped<ServerSideServices.IServerService, ServerSideServices.ServerService>();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        };
    });


// SignalIR 서비스 추가
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});


var app = builder.Build();

// 응답 압축 미들웨어 사용
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// 인증 및 권한 부여 미들웨어
app.UseAuthentication();
app.UseAuthorization();

// ChatHub 엔트포인트 매핑
app.MapHub<ChatHub>("/chathub"); // "/chathub" 경로로 Hub에 접속할 수 있게 됩니다.

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorRealtimeChat.Client._Imports).Assembly);

app.MapControllers();

app.Run();
