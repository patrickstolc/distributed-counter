using System.Text.Json;
using StackExchange.Redis;

namespace counter_api.Core.Services;

public class FailedMessageCache
{
    private readonly ConnectionMultiplexer _redis;

    public FailedMessageCache(string hostname, int port)
    {
        _redis = ConnectionMultiplexer.Connect($"{hostname}:{port}");
    }
    
    public bool HasFailedMessages(string messageType)
    {
        IDatabase db = _redis.GetDatabase();
        return db.ListLength(messageType) > 0;
    }

    public void AddFailedMessage(string messageType, object message)
    {
        string serializedMessage = JsonSerializer.Serialize(message);
        
        IDatabase db = _redis.GetDatabase();
        db.ListLeftPush(messageType, serializedMessage);
    }
    
    public void RemoveFailedMessage(string messageType, object message)
    {
        string serializedMessage = JsonSerializer.Serialize(message);
        
        IDatabase db = _redis.GetDatabase();
        db.ListRemove(messageType, serializedMessage);
    }
    
    public IEnumerable<T>? GetFailedMessages<T>(string messageType)
    {
        
        IDatabase db = _redis.GetDatabase();
        RedisValue[] messages = db.ListRange(messageType, 0, -1);
        string[] serializedMessages = messages.Select(m => m.ToString()).ToArray();

        return serializedMessages.Select(message => JsonSerializer.Deserialize<T>(message));
    }
}