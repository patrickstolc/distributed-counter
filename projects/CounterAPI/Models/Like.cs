using SharedModels;

namespace counter_api.Models;

public class LikeRequest
{
    public int Id { get; init; }
    public DateTime Date { get; set; } 
    public int UserId { get; set; }
    public int PostId { get; set; }
    public int CurrentCount { get; set; }
    public NewLikeType Type { get; set; } = NewLikeType.Like;
}