namespace FailedMessageConsumer;

public enum MessageProtocol
{
    PUBSUB = 0,
    QUEUE = 1,
    TOPIC = 2
}

public class Config
{
    // Service configuration
    public string Hostname { get; set; }
    public int Port { get; set; }
    
    // Cache configuration
    public string CacheHostname { get; set; }
    public int CachePort { get; set; }
    
    // Message configuration
    public string MessageTypeName { get; set; }
    public MessageProtocol MessageProtocol { get; set; }
}