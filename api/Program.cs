using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Api.Models;
using Services;
using Common.Shared.Constants;
using Common.Shared.Exceptions;
using Common.Shared.Utilities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

try
{
    // ====================================
    // ログ設定
    // ====================================
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    
    // 本番環境ではログレベルを制限
    if (builder.Environment.IsProduction())
    {
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
    }

    var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
    logger.LogInformation("=== アプリケーション起動開始 ===");

    // ====================================
    // 基本サービス設定
    // ====================================
    builder.Services.AddControllers(options =>
    {
        // モデルバリデーション失敗時のカスタムレスポンス
        options.ModelValidatorProviders.Clear();
    });
    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(); // 開発環境用

    // ====================================
    // JWT認証設定（セキュリティ強化）
    // ====================================
    var jwtSection = builder.Configuration.GetSection("JWT");
    var jwtSecret = jwtSection["SECRET"];
    var jwtIssuer = jwtSection["ISSUER"];
    var jwtAudience = jwtSection["AUDIENCE"];

    // セキュリティ：機密情報をログに出力しない
    logger.LogInformation("JWT設定読み込み完了 - Issuer: {Issuer}, Audience: {Audience}", jwtIssuer, jwtAudience);

    // JWT設定値の検証（共通ライブラリを使用）
    if (string.IsNullOrWhiteSpace(jwtSecret) || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
    {
        throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "JWT設定値が不足しています");
    }

    try
    {
        JwtHelper.ValidateJwtConfiguration(jwtSecret, jwtIssuer, jwtAudience);
    }
    catch (AppException ex)
    {
        logger.LogError("JWT設定検証失敗: {Message}", ex.ErrorInfo.Message);
        throw;
    }

    // JWT認証ミドルウェアの設定
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = JwtHelper.CreateValidationParameters(jwtSecret, jwtIssuer, jwtAudience);
            
            // イベントハンドラーでカスタムログ出力
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    logger.LogWarning("JWT認証失敗: {Exception}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    logger.LogDebug("JWT認証チャレンジ: {Error}, {Description}", context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    // JwtServiceのDI設定（改善されたコンストラクタ対応）
    builder.Services.AddScoped<IJwtService>(serviceProvider =>
    {
        var jwtLogger = serviceProvider.GetRequiredService<ILogger<JwtService>>();
        return new JwtService(jwtSecret, jwtIssuer, jwtAudience, jwtLogger);
    });

    // ====================================
    // データベース設定
    // ====================================
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "データベース接続文字列が設定されていません");
    }

    // セキュリティ：接続文字列の詳細をログに出力しない（サーバー名のみ）
    var serverName = ExtractServerName(connectionString);
    logger.LogInformation("データベース接続設定完了 - Server: {ServerName}", serverName);

    builder.Services.AddDbContext<MessageRDBContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            // 接続の復旧力を向上
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: ApplicationConstants.RetrySettings.DatabaseRetryCount,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });

        // 開発環境でのみ詳細ログを有効化
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // ====================================
    // その他のサービス設定
    // ====================================
    builder.Services.AddScoped<IAuthService, AuthService>();

    // ====================================
    // CORS設定（セキュリティ強化）
    // ====================================
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    
    builder.Services.AddCors(options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // 開発環境：制限を緩和
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
            logger.LogWarning("開発環境: CORS制限を緩和しています");
        }
        else
        {
            // 本番環境：厳密なCORS設定
            options.AddDefaultPolicy(policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                      .WithHeaders("Content-Type", "Authorization")
                      .AllowCredentials();
            });
            logger.LogInformation("本番環境: CORS制限を有効化 - 許可オリジン数: {Count}", allowedOrigins.Length);
        }
    });

    // ====================================
    // アプリケーション構築
    // ====================================
    var app = builder.Build();

    logger.LogInformation("アプリケーション構築完了");

    // ====================================
    // ミドルウェア設定
    // ====================================
    
    // 開発環境でのみSwagger有効化
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        logger.LogInformation("Swagger UI 有効化: /swagger");
    }

    // セキュリティヘッダー追加
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        if (!app.Environment.IsDevelopment())
        {
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        await next();
    });

    // CORS（認証より前に配置）
    app.UseCors();

    // 認証・認可
    app.UseAuthentication();
    app.UseAuthorization();

    // ルーティング
    app.UseRouting();

    // グローバル例外ハンドリング
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (exceptionFeature?.Error != null)
            {
                var requestLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                
                if (exceptionFeature.Error is AppException appEx)
                {
                    StructuredLogger.LogError(requestLogger, appEx, context.Request);
                    var response = HttpResponseHelper.CreateErrorResponse(appEx);
                    context.Response.StatusCode = (int)appEx.ErrorInfo.StatusCode;
                    context.Response.ContentType = ApplicationConstants.ContentTypes.Json;
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response.Value));
                }
                else
                {
                    StructuredLogger.LogUnhandledException(requestLogger, exceptionFeature.Error, context.Request, "Global exception handler");
                    var response = HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.ServerError);
                    context.Response.StatusCode = (int)ApplicationConstants.ErrorCodes.ServerError.StatusCode;
                    context.Response.ContentType = ApplicationConstants.ContentTypes.Json;
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response.Value));
                }
            }
        });
    });

    // エンドポイントマッピング
    app.MapControllers();

    // ====================================
    // データベース初期化（開発環境のみ）
    // ====================================
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MessageRDBContext>();
        
        try
        {
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("データベース初期化確認完了");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "データベース初期化警告");
        }
    }

    logger.LogInformation("=== ASP.NET Core Web API 起動完了 ===");
    logger.LogInformation("環境: {Environment}", app.Environment.EnvironmentName);
    logger.LogInformation("URL: {Urls}", string.Join(", ", app.Urls));

    await app.RunAsync();
}
catch (AppException ex)
{
    // アプリケーション固有の例外
    Console.Error.WriteLine($"起動失敗 - {ex.ErrorInfo.Code}: {ex.ErrorInfo.Message}");
    Environment.Exit(1);
}
catch (Exception ex)
{
    // 予期しない例外
    Console.Error.WriteLine($"予期しないエラーで起動失敗: {ex.Message}");
    Console.Error.WriteLine($"詳細: {ex}");
    Environment.Exit(1);
}

/// <summary>
/// 接続文字列からサーバー名を抽出（ログ出力用）
/// 機密情報を含まない部分のみを返す
/// </summary>
/// <param name="connectionString">接続文字列</param>
/// <returns>サーバー名、抽出できない場合は"Unknown"</returns>
static string ExtractServerName(string connectionString)
{
    try
    {
        var serverKeywords = new[] { "Server=", "Data Source=", "server=", "data source=" };
        
        foreach (var keyword in serverKeywords)
        {
            var index = connectionString.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var start = index + keyword.Length;
                var end = connectionString.IndexOf(';', start);
                if (end > start)
                {
                    return connectionString.Substring(start, end - start);
                }
            }
        }
        
        return "Unknown";
    }
    catch
    {
        return "Unknown";
    }
}