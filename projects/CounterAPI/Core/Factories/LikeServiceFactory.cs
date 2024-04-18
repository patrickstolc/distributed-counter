using counter_api.Core.Services;
using MessageClient.Factory;
using SharedModels;

namespace counter_api.Core.Factories;

public static class LikeServiceFactory
{
    public static LikeService CreateLikeService()
    {
        var easyNetQFactory = new EasyNetQFactory();
        var newLikeMessageClient = easyNetQFactory.CreateSendReceiveMessageClient<NewLikeMessage>("new-like");
        var httpClient = new HttpClient();
        var likeServiceHost = Environment.GetEnvironmentVariable("LIKE_SERVICE_HOST");
        if(likeServiceHost == null)
            throw new InvalidOperationException("LIKE_SERVICE_HOST environment variable not set");
        
        var failedMessageCache = new FailedMessageCache.FailedMessageCache("like-cache", 6379);
        return new LikeService(newLikeMessageClient, httpClient, likeServiceHost, failedMessageCache);
    }
}