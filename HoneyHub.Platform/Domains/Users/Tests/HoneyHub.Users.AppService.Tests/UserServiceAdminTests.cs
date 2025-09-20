using System.Threading;
using FluentAssertions;
using HoneyHub.Outbox.Abstractions;
using HoneyHub.Users.Api.Sdk.Requests;
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

public class UserServiceAdminTests
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

    [Fact(DisplayName = "AdminCreateUser with password sets hash and enqueues outbox event")]
    public async Task AdminCreateUserAsync_WithPassword_SetsPasswordHashAndEnqueues()
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
            "User",
            It.IsAny<Guid>(),
            It.IsAny<object>(),
            null, null, null,
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var validator = new Mock<IUserServiceValidator>();
        validator.Setup(v => v.ValidateAdminUserRequest(It.IsAny<AdminCreateUserRequest>()));
        validator.Setup(v => v.ValidateAndGetSubscriptionPlanIdAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(plan.Id);

        var userData = new UserDataService(db);
        var password = new Mock<HoneyHub.Users.AppService.Services.SecurityServices.IPasswordService>();
        password.Setup(p => p.CreateSalt()).Returns("c2FsdHNhbHQ=");
        password.Setup(p => p.HashPassword("Str0ng!Pass", "c2FsdHNhbHQ=")).Returns(Convert.ToBase64String(new byte[16]));

        var svc = new UserService(db, outbox.Object, userData, password.Object, validator.Object, NullLogger<UserService>.Instance);

        var req = new AdminCreateUserRequest
        {
            Username = "admin.user",
            Email = "admin@example.com",
            Password = "Str0ng!Pass",
            SubscriptionPlanId = plan.Id
        };

        var id = await svc.AdminCreateUserAsync(req);
        id.Should().NotBeEmpty();

        var saved = await db.Set<UserEntity>().SingleAsync(u => u.UserName == "admin.user");
        saved.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "AdminCreateUser without password throws due to validator rule")]
    public async Task AdminCreateUserAsync_WithoutPassword_Throws()
    {
        using var db = CreateDbContext(out var conn);
        await using var _ = conn;

        var outbox = new Mock<IOutboxStore>();
        var validator = new Mock<IUserServiceValidator>();
        validator.Setup(v => v.ValidateAdminUserRequest(It.IsAny<AdminCreateUserRequest>()))
                 .Throws(new ArgumentException("Password must be specified for admin user creation."));

        var userData = new UserDataService(db);
        var password = new Mock<HoneyHub.Users.AppService.Services.SecurityServices.IPasswordService>();
        var svc = new UserService(db, outbox.Object, userData, password.Object, validator.Object, NullLogger<UserService>.Instance);

        var req = new AdminCreateUserRequest
        {
            Username = "admin.user",
            Email = "admin@example.com",
            Password = ""
        };

        var act = async () => await svc.AdminCreateUserAsync(req);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
