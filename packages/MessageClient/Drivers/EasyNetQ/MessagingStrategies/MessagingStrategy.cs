using EasyNetQ;

namespace MessageClient.Drivers.EasyNetQ.MessagingStrategies;


public abstract class MessagingStrategy
{
  public abstract void Send<TMessage>(TMessage message, IBus bus);
  public abstract IDisposable Listen<TMessage>(Action<TMessage> callback, IBus bus);
}