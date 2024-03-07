using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using StackExchange.Redis;
using NRedisStack.Search.Literals.Enums;

namespace CounterService.Core.Repositories;

public class CachedLike
{
    public required string PostId { get; set; }
    public int Increment { get; set; }
    public int Decrement { get; set; }
}

public class LikeCountCacheRepository
{
    private readonly ConnectionMultiplexer _redisConnection;
    
    public LikeCountCacheRepository(ConnectionMultiplexer redisConnection)
    {
        _redisConnection = redisConnection;
        SetupLikeSchema();
    }
    
    private IDatabase GetDatabase()
    {
        return _redisConnection.GetDatabase();
    }
    
    private JsonCommands GetJsonCommands()
    {
        return GetDatabase().JSON();
    }
    
    private SearchCommands GetSearchCommands()
    {
        return GetDatabase().FT();
    }

    private void SetupLikeSchema()
    {
        Console.WriteLine("Setting up like schema");
        var ft = GetSearchCommands();

        var schema = new Schema()
            .AddTagField(new FieldName("$.PostId", "PostId"))
            .AddNumericField(new FieldName("$.Increment", "Increment"))
            .AddNumericField(new FieldName("$.Decrement", "Decrement"));

        try
        {
            ft.Create(
                "idx:likes",
                new FTCreateParams().On(IndexDataType.JSON).Prefix("likes:"),
                schema);
        } catch (RedisServerException e)
        {
            // Index already exists
        }
    }
    
    private bool LikeSchemaExists(int postId)
    {
        var ft = GetSearchCommands();
        var res = ft.Search(
            "idx:likes",
            new Query($"@PostId:{{{postId.ToString()}}}")
        );

        return res.Documents.Count > 0;
    }

    private void CreateLikeObject(int postId, int defaultIncrement = 0, int defaultDecrement = 0)
    {
        var json = GetJsonCommands();
        var like = new CachedLike
        {
            PostId = postId.ToString(),
            Increment = defaultIncrement,
            Decrement = defaultDecrement
        };
        json.Set("likes:" + postId, "$", like);
    }
    
    public void IncrementLikeCount(int postId)
    {
        Console.WriteLine($"Incrementing like count for post {postId}");
        if (!LikeSchemaExists(postId))
        {
            Console.WriteLine($"Like count for post {postId}: 1 - schema does not exist.");
            CreateLikeObject(postId, defaultIncrement: 1);
        } else {
            Console.WriteLine($"Incrementing like count for post {postId} using JSON commands.");
            var json = GetJsonCommands();
            json.NumIncrbyAsync("likes:" + postId, "Increment", 1);
        }
    }
    
    public void DecrementLikeCount(int postId)
    {
        Console.WriteLine($"Decrementing like count for post {postId}");
        if (!LikeSchemaExists(postId))
        {
            Console.WriteLine($"Like count for post {postId}: 0 - schema does not exist.");
            CreateLikeObject(postId);
        }
        else
        {
            Console.WriteLine($"Decrementing like count for post {postId} using JSON commands.");
            var json = GetJsonCommands();
            json.NumIncrbyAsync("likes:" + postId, "Decrement", 1);
        }
    }
    
    public void AddUserLike(int userId, int postId)
    {
        Console.WriteLine($"Adding like for user {userId} to post {postId}");
        var db = GetDatabase();
        db.SetAddAsync($"liked:{userId}", postId.ToString(), CommandFlags.FireAndForget);
    }
    
    public void RemoveUserLike(int userId, int postId)
    {
        Console.WriteLine($"Removing like for user {userId} from post {postId}");
        var db = GetDatabase();
        db.SetRemoveAsync($"liked:{userId}", postId.ToString(), CommandFlags.FireAndForget);
    }

    public bool GetUserLike(int postId, int userId)
    {
        Console.WriteLine($"Checking if user {userId} has liked post {postId}");
        var db = GetDatabase();
        return db.SetContains($"liked:{userId}", postId.ToString());
    }

    public int GetLikeCount(int postId)
    {
        if (!LikeSchemaExists(postId))
        {
            Console.WriteLine($"Like count for post {postId}: 0 - schema does not exist.");
            CreateLikeObject(postId);
            return 0;
        }
        
        var json = GetJsonCommands();
        var like = json.Get<CachedLike>("likes:" + postId);
        
        Console.WriteLine($"Like count for post {postId}: {like?.Increment - like?.Decrement}");
        
        if(like == null)
        {
            return 0;
        }
        
        return like.Increment - like.Decrement;
    }
}