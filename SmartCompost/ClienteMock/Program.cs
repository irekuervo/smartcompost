
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();
app.UseMiddleware<LoggingMiddleware>();
app.UseRouting();
app.MapControllers();
app.Run();