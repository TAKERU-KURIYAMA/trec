# ビルドステージ
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["api/Api.csproj", "api/"]
RUN dotnet restore "api/Api.csproj"

COPY ./api/ ./api/
WORKDIR /src/api
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4
WORKDIR /home/site/wwwroot

# アプリ本体のビルド成果物
COPY --from=build /app/publish .

# 🔽 必要な設定ファイルと bin ディレクトリを追加でコピー
COPY ./api/host.json .
COPY ./api/local.settings.json .
COPY ./api/bin ./bin