using backend.Models;
using Microsoft.EntityFrameworkCore;
using Services;
using API;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization(); // ← これが必要


// DbContext
builder.Services.AddDbContext<AuthRDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IJwtService>(sp =>
{
    var config = builder.Configuration;
    return new JwtService(
        config["Jwt:Secret"] ?? "default_secret",
        config["Jwt:Issuer"] ?? "MyApp",
        config["Jwt:Audience"] ?? "MyAppUsers"
    );
});

// CORS設定の追加
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000") // Nuxtからのリクエスト許可
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// API クラスの登録
builder.Services.AddScoped<PostRegisterUser>();
builder.Services.AddScoped<PostToken>();
builder.Services.AddScoped<PostTrainingRecord>();
builder.Services.AddScoped<GetTrainingHistory>();
builder.Services.AddScoped<GetTrainingMenus>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await WaitForDatabaseAsync(app.Services);

static async Task WaitForDatabaseAsync(IServiceProvider services, int retryCount = 10)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    for (var i = 0; i < retryCount; i++)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthRDBContext>();
            await db.Database.CanConnectAsync();
            logger.LogInformation("Database connection established.");
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Database not ready, retrying... ({i + 1}/{retryCount})");
            await Task.Delay(2000);
        }
    }

    throw new Exception("Database connection failed after retries.");
}


app.UseSwagger();
app.UseSwaggerUI();

// CORSを有効にする（←順番重要：UseRoutingの前後にしない）
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthorization();

// API マッピング
app.MapPost("/api/auth/register", (
    [FromServices] PostRegisterUser handler,
    [FromServices] AuthRDBContext db,
    [FromServices] IAuthService auth,
    [FromBody] PostRegisterUser.PostRegisterUserRequest req
) => handler.Run(db, auth, req));

app.MapPost("/api/auth/token", (
    [FromServices] PostToken handler,
    [FromServices] AuthRDBContext db,
    [FromServices] IJwtService jwt,
    [FromBody] PostToken.PostTokenRequest req
) => handler.Run(db, jwt, req));

app.MapPost("/api/training/record", (
    [FromServices] PostTrainingRecord handler,
    [FromServices] AuthRDBContext db,
    [FromServices] IJwtService jwt,
    [FromHeader(Name = "Authorization")] string token,
    [FromBody] PostTrainingRecord.PostTrainingRecordRequest req
) => handler.Run(db, jwt, token, req));

app.MapGet("/api/training/history", (
    [FromServices] GetTrainingHistory handler,
    [FromServices] AuthRDBContext db,
    [FromServices] IJwtService jwt,
    [FromHeader(Name = "Authorization")] string token,
    [FromQuery] DateOnly? startDate,
    [FromQuery] DateOnly? endDate
) => handler.Run(db, jwt, token, startDate, endDate));

app.MapGet("/api/training/menus", (
    [FromServices] GetTrainingMenus handler,
    [FromServices] AuthRDBContext db,
    [FromServices] IJwtService jwt,
    [FromHeader(Name = "Authorization")] string token
) => handler.Run(db));

app.Run();
