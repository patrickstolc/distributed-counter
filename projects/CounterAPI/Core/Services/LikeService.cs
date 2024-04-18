using counter_api.Models;
using MessageClient;
using SharedModels;

namespace counter_api.Core.Services;

public class LikeService
{
    private readonly MessageClient<NewLikeMessage> _newLikeClient;
    private readonly HttpClient _httpClient;
    private readonly string _likeServiceHost;
    private readonly FailedMessageCache.FailedMessageCache _failedMessageCache;

    public LikeService()
    {
        
    }
    
    public LikeService(MessageClient<NewLikeMessage> newLikeClient, HttpClient httpClient, string likeServiceHost, FailedMessageCache.FailedMessageCache failedMessageCache)
    {
        _newLikeClient = newLikeClient;
        _httpClient = httpClient;
        _likeServiceHost = likeServiceHost;
        _failedMessageCache = failedMessageCache;
    }

    public LikeService Start()
    {
        _newLikeClient.Connect();
        return this;
    }

    private async Task SendNewLikeMessage(LikeRequest like)
    {
        NewLikeMessage message =
            new NewLikeMessage
            {
                PostId = like.PostId,
                UserId = like.UserId,
                Type = like.Type,
                Date = DateTime.Now,
                Count = (like.Type == NewLikeType.Like ? 1 : -1)
            };
        try
        {
            _newLikeClient.SendUsingQueue(
                message,
                "new-like"
            );
        }
        catch (Exception e)
        {
            _failedMessageCache.AddFailedMessage("new-like", message);
        }
    }
    
    public virtual void AddLike(LikeRequest like)
    {
        Console.WriteLine($"Sending new like message for post {like.PostId} of type {like.Type} for user {like.UserId} using 'new-like' queue");
        Task.Run(async () => SendNewLikeMessage(like));
    }
    
    public async Task<bool> GetUserLike(int postId, int userId)
    {
        Console.WriteLine($"Getting user like for post {postId} by user {userId} using {_likeServiceHost}/like/{postId}/{userId}");
        using HttpResponseMessage response = await _httpClient.GetAsync($"{_likeServiceHost}/like/{postId}/{userId}");

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadFromJsonAsync<bool>();
        return jsonResponse;
    }

    public async Task<int> GetLikeCount(int postId)
    {
        Console.WriteLine($"Getting like count for post {postId} using {_likeServiceHost}/like/{postId}");
        using HttpResponseMessage response = await _httpClient.GetAsync($"{_likeServiceHost}/like/{postId}");

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadFromJsonAsync<LikeCountServiceResponse>();
        if (jsonResponse == null)
            return 0;
        
        return jsonResponse.Count;
    }
}