using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Api.Models; // DbContext の名前空間
using Services;        // DIするサービスの名前空間

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(config =>
    {
        config.AddEnvironmentVariables(); // 環境変数を読み込む
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // 接続文字列の読み込み（環境変数: ConnectionStrings__DefaultConnection）
        var connectionString = configuration["ConnectionStrings:DefaultConnection"];
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("接続文字列が設定されていません。環境変数 'ConnectionStrings__DefaultConnection' を確認してください。");
        }

        // DbContext 登録
        services.AddDbContext<MessageRDBContext>(options =>
            options.UseSqlServer(connectionString));

        // 他のサービス登録
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IJwtService, JwtService>();
    })
    .Build();

host.Run();
