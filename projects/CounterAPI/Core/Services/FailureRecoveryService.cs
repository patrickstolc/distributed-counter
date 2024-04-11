using MessageClient;
using SharedModels;

namespace counter_api.Core.Services;

public class FailureRecoveryService
{
    private readonly FailedMessageCache _failedMessageCache;
    private readonly MessageClient<NewLikeMessage> _newLikeClient;
    private readonly Dictionary<string, Type> _messageHashMap;
    
    public FailureRecoveryService(FailedMessageCache failedMessageCache, MessageClient<NewLikeMessage> newLikeClient)
    {
        _failedMessageCache = failedMessageCache;
        _newLikeClient = newLikeClient;
    }

    public bool CanIRecover()
    {
        return _newLikeClient.CanConnect();
    }
    
    public void Recover()
    {
        var messages = _failedMessageCache.GetFailedMessages<NewLikeMessage>("new-like");
        if (messages == null)
            return;
        
        foreach (var message in messages)
        {
            _newLikeClient.SendUsingQueue(message, "new-like");
            _failedMessageCache.RemoveFailedMessage("new-like", message);
        }
    }
}