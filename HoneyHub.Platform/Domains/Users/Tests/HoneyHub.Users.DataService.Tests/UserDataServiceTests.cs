using FluentAssertions;
using HoneyHub.Users.DataService.Context;
using HoneyHub.Users.DataService.DataServices.Users;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Tests;

public class UserDataServiceTests
{
    private static UsersContext CreateContext()
    {
        var opts = new DbContextOptionsBuilder<UsersContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new UsersContext(opts);
    }

    [Fact(DisplayName = "UserNameExistsAsync returns true when user exists")]
    public async Task UserNameExistsAsync_UserPresent_ReturnsTrue()
    {
        using var ctx = CreateContext();
        var sut = new UserDataService(ctx);

        ctx.Add(new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = "sample",
            NormalizedUserName = "SAMPLE",
            Email = "a@b.com",
            NormalizedEmail = "A@B.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            SubscriptionPlanId = 1
        });
        await ctx.SaveChangesAsync();

        var exists = await sut.UserNameExistsAsync("SAMPLE");

        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "UserNameExistsAsync returns false when user not found")]
    public async Task UserNameExistsAsync_UserAbsent_ReturnsFalse()
    {
        using var ctx = CreateContext();
        var sut = new UserDataService(ctx);

        var exists = await sut.UserNameExistsAsync("MISSING");

        exists.Should().BeFalse();
    }
}
