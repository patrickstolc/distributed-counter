using counter_api.Controllers;
using counter_api.Core.Factories;
using MessageClient.Factory;
using WireMock;
using Moq;
using NUnit.Framework;
using SharedModels;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Assert = Xunit.Assert;

namespace CounterAPI.Service.Tests;

public class TestCounterAPI
{
    private WireMockServer _counterServiceStub;
    
    [SetUp]
    public void Setup()
    {
        _counterServiceStub = WireMockServer.Start(
            new WireMockServerSettings
            {
                Urls = new[] { "http://localhost:5001" }
            }    
        );
        _counterServiceStub.Given(
            Request.Create().WithPath("/like/1").UsingGet()
        ).RespondWith(
            Response.Create().WithBody(
                "{\"Count\": 200, \"Date\": \"2022-01-01T00:00:00\", \"PostId\": 1}"
            ).WithHeader("Content-Type", "application/json")
        );
    }
    
    [Test]
    public async Task GetLike_InputIs1_Returns200()
    {
        // Arrange
        var mock = new Mock<EasyNetQFactory>();
        mock.Setup(
            factory => factory.CreateSendReceiveMessageClient<NewLikeMessage>("new-like")
        );
        var controller = new CounterController(
            LikeServiceFactory.CreateLikeService()
        );
        
        // Act
        var result = await controller.Get(1);
        
        // Assert
        Assert.Equal(200, result.Count);
    }
}