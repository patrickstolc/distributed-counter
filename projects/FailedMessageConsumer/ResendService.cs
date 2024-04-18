using Microsoft.Extensions.Hosting;

namespace FailedMessageConsumer;

public class ResendService<T> : BackgroundService
{
    private readonly FailedMessageService<T> _failedMessageService;
    
    public ResendService(FailedMessageService<T> failedMessageService, IHostApplicationLifetime applicationLifetime)
    {
        _failedMessageService = failedMessageService;
        applicationLifetime.ApplicationStarted.Register(OnStarted);
        applicationLifetime.ApplicationStopping.Register(OnStopping);
        applicationLifetime.ApplicationStopped.Register(OnStopped);
    }
    
    private void OnStarted()
    {
        Console.WriteLine("Resend service started..");
    }
    
    private void OnStopping()
    {
        Console.WriteLine("Resend service stopping..");
    }
    
    private void OnStopped()
    {
        Console.WriteLine("Resend service stopped..");
    }
    
    private async Task ResendFailedMessage()
    {
        // check if there are any failed messages
        if(!_failedMessageService.AnyFailedMessages())
            return;
        
        // check if we can recover failed messages
        if(!_failedMessageService.CanIRecover())
            return;
        
        try
        {   
            // try to resend failed messages
            Console.WriteLine("Recovering failed messages..");
            _failedMessageService.Recover();
        } catch (Exception e)
        {
            Console.WriteLine($"Failed to recover messages: {e}");
            return;
        }
        Console.WriteLine("Failed messages recovered..");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Resend service running..");
            await ResendFailedMessage();
            await Task.Delay(10000, stoppingToken);
        }
    }
}