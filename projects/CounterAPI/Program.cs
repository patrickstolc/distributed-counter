using counter_api.Core.Factories;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton(LikeServiceFactory.CreateLikeService().Start());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(
    corsBuilder => corsBuilder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()
);
app.UseHttpLogging();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
