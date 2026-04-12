using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using NotificationService.Api.Authentication;
using NotificationService.Api.Middleware;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
var appConfigEndpoint = builder.Configuration["AzureAppConfiguration:Endpoint"];

if (!string.IsNullOrWhiteSpace(appConfigEndpoint))
{
    try
    {
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options
                .Connect(new Uri(appConfigEndpoint), new Azure.Identity.DefaultAzureCredential())
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new Azure.Identity.DefaultAzureCredential());
                });
        });
        Console.WriteLine($"[AppConfig] Connected: {appConfigEndpoint}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[AppConfig] FAILED: {ex.GetType().Name}: {ex.Message}");
    }
}
else
{
    Console.WriteLine("[AppConfig] Endpoint not set — using appsettings.json (local dev)");
}

// -----------------------------------------------------------------------
// 1. Validate required configuration at startup — fail fast, not at runtime
// -----------------------------------------------------------------------
var postgresConnection = builder.Configuration.GetConnectionString("PostgresConnection");
var azureQueueConnection = builder.Configuration.GetConnectionString("AzureQueueStorage");
var apiKey = builder.Configuration["ApiKey"];

if (string.IsNullOrWhiteSpace(postgresConnection))
    throw new InvalidOperationException(
        "PostgresConnection is not configured. Set it via environment variable 'ConnectionStrings__PostgresConnection'.");

if (string.IsNullOrWhiteSpace(azureQueueConnection))
    throw new InvalidOperationException(
        "AzureQueueStorage is not configured. Set it via environment variable 'ConnectionStrings__AzureQueueStorage'.");

if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException(
        "ApiKey is not configured. Set it via environment variable 'ApiKey'.");

// -----------------------------------------------------------------------
// 2. Controllers
// -----------------------------------------------------------------------
builder.Services.AddControllers();

// -----------------------------------------------------------------------
// 3. OpenAPI / Swagger
// -----------------------------------------------------------------------
builder.Services.AddOpenApi();

// -----------------------------------------------------------------------
// 4. Authentication — API key scheme
// -----------------------------------------------------------------------
builder.Services
    .AddAuthentication(ApiKeyAuthenticationConstants.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationConstants.SchemeName,
        _ => { });

builder.Services.AddAuthorization();

// -----------------------------------------------------------------------
// 5. Rate limiting
// -----------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    // General API limit: 120 requests / minute per IP
    options.AddSlidingWindowLimiter("general", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.PermitLimit = 120;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Stricter OTP limit: 10 requests / minute per IP — brute-force mitigation
    options.AddSlidingWindowLimiter("otp", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, _) =>
    {
        ctx.HttpContext.Response.Headers["Retry-After"] = "60";
        await ctx.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests. Please retry after 60 seconds." });
    };
});

// -----------------------------------------------------------------------
// 6. CORS — explicit allow-list; defaults to deny-all in production
// -----------------------------------------------------------------------
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedCors", policy =>
    {
        if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .WithMethods("GET", "POST", "DELETE")
                  .WithHeaders("Content-Type", ApiKeyAuthenticationConstants.HeaderName);
        }
        // If production and no origins configured — no origin is allowed (secure by default)
    });
});

// -----------------------------------------------------------------------
// 7. Health checks
// -----------------------------------------------------------------------
builder.Services.AddHealthChecks();

// -----------------------------------------------------------------------
// 8. Repositories and services (DI)
// -----------------------------------------------------------------------
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IBusinessVerificationRepository, BusinessVerificationRepository>();
builder.Services.AddScoped<IPasswordResetRequestRepository, PasswordResetRequestRepository>();

builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IOtpFunctionHandler, OtpFunctionHandler>();
builder.Services.AddScoped<IQueueService, AzureQueueService>();

// -----------------------------------------------------------------------
// 9. Build
// -----------------------------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------------------------
// 10. HTTP request pipeline
// -----------------------------------------------------------------------

// Security headers — applied to every response
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

// Global exception handler — must be first in pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Notification Service API v1");
        options.RoutePrefix = string.Empty;
    });
}
else
{
    // Enforce HTTPS with HSTS in production
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("RestrictedCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

// -----------------------------------------------------------------------
// Constants — kept here to avoid a separate tiny file
// -----------------------------------------------------------------------
public static class ApiKeyAuthenticationConstants
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";
}
