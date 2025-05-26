using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Api.Models;
using Services;
using Microsoft.EntityFrameworkCore;

try
{
    Console.WriteLine("=== 起動開始 ===");

    var host = Host.CreateDefaultBuilder()
        .ConfigureFunctionsWorkerDefaults()
        .ConfigureAppConfiguration(config =>
        {
            config.AddEnvironmentVariables();
        })
        .ConfigureServices((context, services) =>
        {
            var config = context.Configuration;

            Console.WriteLine("JWT__SECRET: " + config["JWT__SECRET"]);
            Console.WriteLine("JWT__ISSUER: " + config["JWT__ISSUER"]);
            Console.WriteLine("JWT__AUDIENCE: " + config["JWT__AUDIENCE"]);

            var jwtSecret = config["JWT__SECRET"];
            var jwtIssuer = config["JWT__ISSUER"];
            var jwtAudience = config["JWT__AUDIENCE"];

            if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                throw new InvalidOperationException("JWT設定値が不足しています");

            services.AddSingleton<IJwtService>(new JwtService(jwtSecret, jwtIssuer, jwtAudience));

            var connectionString = config["ConnectionStrings:DefaultConnection"];
            Console.WriteLine("接続文字列: " + connectionString);
            services.AddDbContext<MessageRDBContext>(options => options.UseSqlServer(connectionString));
        })
        .Build();

    Console.WriteLine("=== 起動実行 ===");
    host.Run();
}
catch (Exception ex)
{
    Console.WriteLine("=== 起動時例外 ===");
    Console.WriteLine(ex.ToString());
    throw;
}
