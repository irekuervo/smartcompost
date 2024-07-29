var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
});

var app = builder.Build();
app.UseMiddleware<LoggingMiddleware>();
app.UseRouting();
app.MapControllers();
app.Run();