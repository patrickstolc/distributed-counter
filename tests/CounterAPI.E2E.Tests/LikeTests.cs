using counter_api.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using SharedModels;
using Xunit.Abstractions;

namespace CounterAPI.E2E.Tests;

public class LikeTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private TestClient? _client;
    private IFutureDockerImage _counterServiceImage;
    private IFutureDockerImage _counterApiImage;

    private INetwork _network;
    private IContainer _counterService;
    private IContainer _counterApi;
    private IContainer _redis;
    private IContainer _rabbitmq;

    public LikeTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public async void CanGetLikeCount()
    {   
        await Task.Delay(5 * 1000);
        // Arrange
        var likes = new List<LikeRequest>
        {
            new LikeRequest
            {
                Id = 1,
                Date = DateTime.Now,
                UserId = 1,
                PostId = 1,
                CurrentCount = 0,
                Type = NewLikeType.Like
            },
            new LikeRequest
            {
                Id = 2,
                Date = DateTime.Now,
                UserId = 2,
                PostId = 1,
                CurrentCount = 0,
                Type = NewLikeType.Like
            },
            new LikeRequest
            {
                Id = 3,
                Date = DateTime.Now,
                UserId = 3,
                PostId = 1,
                CurrentCount = 0,
                Type = NewLikeType.Like
            }
        };

        try
        {
            foreach (var like in likes)
            {
                await _client!.AddLike(like);
                _output.WriteLine("Like added");
                await Task.Delay(1000);
            }
        } catch (Exception e)
        {
            _output.WriteLine(e.Message);
            _output.WriteLine(e.StackTrace);
            await Task.Delay(5 * 1000);
        }
        
        // Act
        var response = await _client.GetLikeCount(1);
        _output.WriteLine($"Like count for post 1 is {response!.Count}");
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response!.Count);
    }
    
    public async Task InitializeAsync()
    {
        _output.WriteLine("Starting containers");
        
        _client = new TestClient(
            new HttpClient()
        );
        
        // network
        _network = new NetworkBuilder().Build();
        
        // rabbitmq
        _rabbitmq = new ContainerBuilder()
            .WithImage("rabbitmq:3-management")
            .WithName("rabbitmq")
            .WithNetwork(_network)
            .WithNetworkAliases("rabbitmq")
            .WithPortBinding(5672, false)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Time to start RabbitMQ.*\\n"))
            .Build();
        await _rabbitmq.StartAsync();
        _output.WriteLine("RabbitMQ started");
        
        // redis
        _redis = new ContainerBuilder()
            .WithImage("redislabs/redisearch:2.2.5")
            .WithName("like-cache")
            .WithNetwork(_network)
            .WithNetworkAliases("like-cache")
            .WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Ready to accept connections.*\\n"))
            .Build();
        await _redis.StartAsync();
        _output.WriteLine("Redis started");
        
        // counter service
        _counterServiceImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(
                CommonDirectoryPath.GetSolutionDirectory(),
                string.Empty
            )
            .WithDockerfile("projects/CounterService/Dockerfile")
            .WithCleanUp(true)
            .Build();
        await _counterServiceImage.CreateAsync();

        var redisHostname = $"{_redis.IpAddress}:{_redis.GetMappedPublicPort(6379)}";
        
        _output.WriteLine($"Redis hostname: {redisHostname}");
        _counterService = new ContainerBuilder()
            .WithName("counterservice")
            .WithImage(_counterServiceImage)
            .WithNetwork(_network)
            .WithNetworkAliases("counterservice")
            .WithPortBinding("5050", "80")
            .WithEnvironment("REDIS_HOSTNAME", $"like-cache")
            .WithEnvironment("EASYNETQ_CONNECTION_STRING", "host=rabbitmq;port=5672;virtualHost=/;username=guest;password=guest")
            .DependsOn(_redis)
            .DependsOn(_rabbitmq)
            .WithCleanUp(true)
            .Build();
        await _counterService.StartAsync();
        _output.WriteLine("Counter service started");
        
        // counter api
        _counterApiImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(
                CommonDirectoryPath.GetSolutionDirectory(),
                string.Empty
            )
            .WithDockerfile("projects/CounterAPI/Dockerfile")
            .Build();
        await _counterApiImage.CreateAsync();
        
        _counterApi = new ContainerBuilder()
            .WithName("counterapi")
            .WithImage(_counterApiImage)
            .WithNetwork(_network)
            .WithNetworkAliases("counterapi")
            .WithPortBinding("8080", "80")
            .WithEnvironment("EASYNETQ_CONNECTION_STRING", "host=rabbitmq;port=5672;virtualHost=/;username=guest;password=guest")
            .WithEnvironment("LIKE_SERVICE_HOST","http://counterservice")
            .DependsOn(_redis)
            .DependsOn(_rabbitmq)
            .Build();
        await _counterApi.StartAsync();
        _output.WriteLine("Counter API started");
    }
    
    public async Task DisposeAsync()
    {
        await _counterApiImage.DisposeAsync().ConfigureAwait(false);
        await _counterServiceImage.DisposeAsync().ConfigureAwait(false);
        await _counterService.DisposeAsync().ConfigureAwait(false);
        await _counterApi.DisposeAsync().ConfigureAwait(false);
        await _redis.DisposeAsync().ConfigureAwait(false);
        await _rabbitmq.DisposeAsync().ConfigureAwait(false);
    }
}