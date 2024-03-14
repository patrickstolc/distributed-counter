using System.Net.Http.Json;
using counter_api.Models;
using SharedModels;

namespace CounterAPI.E2E.Tests;

public class TestClient
{
    private readonly HttpClient _httpClient;

    public TestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<NewLikeApiResponse> AddLike(LikeRequest like)
    {
        var url = "http://localhost:8080/like";
        using HttpResponseMessage response = await _httpClient.PutAsJsonAsync(url, like);
        response.EnsureSuccessStatusCode();
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<NewLikeApiResponse>();
        return jsonResponse!;
    }

    public async Task<LikeCountApiResponse> GetLikeCount(int postId)
    {
        var url = "http://localhost:8080/like/1";
        using HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<LikeCountApiResponse>();
        return jsonResponse!;
    }
}