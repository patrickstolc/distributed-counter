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
            await Task.Delay(10000, stoppingToken);
            
            bool canRecover = failureRecoveryService.CanIRecover();
            Console.WriteLine("Failure recovery process is running..");

            if (!canRecover)
            {
                Console.WriteLine("Cannot recover, trying again later..");
                continue;
            }
            
            if(!failureRecoveryService.AnyFailedMessages())
            {
                Console.WriteLine("No failed messages to recover..");
                continue;
            }

            try
            {
                Console.WriteLine("Recovering failed messages..");
                failureRecoveryService.Recover();
                Console.WriteLine("Failed messages recovered..");
            } catch (Exception e)
            {
                Console.WriteLine($"Failed to recover messages: {e.Message}");
            }
        }
        
        Console.WriteLine("Failure recovery process is stopping..");
    }
}