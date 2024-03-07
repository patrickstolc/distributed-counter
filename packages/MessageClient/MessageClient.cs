using MessageClient.Drivers;
using MessageClient.Drivers.EasyNetQ.MessagingStrategies;

namespace MessageClient;

public class MessageClient<TMessage>: IDisposable
{
    private readonly IDriver<TMessage> _driver;

    public MessageClient(IDriver<TMessage> driver)
    {
        _driver = driver;
    }

    public MessageClient<TMessage> Connect()
    {
      _driver.Connect();
      return this;
    }
    
    public void ConnectAndListen(Action<TMessage> callback)
    {
        Connect().Listen(callback);
    }

    private void Listen(Action<TMessage> callback)
    {
      _driver.Listen(callback);
    }
    
    private IDisposable Listen<T>(Action<T> callback, MessagingStrategy messagingStrategy)
    {
      return _driver.Listen<T>(callback, messagingStrategy);
    }
    
    public IDisposable ListenUsingTopic<T>(Action<T> callback, string clientId, string topic)
    {
        return Listen<T>(callback, new TopicStrategy(clientId, topic));
    }
    
    public IDisposable ListenUsingQueue<T>(Action<T> callback, string queue)
    {
        return Listen<T>(callback, new SendReceiveStrategy(queue));
    }
    
    public IDisposable ListenUsingPubSub<T>(Action<T> callback, string clientId)
    {
        return Listen<T>(callback, new PubSubStrategy(clientId));
    }
    
    public void Send(TMessage message)
    {
      _driver.Send(message);
    }

    private void Send<T>(T message, MessagingStrategy messagingStrategy)
    {
      _driver.Send<T>(message, messagingStrategy);
    }
    
    public void SendUsingTopic<T>(T message, string topic)
    {
        Send<T>(message, new TopicStrategy(null, topic));
    }
    
    public void SendUsingQueue<T>(T message, string queue)
    {
        Send<T>(message, new SendReceiveStrategy(queue));
    }
    
    public void SendUsingPubSub<T>(T message)
    {
        Send<T>(message, new PubSubStrategy(null));
    }

    public void Dispose()
    {
      if (!_driver.Connected())
        return;
      _driver.Disconnect();
    }
}
