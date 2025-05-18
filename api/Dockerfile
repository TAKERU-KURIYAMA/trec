# ビルドステージ
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["Api.csproj", "./"]
RUN dotnet restore "./Api.csproj"

COPY . .
RUN dotnet publish "./Api.csproj" -c Release -o /app/publish

# 実行ステージ
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8

WORKDIR /home/site/wwwroot
COPY --from=build /app/publish .

# 環境変数（開発用ログ出力）
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
