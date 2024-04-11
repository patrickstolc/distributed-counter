using counter_api.Core.Services;
using MessageClient;
using MessageClient.Drivers.EasyNetQ;
using MessageClient.Drivers.EasyNetQ.MessagingStrategies;
using SharedModels;

namespace CounterService.Core.Helpers;

public class FailureRecoveryProcess: BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Failure recovery process is running..");

        FailureRecoveryService failureRecoveryService = new FailureRecoveryService(
            new FailedMessageCache("like-cache", 6379),
            new MessageClient<NewLikeMessage>(
                new EasyNetQDriver<NewLikeMessage>(
                    "host=rabbitmq;port=5672;virtualHost=/;username=guest;password=guest",
                    new SendReceiveStrategy("new-like")
                )
            )
        );
        
        while(!stoppingToken.IsCancellationRequested)
        {
            bool canRecover = failureRecoveryService.CanIRecover();
            Console.WriteLine("Failure recovery process is running..");
            Console.WriteLine($"Can recover: {canRecover}");
            
            if (canRecover)
            {
                Console.WriteLine("Trying to recover failed messages..");
                try
                {
                    failureRecoveryService.Recover();
                }
                catch
                {
                    
                }
            }
            await Task.Delay(10000, stoppingToken);
        }
        
        Console.WriteLine("Failure recovery process is stopping..");
    }
}