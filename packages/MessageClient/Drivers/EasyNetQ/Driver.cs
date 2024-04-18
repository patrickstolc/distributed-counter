using EasyNetQ;
using MessageClient.Drivers.EasyNetQ.MessagingStrategies;

namespace MessageClient.Drivers.EasyNetQ
{
    public class EasyNetQDriver<TMessage> : IDriver<TMessage>
    {
        private readonly string _connectionString;
        private IBus? _bus;
        private IDisposable? _subscriptionResult;
        private readonly MessagingStrategy? _messagingStrategy;

        public EasyNetQDriver(string hostname, int port)
        {
            _connectionString = $"host={hostname};port={port}";
        }
        public EasyNetQDriver(string connectionString, MessagingStrategy? messagingStrategy)
        {
            _connectionString = connectionString;
            _messagingStrategy = messagingStrategy;
        }

        public bool Connected()
        {
            return _bus != null;
        }

        public void Connect()
        {
            // Connect to the message broker
            _bus = RabbitHutch.CreateBus(_connectionString);
        }

        public static IConnection CreateConnection(string? connectionString = null)
        {
            connectionString = connectionString ?? Environment.GetEnvironmentVariable("EASYNETQ_CONNECTION_STRING");
            if (connectionString is null or "")
            {
                throw new InvalidOperationException("EASYNETQ_CONNECTION_STRING environment variable not set");
            }

            return new EasyNetQConnection
            {
                ConnectionHandle = new EasyNetQConnectionHandle { Handle = RabbitHutch.CreateBus(connectionString) }
            };
        }

        public void Disconnect()
        {
            // Check if there's a subscription
            if (_subscriptionResult == null)
            {
                throw new InvalidOperationException("You must call Connect before Disconnect");
            }

            // Disconnect from the message broker
            _subscriptionResult.Dispose();
        }

        public void Listen(Action<TMessage> callback)
        {
            if(!Connected() || _bus == null)
                throw new InvalidOperationException("You must call Connect before Listen");
            _subscriptionResult = _messagingStrategy.Listen<TMessage>(callback, _bus);
        }

        public IDisposable Listen<T>(Action<T> callback, MessagingStrategy messagingStrategy)
        {
            if(!Connected() || _bus == null)
                throw new InvalidOperationException("You must call Connect before Listen");
            _subscriptionResult = messagingStrategy.Listen<T>(callback, _bus);
            return _subscriptionResult;
        }

        public void Send(TMessage message)
        {
            if(!Connected() || _bus == null)
                throw new InvalidOperationException("You must call Connect before Send");
            _messagingStrategy.Send(message, _bus);
        }

        public void Send<T>(T message, MessagingStrategy messagingStrategy)
        {
            if(!Connected() || _bus == null)
                throw new InvalidOperationException("You must call Connect before Send");
            messagingStrategy.Send<T>(message, _bus);
        }
    }
}