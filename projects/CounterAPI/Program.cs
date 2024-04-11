using System.Threading.RateLimiting;
using counter_api.Core;
using counter_api.Core.Factories;
using counter_api.Core.Services;
using CounterService.Core.Helpers;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddMemoryCache();
builder.Services.AddHttpLogging(o => { });
builder.Services.AddSingleton(LikeServiceFactory.CreateLikeService().Start());
builder.Services.AddControllers();
builder.Services.AddHostedService<FailureRecoveryProcess>();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("likeRateLimitPerIpAddress", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress.ToString(),
        factory: partition => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 3,
            QueueLimit = 0,
            Window = TimeSpan.FromSeconds(5)
        }
    ));
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try later again...", cancellationToken: token);
    };
});

var app = builder.Build();
app.UseCors(
    corsBuilder => corsBuilder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()
);
app.UseRateLimiter();
app.UseHttpLogging();
app.UseAuthorization();
app.MapControllers();

app.Run();
