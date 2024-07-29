var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


var app = builder.Build();
app.UseMiddleware<LoggingMiddleware>();
app.UseRouting();
app.MapControllers();
app.Run();