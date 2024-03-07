using MessageClient.Drivers.EasyNetQ;
using MessageClient.Drivers.EasyNetQ.MessagingStrategies;

namespace MessageClient.Factory
{
    public abstract class MessageClientFactory
    {
      public abstract MessageClient<TMessage> FactoryMethod<TMessage>(MessagingStrategy messagingStrategy);
    }
};