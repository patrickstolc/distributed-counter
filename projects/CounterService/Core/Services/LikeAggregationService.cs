using CounterService.Core.Repositories;
using Polly;
using StackExchange.Redis;

namespace CounterService.Core.Services;

public class LikeAggregationService
{
    private readonly LikeCountCacheRepository _likeCountCacheRepository;
    private readonly LikeService _likeService;
    private readonly Policy<int> _fallbackPolicy;
    
    public LikeAggregationService(LikeCountCacheRepository likeCountCacheRepository, LikeService likeService)
    {
        _likeCountCacheRepository = likeCountCacheRepository;
        _likeService = likeService;
        _fallbackPolicy = Policy<int>.Handle<RedisConnectionException>().Fallback(
            (ctx) =>
            {
                Console.WriteLine("Fallback policy triggered");
                var postId = (int) ctx["postId"];
                return GetLikeCountFromDatabase(postId);
            },
            (_, __) => { }
        );
    }

    public int GetLikeCountFromDatabase(int postId)
    {
        return _likeService.LikeCount(postId);
    }
    
    public bool GetUserLike(int postId, int userId)
    {
        return _likeCountCacheRepository.GetUserLike(postId, userId);
    }
    
    public int GetLikeCount(int postId)
    {
        Context policyContext = new Context();
        policyContext["postId"] = postId;

        return _fallbackPolicy.Execute((_) =>
        {
            return _likeCountCacheRepository.GetLikeCount(postId);
        }, policyContext);
    }

    public bool LikeHasBeenProcessed(string transactionId)
    {
        return _likeCountCacheRepository.TransactionHasBeenProcessed(transactionId);
    }

    public void HandleLike(int postId, int userId, string transactionId)
    {
        if (LikeHasBeenProcessed(transactionId))
        {
            Console.WriteLine($"Transaction with ID: {transactionId} has already been processed. Skipping...");
            return;
        }
        
        Console.WriteLine($"Handling like for post {postId} by user {userId}");
        IncrementLikeCount(postId, transactionId);
        Console.WriteLine($"Adding like for user {userId} to post {postId}");
        AddUserLike(userId, postId);
        Console.WriteLine($"Like count for post {postId}: {_likeCountCacheRepository.GetLikeCount(postId)}");
    }
    
    public void HandleDislike(int postId, int userId, string transactionId)
    {
        if (LikeHasBeenProcessed(transactionId))
        {
            Console.WriteLine($"Transaction with ID: {transactionId} has already been processed. Skipping...");
            return;
        }
        
        Console.WriteLine($"Handling dislike for post {postId} by user {userId}");
        DecrementLikeCount(postId, transactionId);
        Console.WriteLine($"Removing like for user {userId} from post {postId}");
        RemoveUserLike(userId, postId);
        Console.WriteLine($"Like count for post {postId}: {_likeCountCacheRepository.GetLikeCount(postId)}");
    }

    private void AddUserLike(int userId, int postId)
    {
        Console.WriteLine($"Adding like for user {userId} to post {postId}");
        _likeCountCacheRepository.AddUserLike(userId, postId);
    }

    private void RemoveUserLike(int userId, int postId)
    {
        Console.WriteLine($"Removing like for user {userId} from post {postId}");
        _likeCountCacheRepository.RemoveUserLike(userId, postId);
    }

    private void IncrementLikeCount(int postId, string transactionId)
    {
        Console.WriteLine($"Incrementing like count for post {postId}");
        _likeCountCacheRepository.IncrementLikeCount(postId, transactionId);
    }

    private void DecrementLikeCount(int postId, string transactionId)
    {
        Console.WriteLine($"Decrementing like count for post {postId}");
        _likeCountCacheRepository.DecrementLikeCount(postId, transactionId);
    }
}