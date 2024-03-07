using CounterService.Core.Helpers;
using CounterService.Core.Repositories;
using CounterService.Core.Services;

namespace CounterService.Core.Factories;

public static class LikeServiceFactory
{
    public static LikeService CreateLikeService()
    {
        var likeDataContext = new LikeDataContext();
        var likeRepository = new LikeRepository(likeDataContext);
        return new LikeService(likeRepository);
    }
}