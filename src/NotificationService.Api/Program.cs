using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1️⃣  Add services to the container
// ---------------------------------------------------------------------

// Enable controllers (for attribute routing)
builder.Services.AddControllers();

// Add OpenAPI (Swagger)
builder.Services.AddOpenApi();

// Register repositories (infrastructure layer)
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IBusinessVerificationRepository, BusinessVerificationRepository>();
builder.Services.AddScoped<IPasswordResetRequestRepository, PasswordResetRequestRepository>();

// Register application services (application layer)
builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IOtpFunctionHandler, OtpFunctionHandler>();

// Register Azure Queue Service
builder.Services.AddScoped<IQueueService, AzureQueueService>();

// Optional: CORS (if calling from frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// ---------------------------------------------------------------------
// 2️⃣  Configure the HTTP request pipeline
// ---------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI
    app.MapOpenApi(); // new .NET 9 style
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Notification Service API v1");
        options.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

// Optional: Global exception handler middleware (recommended)
// app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Enable attribute-routed controllers
app.MapControllers();

app.Run();
