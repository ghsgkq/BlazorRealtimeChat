# 1. 빌드 스테이지
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 솔루션 및 프로젝트 파일 복사 (캐시 최적화)
COPY ["BlazorRealtimeChat.sln", "./"]
COPY ["BlazorRealtimeChat/BlazorRealtimeChat/BlazorRealtimeChat.csproj", "BlazorRealtimeChat/BlazorRealtimeChat/"]
COPY ["BlazorRealtimeChat/BlazorRealtimeChat.Client/BlazorRealtimeChat.Client.csproj", "BlazorRealtimeChat/BlazorRealtimeChat.Client/"]
COPY ["BlazorRealtimeChat/BlazorRealtimeChat.Shared/BlazorRealtimeChat.Shared.csproj", "BlazorRealtimeChat/BlazorRealtimeChat.Shared/"]

# 패키지 복원
RUN dotnet restore

# 전체 소스 복사 및 빌드
COPY . .
WORKDIR "/src/BlazorRealtimeChat/BlazorRealtimeChat"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# 2. 실행 스테이지 (가벼운 런타임 이미지 사용)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
# 컨테이너 실행 시 서버 앱 시작
ENTRYPOINT ["dotnet", "BlazorRealtimeChat.dll"]