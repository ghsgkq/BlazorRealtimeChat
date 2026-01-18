using Blazored.LocalStorage;
using BlazorRealtimeChat.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 인증 관련 서비스 등록
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// 인터셉터 역할을 할 핸들러들을 먼저 등록합니다.
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddScoped<UnauthorizedInterceptor>();

// 기본 HttpClient 등록을, 인터셉터 체인이 포함된 HttpClient로 교체합니다.
builder.Services.AddScoped(sp =>
{
    // 가장 바깥쪽 핸들러(가장 먼저 응답을 확인)부터 체인을 구성합니다.
    var unauthorizedInterceptor = sp.GetRequiredService<UnauthorizedInterceptor>();
    // 그 안쪽에는 토큰을 추가하는 핸들러가 위치합니다.
    var tokenHandler = sp.GetRequiredService<CustomAuthorizationMessageHandler>();

    // 체인 연결: UnauthorizedInterceptor -> CustomAuthorizationMessageHandler -> (기본 핸들러)
    unauthorizedInterceptor.InnerHandler = tokenHandler;
    // 가장 안쪽 핸들러는 새로운 HttpClientHandler로 설정합니다.
    tokenHandler.InnerHandler = new HttpClientHandler();

    // 완성된 체인의 가장 바깥쪽 핸들러를 사용하여 HttpClient를 생성합니다.
    return new HttpClient(unauthorizedInterceptor)
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
});

// 서비스 등록
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IChannelService, ChannelService>();
builder.Services.AddScoped<IServerService, ServerService>(); // 새로 만든 서버 서비스를 등록합니다.
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IUserService, UserService>();

await builder.Build().RunAsync();
