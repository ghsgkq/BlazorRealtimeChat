using Microsoft.AspNetCore.Components;
using System.Net;

namespace BlazorRealtimeChat.Client.Services
{
    public class UnauthorizedInterceptor : DelegatingHandler
    {
        private readonly NavigationManager _navigationManager;

        public UnauthorizedInterceptor(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // 401 Unauthorized 응답을 받으면, 로그인 페이지로 리디렉션합니다.
                _navigationManager.NavigateTo("/login");
            }

            return response;
        }
    }
}
