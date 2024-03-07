using CounterService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SharedModels;

namespace CounterService.Controllers;

[ApiController]
[Route("like")]
public class CounterServiceController : ControllerBase
{
    private readonly LikeAggregationService _likeAggregationService;

    public CounterServiceController(LikeAggregationService likeAggregationService)
    {
        _likeAggregationService = likeAggregationService;
    }
    
    [HttpGet("{postId}/{userId}", Name = "GetUserLike")]
    public bool GetUserLike(int postId, int userId)
    {
        return _likeAggregationService.GetUserLike(postId, userId);
    }

    [HttpGet("{postId}", Name = "GetLikes")]
    public LikeCountServiceResponse GetLikes(int postId)
    {
        var likeCount = _likeAggregationService.GetLikeCount(postId);
        return new LikeCountServiceResponse
        {
            Count = likeCount,
            PostId = postId,
            Date = DateTime.Now
        };
    }
}
