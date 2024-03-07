using System.Collections.Concurrent;
using MessageClient;
using SharedModels;

namespace Notifications.Core;

public class MessagingService
{
    private readonly ConcurrentDictionary<string, IConnectionAdapter> _connections = new();
    private readonly ConcurrentDictionary<string, ISet<string>> _likeUpdateSubscriptions = new();
    private readonly MessageClient<LikeUpdateMessage> _updateMessageClient;
    
    public MessagingService(MessageClient<LikeUpdateMessage> updateMessageClient)
    {
        _updateMessageClient = updateMessageClient;
    }
    
    public MessagingService Start()
    {
        _updateMessageClient.ConnectAndListen(HandleLikeUpdate);
        return this;
    }
    
    public Task<string?> TryAddUser(IConnectionAdapter connection, string userId)
    {
        if (!TryAddUser(userId, connection))
        {
            return Task.FromResult($"Userid '{userId}' already taken");
        }
        
        SendMessage(connection, new UserConnected(userId));
        return Task.FromResult<string?>(null);
    }
    
    public Task<string?> TrySubscribeUserToLikeUpdates(string postId, string userId)
    {
        if (!SubscribeUserToLikeUpdates(userId, postId))
        {
            return Task.FromResult($"User '{userId}' already subscribed to like updates for post '{postId}'");
        }

        return Task.FromResult<string?>(null);
    }
    
    private bool SubscribeUserToLikeUpdates(string userId, string postId)
    {
        if (!_likeUpdateSubscriptions.TryGetValue(postId, out var userIds))
        {
            userIds = new HashSet<string>();
            _likeUpdateSubscriptions.TryAdd(postId, userIds);
        }
        
        return userIds.Add(userId);
    }
    
    private bool TryAddUser(string name, IConnectionAdapter connection)
    {
        if (_connections.ContainsKey(name))
        {
            return false;
        }

        _connections.TryAdd(name, connection);
        
        return true;
    }
    
    public void RemoveUser(string name)
    {
        _connections.TryRemove(name, out _);
    }
    
    private async void HandleLikeUpdate(LikeUpdateMessage message)
    {
        await BroadcastLikeUpdate(message);
    }

    private Task BroadcastLikeUpdate(LikeUpdateMessage message)
    {
        var receivers = GetLikeUpdateReceivers(message.PostId.ToString());
        var connectionAdapters = receivers as IConnectionAdapter[] ?? receivers.ToArray();
        if(!connectionAdapters.Any())
        {
            return Task.CompletedTask;
        }

        var updateMessage = new LikeUpdate(message.PostId.ToString(), message.Count);
        
        return BroadcastMessage(updateMessage, connectionAdapters);
    }
    
    private IEnumerable<IConnectionAdapter> GetLikeUpdateReceivers(string postId)
    {
        return _connections.Values;
    }

    private Task SendMessage(IConnectionAdapter connection, Message message)
    {
        return connection.SendMessage(message);
    }

    private async Task BroadcastMessage(Message message, IEnumerable<IConnectionAdapter>? receivers = null)
    {
        foreach (var connection in receivers ?? _connections.Values)
        {
            await connection.SendMessage(message);
        }
    }
}