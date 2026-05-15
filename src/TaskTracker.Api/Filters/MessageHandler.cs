using System.Diagnostics;

namespace TaskTracker.Api.Filters;

public sealed class MessageHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(RequestDelegate next, ILogger<MessageHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms.",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
