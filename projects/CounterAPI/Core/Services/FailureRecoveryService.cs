using System.Net;
using System.Net.Sockets;
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

    private bool CheckConnectivity(string hostname, int port)
    {
        try
        {
            TcpClient client = new TcpClient();
            IPAddress ipAddress = Dns.GetHostEntry (hostname).AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint (ipAddress, port);
            client.Connect(ipEndPoint);
            client.Close();
            return true;
        } catch (Exception e)
        {
            return false;
        }
    }
    
    public bool AnyFailedMessages()
    {
        return _failedMessageCache.HasFailedMessages("new-like");
    }

    public bool CanIRecover()
    {
        return CheckConnectivity("rabbitmq", 5672);
    }
    
    public void Recover()
    {
        var messages = _failedMessageCache.GetFailedMessages<NewLikeMessage>("new-like");
        if (messages == null)
            return;

        _newLikeClient.Connect();
        foreach (var message in messages)
        {
            _newLikeClient.SendUsingQueue(message, "new-like");
            _failedMessageCache.RemoveFailedMessage("new-like", message);
        }
    }
}