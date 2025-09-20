using System.Threading;
using FluentAssertions;
using HoneyHub.Outbox.Abstractions;
using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.AppService.Services.SecurityServices;
using HoneyHub.Users.AppService.Services.Users;
using HoneyHub.Users.AppService.Services.Validators.Users;
using HoneyHub.Users.DataService.Context;
using HoneyHub.Users.DataService.DataServices.Users;
using HoneyHub.Users.DataService.Entities.Subscriptions;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HoneyHub.Users.AppService.Tests.Services.Users;

public class UserServiceRollbackTests
{
    private static UsersContext CreateDbContext(out SqliteConnection connection)
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<UsersContext>()
            .UseSqlite(connection)
            .Options;

        var ctx = new UsersContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact(DisplayName = "CreatePasswordUser rolls back when outbox enqueue fails")]
    public async Task CreatePasswordUserAsync_OutboxThrows_RollsBack()
    {
        using var db = CreateDbContext(out var conn);
        await using var _ = conn;

        var plan = new SubscriptionPlanEntity
        {
            Name = "Basic",
            DisplayName = "Basic",
            Price = 0m,
            Currency = "USD",
            BillingCycle = "Monthly",
            BillingIntervalMonths = 1,
            IsActive = true,
            IsPublic = true,
            IsDefault = true,
            SortOrder = 1,
            PopularBadge = false,
            Version = 1,
            EffectiveFrom = DateTime.UtcNow
        };
        db.Add(plan);
        await db.SaveChangesAsync();

        var outbox = new Mock<IOutboxStore>();
        outbox.Setup(o => o.EnqueueAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<object>(),
            null, null, null,
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Outbox failure"));

        var userData = new UserDataService(db);
        var password = new Mock<IPasswordService>();
        password.Setup(p => p.CreateSalt()).Returns("c2FsdHNhbHQ=");
        password.Setup(p => p.HashPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(Convert.ToBase64String(new byte[16]));

        var validator = new Mock<IUserServiceValidator>();
        validator.Setup(v => v.ValidatePasswordUserRequest(It.IsAny<CreatePasswordUserRequest>()));
        validator.Setup(v => v.ValidateAndGetSubscriptionPlanIdAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(plan.Id);

        var svc = new UserService(db, outbox.Object, userData, password.Object, validator.Object, NullLogger<UserService>.Instance);

        var req = new CreatePasswordUserRequest
        {
            Username = "rollback.user",
            Email = "rollback@example.com",
            Password = "Str0ng!Pass",
            SubscriptionPlanId = plan.Id
        };

        var act = async () => await svc.CreatePasswordUserAsync(req);
        await act.Should().ThrowAsync<InvalidOperationException>();

        (await db.Set<UserEntity>().CountAsync(u => u.UserName == "rollback.user")).Should().Be(0);
    }
}
