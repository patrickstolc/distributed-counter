using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedModels;

namespace FailedMessageConsumer;

public static class Program
{
    public static async Task Main()
    {
        // create a new failed message service
        var failedMessageService = new FailedMessageService<NewLikeMessage>(new Config
        {
            Hostname = "rabbitmq",
            Port = 5672,
            CacheHostname = "like-cache",
            CachePort = 6379,
            MessageTypeName = "new-like",
            MessageProtocol = MessageProtocol.QUEUE
        });
        
        // create a new host for the background service
        var host = Host.CreateDefaultBuilder().ConfigureServices(
            services =>
            {
                services.AddHostedService(
                    provider => new ResendService<NewLikeMessage>(
                        failedMessageService, 
                        provider.GetRequiredService<IHostApplicationLifetime>()
                    )
                );
            }
        );
        
        await host.RunConsoleAsync();
    }
}