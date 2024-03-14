using CounterService.Core.Repositories;
using CounterService.Core.Services;
using StackExchange.Redis;

namespace CounterService.Core.Factories;

public static class LikeAggregationServiceFactory
{
    public static LikeAggregationService CreateLikeAggregationService()
    {
        var redisHostname = Environment.GetEnvironmentVariable("REDIS_HOSTNAME");
        if (redisHostname == null)
        {
            throw new Exception("REDIS_HOSTNAME environment variable not set");
        }
        
        Console.WriteLine($"Trying to connect to Redis at {redisHostname}");
        
        var redisConnection = ConnectionMultiplexer.Connect(redisHostname);
        var countCacheRepository = new LikeCountCacheRepository(redisConnection);
        return new LikeAggregationService(countCacheRepository);
    }
}