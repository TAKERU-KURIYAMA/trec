# ビルドステージ
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["api/Api.csproj", "api/"]
COPY ["api/efpt.config.json", "api/"]
RUN dotnet restore "api/Api.csproj"

COPY ./api/ ./api/
WORKDIR /src/api
RUN dotnet build "Api.csproj" -c Release -o /app/build
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ランタイムステージ - 標準的な ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "Api.dll"]