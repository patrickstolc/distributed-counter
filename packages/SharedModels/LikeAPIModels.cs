using System;
namespace SharedModels;

public class NewLikeApiResponse
{
    public int Count { get; set; }
    public DateTime? Date { get; set; }
}

public class LikeCountApiResponse
{
    public int Count { get; set; }
    public int PostId { get; set; }
    public DateTime Date { get; set; }
}