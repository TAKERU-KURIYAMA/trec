FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ソースコピー
COPY ["Api/Api.csproj", "Api/"]
RUN dotnet restore "Api/Api.csproj"
COPY . .

WORKDIR "/src/Api"
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

# 実行イメージ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Azure Functions の起動コマンド
ENTRYPOINT ["dotnet", "Api.dll"]
