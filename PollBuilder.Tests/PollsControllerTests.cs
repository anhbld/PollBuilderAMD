using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using PollBuilder.API.Controllers;
using PollBuilder.API.Data;
using PollBuilder.API.DTOs;
using PollBuilder.API.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class PollsControllerTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CastVote_ShouldSaveToDatabase_WhenRequestIsValid()
    {
        // Arrange
        var context = GetInMemoryContext();
        var mockHubContext = new Mock<IHubContext<PollHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var poll = new Poll { Code = "tst123", Question = "Fav framework?", Options = new() { "React", "Vue" } };
        context.Polls.Add(poll);
        await context.SaveChangesAsync();

        var controller = new PollsController(context, mockHubContext.Object);

        // Act
        var result = await controller.Vote("tst123", new VoteRequest(0, "", "voter-token-abc"));

        // Assert
        Assert.NotNull(result);
        var dbVote = await context.Votes.FirstOrDefaultAsync(v => v.PollCode == "tst123");
        Assert.NotNull(dbVote);
        Assert.Equal("voter-token-abc", dbVote.VoterToken);
    }

    [Fact]
    public async Task DuplicateVote_ShouldThrowExceptionOrReturnConflict()
    {
        // Arrange
        var context = GetInMemoryContext();
        var mockHubContext = new Mock<IHubContext<PollHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var poll = new Poll { Code = "dup456", Question = "Testing duplicates", Options = new() { "A", "B" } };
        context.Polls.Add(poll);
        context.Votes.Add(new Vote { PollCode = "dup456", OptionIndex = 0, VoterToken = "shared-token" });
        await context.SaveChangesAsync();

        var controller = new PollsController(context, mockHubContext.Object);

        // Act
        var actionResult = await controller.Vote("dup456", new VoteRequest(1, "", "shared-token"));

        // Assert
        Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(actionResult);
    }
}