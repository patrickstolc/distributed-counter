using CounterService.Core.Repositories;
using SharedModels;

namespace CounterService.Core.Services;

public class LikeService
{
    private readonly LikeRepository _likeRepository;
    
    public LikeService(LikeRepository likeRepository)
    {
        _likeRepository = likeRepository;
    }

    public void AddLike(Like like)
    {
        _likeRepository.Add(like);
    }
    
    public int LikeCount(int postId)
    {
        return GetLikes(postId).Count();
    }
    
    public IEnumerable<Like> GetLikes(int postId)
    {
        return _likeRepository.Find(
            like => like.PostId == postId
        );
    }
}