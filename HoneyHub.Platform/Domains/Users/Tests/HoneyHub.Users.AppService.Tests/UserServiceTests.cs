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

namespace HoneyHub.Users.AppService.Tests;

public class UserServiceTests
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

    private static UserDataService CreateUserDataService(UsersContext ctx)
        => new(ctx);

    [Fact(DisplayName = "CreatePasswordUser persists user and enqueues outbox event")]
    public async Task CreatePasswordUserAsync_ValidRequest_PersistsAndEnqueues()
    {
        using var db = CreateDbContext(out var conn);
        await using var _ = conn;

        // Seed a valid subscription plan to satisfy FK constraint
        var plan = new SubscriptionPlanEntity
        {
            Name = "Basic",
            DisplayName = "Basic",
            Description = null,
            ShortDescription = null,
            Price = 0m,
            Currency = "USD",
            BillingCycle = "Monthly",
            BillingIntervalMonths = 1,
            IsActive = true,
            IsPublic = true,
            IsDefault = true,
            SortOrder = 1,
            CallToAction = null,
            PopularBadge = false,
            TrialDays = null,
            Version = 1,
            EffectiveFrom = DateTime.UtcNow,
            EffectiveTo = null
        };
        db.Add(plan);
        await db.SaveChangesAsync();

        var outbox = new Mock<IOutboxStore>(MockBehavior.Strict);
        outbox.Setup(o => o.EnqueueAsync(
                It.IsAny<string>(), // eventType
                "User",
                It.IsAny<Guid>(),
                It.IsAny<object>(),
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var userData = CreateUserDataService(db);

        var password = new Mock<IPasswordService>();
        password.Setup(p => p.CreateSalt()).Returns("c2FsdHNhbHQ="); // base64 for "saltsalt"
        password.Setup(p => p.HashPassword("Str0ng!Pass", "c2FsdHNhbHQ=")).Returns(Convert.ToBase64String(new byte[16]));

        var validator = new Mock<IUserServiceValidator>();
        validator.Setup(v => v.ValidatePasswordUserRequest(It.IsAny<CreatePasswordUserRequest>()));
        validator.Setup(v => v.ValidateAndGetSubscriptionPlanIdAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(plan.Id);

        var svc = new UserService(db, outbox.Object, userData, password.Object, validator.Object, NullLogger<UserService>.Instance);

        var req = new CreatePasswordUserRequest
        {
            Username = "new.user",
            Email = "new.user@example.com",
            Password = "Str0ng!Pass",
            SubscriptionPlanId = plan.Id
        };

        var id = await svc.CreatePasswordUserAsync(req, CancellationToken.None);

        id.Should().NotBeEmpty();

        var saved = await db.Set<UserEntity>().SingleAsync(u => u.UserName == "new.user");
        saved.PasswordHash.Should().NotBeNull();
        saved.SubscriptionPlanId.Should().Be(plan.Id);

        outbox.Verify();
    }

    [Fact(DisplayName = "CreatePasswordUser throws when username exists")]
    public async Task CreatePasswordUserAsync_DuplicateUsername_ThrowsArgumentException()
    {
        using var db = CreateDbContext(out var conn);
        await using var _ = conn;

        // Seed a valid subscription plan first
        var plan = new SubscriptionPlanEntity
        {
            Name = "Basic",
            DisplayName = "Basic",
            Description = null,
            ShortDescription = null,
            Price = 0m,
            Currency = "USD",
            BillingCycle = "Monthly",
            BillingIntervalMonths = 1,
            IsActive = true,
            IsPublic = true,
            IsDefault = true,
            SortOrder = 1,
            CallToAction = null,
            PopularBadge = false,
            TrialDays = null,
            Version = 1,
            EffectiveFrom = DateTime.UtcNow,
            EffectiveTo = null
        };
        db.Add(plan);
        await db.SaveChangesAsync();

        // Seed existing user referencing the seeded plan
        db.Add(new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = "existing",
            NormalizedUserName = "EXISTING",
            Email = "e@x.com",
            NormalizedEmail = "E@X.COM",
            EmailConfirmed = false,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            SubscriptionPlanId = plan.Id
        });
        await db.SaveChangesAsync();

        var outbox = new Mock<IOutboxStore>();
        var userData = CreateUserDataService(db);
        var password = new Mock<IPasswordService>();
        var validator = new Mock<IUserServiceValidator>();
        validator.Setup(v => v.ValidatePasswordUserRequest(It.IsAny<CreatePasswordUserRequest>()));
        validator.Setup(v => v.ValidateAndGetSubscriptionPlanIdAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(plan.Id);

        var svc = new UserService(db, outbox.Object, userData, password.Object, validator.Object, NullLogger<UserService>.Instance);

        var req = new CreatePasswordUserRequest
        {
            Username = "existing",
            Email = "e2@x.com",
            Password = "Str0ng!Pass",
            SubscriptionPlanId = plan.Id
        };

        var act = async () => await svc.CreatePasswordUserAsync(req, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Username is already taken.*");
    }
}
