using CounterService.Core.Services;
using SharedModels;
using MessageClient.Factory;

namespace CounterService.Core.Factories;

public static class NewLikeServiceFactory
{
    public static NewLikeService CreateNewLikeService()
    {
        Console.WriteLine("Creating new like service..");
        var easyNetQFactory = new EasyNetQFactory();
        var newLikeMessageClient = easyNetQFactory.CreateSendReceiveMessageClient<NewLikeMessage>("new-like");
        var likeAggregationService = LikeAggregationServiceFactory.CreateLikeAggregationService();
        var likeService = LikeServiceFactory.CreateLikeService();
        
        return new NewLikeService(newLikeMessageClient, likeAggregationService, likeService);
    }
}