using EasyNetQ;

namespace MessageClient.Drivers.EasyNetQ;

public class EasyNetQConnectionHandle : IConnectionHandle<IBus> {
    public IBus Handle { get; set; }
}

public class EasyNetQConnection : IConnection {
    public EasyNetQConnectionHandle ConnectionHandle { get; set; }
}