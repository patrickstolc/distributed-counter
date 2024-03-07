using CounterService.Core.Helpers;
using Repository;
using SharedModels;

namespace CounterService.Core.Repositories;

public class LikeRepository: EntityFrameworkRepository<Like>
{
    public LikeRepository(LikeDataContext context) : base(context) { }
}