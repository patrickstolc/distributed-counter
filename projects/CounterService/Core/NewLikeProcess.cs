using CounterService.Core.Factories;
using CounterService.Core.Services;

namespace CounterService.Core;

public class NewLikeProcess: BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("New like process is running..");

        var newLikeService = NewLikeServiceFactory.CreateNewLikeService();
        newLikeService.Start();
        
        while(!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
        Console.WriteLine("New like process is stopping..");
    }
}