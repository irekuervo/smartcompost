using MockSmartcompost.Utils;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;

    }

    public async Task InvokeAsync(HttpContext context)
    {
        AppLogger.Log($"{DateTime.Now} | {context.Request.Method} | {context.Request.Path}");

        await _next(context);
    }
}