using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Domain.Repositories;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1️⃣  Add services to the container
// ---------------------------------------------------------------------

// Enable controllers (for attribute routing)
builder.Services.AddControllers();

// Add OpenAPI (Swagger)
builder.Services.AddOpenApi();

// Register Dapper context (singleton since it only manages connection strings)
// builder.Services.AddSingleton<DapperContext>();

// Register repositories (infrastructure layer)
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Register application services (application layer)
builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();

// Configure SMTP settings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Configure SMS settings
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));

// Register Template Engine (singleton with templates path from configuration)
var templatesPath = builder.Configuration.GetValue<string>("TemplatesPath") ?? "Templates";
var fullTemplatesPath = Path.Combine(builder.Environment.ContentRootPath, templatesPath);
builder.Services.AddSingleton<ITemplateEngine>(sp => new TemplateEngine(fullTemplatesPath));

// Register Email service
builder.Services.AddScoped<IEmailService, EmailService>();

// Register HttpClient for SMS service
builder.Services.AddHttpClient<ISmsService, SmsService>();

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
