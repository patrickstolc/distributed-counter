using MessageClient.Drivers.EasyNetQ;

namespace MessageClient.Drivers;

public interface IConnectionHandle<T>
{
    public T Handle { get; set; }
}

public interface IConnection
{
    EasyNetQConnectionHandle ConnectionHandle { get; }
}