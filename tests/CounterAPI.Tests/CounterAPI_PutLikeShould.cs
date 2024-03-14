using Xunit;
using counter_api.Controllers;
using counter_api.Core.Services;
using counter_api.Models;
using Moq;
using SharedModels;

namespace CounterAPI.Tests;

public class CounterAPI_PutLikeShould
{
    [Fact]
    public void PutLikeShould_IncrementByOne()
    {
        // Arrange
        var mock = new Mock<LikeService>();
        mock.Setup(
            service => service.AddLike(new LikeRequest
            {
                CurrentCount = 0,
                PostId = 1,
                Date = DateTime.Now,
                Type = NewLikeType.Like,
                UserId = 1
            })
        );
        var controller = new CounterController(
            mock.Object
        );

        // Act
        var result = controller.Update(
            new LikeRequest
            {
                CurrentCount = 0,
                PostId = 1,
                Date = DateTime.Now,
                Type = NewLikeType.Like,
                UserId = 1
            }
        );

        // Assert
        mock.Verify(service => service.AddLike(
            new LikeRequest
            {
                CurrentCount = 0,
                PostId = 1,
                Date = DateTime.Now,
                Type = NewLikeType.Like,
                UserId = 1
            }), Times.AtMostOnce);
        Assert.Equal(
            1,
            result.Count        
        );
    }
    
    [Fact]
    public void PutLikeShould_DecrementByOne()
    {
        // Arrange
        var mock = new Mock<LikeService>();
        mock.Setup(
            service => service.AddLike(new LikeRequest
            {
                CurrentCount = 0,
                PostId = 1,
                Date = DateTime.Now,
                Type = NewLikeType.Dislike,
                UserId = 1
            })
        );
        var controller = new CounterController(
            mock.Object
        );

        // Act
        var result = controller.Update(
            new LikeRequest
            {
                CurrentCount = 1,
                PostId = 1,
                Date = DateTime.Now,
                Type = NewLikeType.Dislike,
                UserId = 1
            }
        );

        // Assert
        mock.Verify(service => service.AddLike(
            new LikeRequest
            {
                CurrentCount = 1,
                PostId = 1,
                Date = DateTime.Now,
                Type = NewLikeType.Dislike,
                UserId = 1
            }), Times.AtMostOnce);
        Assert.Equal(
            0,
            result.Count        
        );
    }
}