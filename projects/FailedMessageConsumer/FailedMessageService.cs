using System.Net;
using System.Net.Sockets;
using MessageClient;
using MessageClient.Drivers.EasyNetQ;

namespace FailedMessageConsumer;

public class FailedMessageService<T>
{
    private readonly MessageClient<T> _messageClient;
    private readonly FailedMessageCache.FailedMessageCache _failedMessageCache;
    
    private readonly string _serviceHostname;
    private readonly int _servicePort;
    
    private readonly string _messageTypeName;
    private readonly MessageProtocol _messageProtocol;

    public FailedMessageService(Config config)
    {
        _messageClient = new MessageClient<T>(
            new EasyNetQDriver<T>(
                config.Hostname,
                config.Port
            )
        );
        _failedMessageCache = new FailedMessageCache.FailedMessageCache(config.CacheHostname, config.CachePort);
        _serviceHostname = config.Hostname;
        _servicePort = config.Port;
        _messageTypeName = config.MessageTypeName;
        _messageProtocol = config.MessageProtocol;
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
        return _failedMessageCache.HasFailedMessages(_messageTypeName);
    }

    public bool CanIRecover()
    {
        return CheckConnectivity(_serviceHostname, _servicePort);
    }

    private void ResendUsingPubSub(T message)
    {
        _messageClient.SendUsingPubSub(message);
        RemoveFromCache(message);
    }
    
    private void ResendUsingQueue(T message, string queueName)
    {
        _messageClient.SendUsingQueue(message, queueName);
        RemoveFromCache(message);
    }
    
    private void ResendUsingTopic(T message, string topicName)
    {
        try
        {
            _messageClient.SendUsingTopic(message, topicName);
            RemoveFromCache(message);
        } catch (Exception e)
        {
            Console.WriteLine($"Failed to resend message: {e.Message}");
        }
    }
    
    private void RemoveFromCache(T message)
    {
        _failedMessageCache.RemoveFailedMessage(_messageTypeName, message);
    }
    
    private void ResendMessage(T?[] messages)
    {
        if(messages.All(message => message == null))
            return;
        
        if(messages.Length == 0)
            return;
        
        switch (_messageProtocol)
        {
            case MessageProtocol.PUBSUB:
                Array.ForEach(messages, ResendUsingPubSub);
                break;
            case MessageProtocol.QUEUE:
                Array.ForEach(messages, message => ResendUsingQueue(message, _messageTypeName));
                break;
            case MessageProtocol.TOPIC:
                Array.ForEach(messages, message => ResendUsingTopic(message, _messageTypeName));
                break;
        }
    }
    
    public void Recover()
    {
        var messages = _failedMessageCache.GetFailedMessages<T>(_messageTypeName);
        
        if (messages == null)
            return;
        
        // Connect to the message broker
        _messageClient.Connect();
        
        // Resend the failed messages
        var validMessages = messages.Where(message => message != null).ToArray();
        ResendMessage(validMessages.ToArray());
    }
}