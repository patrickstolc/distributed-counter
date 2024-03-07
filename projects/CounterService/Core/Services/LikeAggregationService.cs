using CounterService.Core.Repositories;

namespace CounterService.Core.Services;

public class LikeAggregationService
{
    private readonly LikeCountCacheRepository _likeCountCacheRepository;
    
    public LikeAggregationService(LikeCountCacheRepository likeCountCacheRepository)
    {
        _likeCountCacheRepository = likeCountCacheRepository;
    }
    
    public bool GetUserLike(int postId, int userId)
    {
        return _likeCountCacheRepository.GetUserLike(postId, userId);
    }
    
    public int GetLikeCount(int postId)
    {
        return _likeCountCacheRepository.GetLikeCount(postId);
    }

    public void HandleLike(int postId, int userId)
    {
        Console.WriteLine($"Handling like for post {postId} by user {userId}");
        IncrementLikeCount(postId);
        Console.WriteLine($"Adding like for user {userId} to post {postId}");
        AddUserLike(userId, postId);
        Console.WriteLine($"Like count for post {postId}: {_likeCountCacheRepository.GetLikeCount(postId)}");
    }
    
    public void HandleDislike(int postId, int userId)
    {
        Console.WriteLine($"Handling dislike for post {postId} by user {userId}");
        DecrementLikeCount(postId);
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

    private void IncrementLikeCount(int postId)
    {
        Console.WriteLine($"Incrementing like count for post {postId}");
        _likeCountCacheRepository.IncrementLikeCount(postId);
    }

    private void DecrementLikeCount(int postId)
    {
        Console.WriteLine($"Decrementing like count for post {postId}");
        _likeCountCacheRepository.DecrementLikeCount(postId);
    }
}