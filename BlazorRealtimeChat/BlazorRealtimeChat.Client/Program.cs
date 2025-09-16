using Blazored.LocalStorage;
using BlazorRealtimeChat.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// HttpClient를 앱 전체에서 사용할 수 있도록 등록
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Scoped로 AuthService 등록
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IChannelService, ChannelService>();

// Blazored.LocalStorage 서비스 등록
builder.Services.AddBlazoredLocalStorage();


await builder.Build().RunAsync();
