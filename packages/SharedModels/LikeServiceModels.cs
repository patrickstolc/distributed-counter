namespace SharedModels;

public enum NewLikeType
{
    Like = 0,
    Dislike = 1
}

public class Like
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public NewLikeType Type { get; set; } = NewLikeType.Like;
}

public class NewLikeMessage
{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public NewLikeType Type { get; set; } = NewLikeType.Like;
    public int Count { get; set; }
}

public class LikeUpdateMessage
{
    public int PostId { get; set; }
    public int Count { get; set; }
}

public class LikeCountServiceResponse
{
    public int Count { get; set; }
    public int PostId { get; set; }
    public DateTime Date { get; set; }
}