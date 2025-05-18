using System.Diagnostics;
using System.Security.Claims;

namespace CurrencyConverter.Api.Middleware;

/// <summary>
/// Middleware to log HTTP requests and responses.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        var request = context.Request;
        var user = context.User;

        string? clientIp = context.Connection.RemoteIpAddress?.ToString();
        string clientId = user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "anonymous";
        string method = request.Method;
        string path = request.Path;

        try
        {
            await _next(context); // proceed to next middleware
        }
        finally
        {
            stopwatch.Stop();
            int statusCode = context.Response.StatusCode;

            _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms | IP: {IP} | ClientId: {ClientId} | CorrelationId: {CorrelationId}",
                method, path, statusCode, stopwatch.ElapsedMilliseconds, clientIp, clientId, correlationId);
        }
    }
}

