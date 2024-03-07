using Notifications.Core;
using SharedModels;
using MessageClient;
using MessageClient.Factory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MessageClient<LikeUpdateMessage>>(provider => 
    new EasyNetQFactory().CreateSendReceiveMessageClient<LikeUpdateMessage>("like-updates")    
);
builder.Services.AddSingleton<MessagingService>(
    provider => new MessagingService(provider.GetRequiredService<MessageClient<LikeUpdateMessage>>())
);
builder.Services.AddTransient<ServerSentEventsAdapter>(
    serverSentEventsAdapter =>
        new ServerSentEventsAdapter(serverSentEventsAdapter.GetRequiredService<MessagingService>())
            .Start());
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("X-Connection-Id"));
});

var app = builder.Build();
app.UseCors("AllowAnyOrigin");
app.Map("sse", async (HttpContext context, CancellationToken ct, ServerSentEventsAdapter service, string name) =>
    await service.HandleServerSentEventsRequest(context, ct, name));

app.Run();