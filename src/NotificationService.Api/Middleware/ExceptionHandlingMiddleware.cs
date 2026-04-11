using NotificationService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace NotificationService.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, message) = exception switch
        {
            NotificationNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            OtpNotFoundException          => (HttpStatusCode.NotFound, exception.Message),
            OtpExpiredException           => (HttpStatusCode.Gone, exception.Message),
            _                             => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}