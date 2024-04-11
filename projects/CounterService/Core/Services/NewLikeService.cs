using SharedModels;
using MessageClient;
using Polly;
using StackExchange.Redis;

namespace CounterService.Core.Services;

public class NewLikeService
{
    private readonly MessageClient<NewLikeMessage> _newLikeMessageClient;
    private readonly LikeAggregationService _likeAggregationService;
    private readonly LikeService _likeService;
    private readonly Policy _retryPolicy;

    public NewLikeService(MessageClient<NewLikeMessage> newLikeMessageClient, LikeAggregationService likeAggregationService, LikeService likeService)
    {
        _newLikeMessageClient = newLikeMessageClient;
        _likeAggregationService = likeAggregationService;
        _likeService = likeService;
        _retryPolicy = Policy.Handle<RedisConnectionException>().WaitAndRetry(
            5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, timeSpan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} due to {exception.Message}");
            }
        );
    }
    
    public void Start()
    {
        _newLikeMessageClient.ConnectAndListen(OnNewLike);
    }
    
    private void OnNewLike(NewLikeMessage newLikeMessage)
    {
        Console.WriteLine($"Received new like message for post {newLikeMessage.PostId} of type {newLikeMessage.Type}");
        
        // broadcast like update to all connected clients
        _newLikeMessageClient.SendUsingQueue<LikeUpdateMessage>(new LikeUpdateMessage
        {
            Count = newLikeMessage.Count,
            PostId = newLikeMessage.PostId
        }, "like-updates");
        
        // increment or decrement in the like cache
        switch (newLikeMessage.Type)
        {
            case NewLikeType.Like:
                _retryPolicy.Execute(() =>
                {
                    _likeAggregationService.HandleLike(newLikeMessage.PostId, newLikeMessage.UserId,
                        newLikeMessage.TransactionId);
                });
                break;
            case NewLikeType.Dislike:
                _retryPolicy.Execute(() =>
                {
                    _likeAggregationService.HandleDislike(newLikeMessage.PostId, newLikeMessage.UserId,
                        newLikeMessage.TransactionId);
                });
                break;
        }
        
        // write the like update to the database
        _likeService.AddLike(new Like
        {
            PostId = newLikeMessage.PostId,
            Type = newLikeMessage.Type,
            UserId = newLikeMessage.UserId,
            Date = DateTime.Now
        });
    }
}
