using AiChat.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace AiChat.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorType, message) = exception switch
        {
            DomainException => (HttpStatusCode.BadRequest, "domain_error", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "auth_error", exception.Message),
            InvalidOperationException when exception.Message.Contains("already exists") =>
                (HttpStatusCode.Conflict, "conflict", exception.Message),
            InvalidOperationException when exception.Message.Contains("Invalid") =>
                (HttpStatusCode.BadRequest, "validation_error", exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, "validation_error", exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "not_found", "Resource not found"),
            _ => (HttpStatusCode.InternalServerError, "internal_error", "An internal error occurred")
        };

        context.Response.StatusCode = (int)statusCode;

        // 仅在开发环境返回详细错误
        var response = new
        {
            success = false,
            error = new
            {
                type = errorType,
                message = message,
                traceId = context.TraceIdentifier,
                stackTrace = _env.IsDevelopment() ? exception.StackTrace : null
            },
            timestamp = DateTime.UtcNow
        };

        // 记录错误日志
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}. TraceId: {TraceId}", message, context.TraceIdentifier);
        }

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
