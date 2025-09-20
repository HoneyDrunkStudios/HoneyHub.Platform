using System.Threading;
using FluentAssertions;
using HoneyHub.Outbox.Abstractions;
using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.AppService.Services.Users;
using HoneyHub.Users.AppService.Services.Validators.Users;
using HoneyHub.Users.DataService.Context;
using HoneyHub.Users.DataService.DataServices.Users;
using HoneyHub.Users.DataService.Entities.Identity;
using HoneyHub.Users.DataService.Entities.Subscriptions;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HoneyHub.Users.AppService.Tests.Services.Users;

public class UserServiceExternalTests
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

    [Fact(DisplayName = "CreateExternalUser creates user, login and enqueues outbox event")]
    public async Task CreateExternalUserAsync_Valid_CreatesUserLoginAndEnqueues()
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

        var outbox = new Mock<IOutboxStore>(MockBehavior.Strict);
        outbox.Setup(o => o.EnqueueAsync(
            It.IsAny<string>(),
            "User",
            It.IsAny<Guid>(),
            It.IsAny<object>(),
            null, null, null,
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var validator = new Mock<IUserServiceValidator>();
        validator.Setup(v => v.ValidateExternalUserRequest(It.IsAny<CreateExternalUserRequest>()));
        validator.Setup(v => v.ValidateAndGetSubscriptionPlanIdAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(plan.Id);

        var userData = new UserDataService(db);
        var password = new Mock<HoneyHub.Users.AppService.Services.SecurityServices.IPasswordService>();

        var svc = new UserService(db, outbox.Object, userData, password.Object, validator.Object, NullLogger<UserService>.Instance);

        var req = new CreateExternalUserRequest
        {
            Username = "ext.user",
            Email = "ext@example.com",
            ProviderId = "provider-123"
        };

        var publicId = await svc.CreateExternalUserAsync(req);
        publicId.Should().NotBeEmpty();

        var saved = await db.Set<UserEntity>()
            .Include(u => u.Logins)
            .SingleAsync(u => u.UserName == "ext.user");
        saved.Logins.Should().ContainSingle();
        saved.Logins.First().ProviderKey.Should().Be("provider-123");

        outbox.Verify();
    }

    [Fact(DisplayName = "CreateExternalUser respects case-insensitive username uniqueness")]
    public async Task CreateExternalUserAsync_DuplicateUsernameDifferentCase_Throws()
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

        // Seed existing user with uppercase normalized name
        db.Add(new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = "Existing",
            NormalizedUserName = "EXISTING",
            Email = "e@x.com",
            NormalizedEmail = "E@X.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            SubscriptionPlanId = plan.Id
        });
        await db.SaveChangesAsync();

        var outbox = new Mock<IOutboxStore>();
        var validator = new Mock<IUserServiceValidator>();
        validator.Setup(v => v.ValidateExternalUserRequest(It.IsAny<CreateExternalUserRequest>()));
        validator.Setup(v => v.ValidateAndGetSubscriptionPlanIdAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(plan.Id);

        var userData = new UserDataService(db);
        var password = new Mock<HoneyHub.Users.AppService.Services.SecurityServices.IPasswordService>();

        var svc = new UserService(db, outbox.Object, userData, password.Object, validator.Object, NullLogger<UserService>.Instance);

        var req = new CreateExternalUserRequest
        {
            Username = "existing", // different case
            Email = "e2@x.com",
            ProviderId = "123"
        };

        var act = async () => await svc.CreateExternalUserAsync(req);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Username is already taken.*");
    }
}
