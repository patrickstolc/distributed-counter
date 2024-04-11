using counter_api.Core.Services;
using Microsoft.AspNetCore.Mvc;
using counter_api.Models;
using Microsoft.AspNetCore.RateLimiting;
using SharedModels;

namespace counter_api.Controllers;

[ApiController]
[Route("like")]
public class CounterController : ControllerBase
{
    private readonly LikeService _likeService;
    
    public CounterController(LikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpGet("{postId}/{userId}", Name = "GetUserLike")]
    public async Task<bool> GetUserLike(int postId, int userId)
    {
        return await _likeService.GetUserLike(postId, userId);
    }
    
    [HttpGet("{postId}", Name = "GetLikes")]
    public async Task<LikeCountApiResponse> Get(int postId)
    {
        var count = await _likeService.GetLikeCount(postId);
        return new LikeCountApiResponse
        {
            Count = count,
            Date = DateTime.Now
        };
    }
    
    [EnableRateLimiting("likeRateLimitPerIpAddress")]
    [HttpPut]
    public NewLikeApiResponse Update([FromBody]LikeRequest like)
    {
        try
        {
            _likeService.AddLike(like);
        } catch (Exception e)
        {
        }

        var newCount = like.CurrentCount + (like.Type == NewLikeType.Like ? 1 : -1);
        return new NewLikeApiResponse
        {
            Count = newCount,
            Date = DateTime.Now
        };
    }
}