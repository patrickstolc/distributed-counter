using EasyNetQ;

namespace MessageClient.Drivers.EasyNetQ.MessagingStrategies;

public class SendReceiveStrategy : MessagingStrategies.MessagingStrategy
{
    private readonly string _queueName;

    public SendReceiveStrategy(string queueName)
    {
        _queueName = queueName;
    }

    public override void Send<TMessage>(TMessage message, IBus bus)
    {
        bus.SendReceive.Send(_queueName, message);
    }

    public override IDisposable Listen<TMessage>(Action<TMessage> callback, IBus bus)
    {
        return bus.SendReceive.Receive(_queueName, callback);
    }
}