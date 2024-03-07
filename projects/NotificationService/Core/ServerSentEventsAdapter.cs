using System.Collections.Concurrent;
using System.Text.Json;
namespace Notifications.Core;

public class ServerSentEventsAdapter : IConnectionAdapter
{
    private readonly ConcurrentQueue<Message> _buffer = new();
    private CancellationTokenSource? _cts;
    private readonly MessagingService _service;

    public ServerSentEventsAdapter(MessagingService service)
    {
        _service = service;
    }
    
    public ServerSentEventsAdapter Start()
    {
        _service.Start();
        return this;
    }
    
    public async Task HandleServerSentEventsRequest(HttpContext context, CancellationToken ct, string name)
    {
        Console.WriteLine($"User '{name}' connected to SSE endpoint.");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var error = await _service.TryAddUser(this, name);
        if (error != null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(error, ct);
            return;
        }
        
        var subscriptionError = await _service.TrySubscribeUserToLikeUpdates(
            "1", name   
        );
        if (subscriptionError != null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(subscriptionError, ct);
            return;
        }
        
        context.Response.Headers.Add("Content-Type", "text/event-stream");
        
        while (!_cts.IsCancellationRequested || !context.RequestAborted.IsCancellationRequested)
        {
            if (_buffer.TryDequeue(out var message))
            {
                await context.Response.WriteAsync($"data: ", ct);
                await JsonSerializer.SerializeAsync(context.Response.Body, message, cancellationToken: ct);
                await context.Response.WriteAsync($"\n\n", ct);
                await context.Response.Body.FlushAsync(ct);
            }
        }
        
        Console.WriteLine($"User '{name}' disconnected from SSE endpoint.");
        _service.RemoveUser(name);
        await CloseConnection("User disconnected");
    }
    
    public Task CloseConnection(string reason)
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    public Task SendMessage(Message message)
    {
        _buffer.Enqueue(message);
        return Task.CompletedTask;
    }

}