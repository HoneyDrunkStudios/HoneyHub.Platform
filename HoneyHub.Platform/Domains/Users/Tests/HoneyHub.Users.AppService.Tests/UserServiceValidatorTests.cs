using FluentAssertions;
using HoneyHub.Users.AppService.Services.Validators.Users;
using HoneyHub.Users.DataService.DataServices.Subscriptions;
using HoneyHub.Users.DataService.Entities.Subscriptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HoneyHub.Users.AppService.Tests.Services.Validators.Users;

public class UserServiceValidatorTests
{
    [Fact(DisplayName = "ValidateAndGetSubscriptionPlanIdAsync returns requested plan when active")]
    public async Task ValidateAndGetSubscriptionPlanIdAsync_ActivePlan_ReturnsId()
    {
        var plan = new SubscriptionPlanEntity { Id = 10, Name = "P", DisplayName = "P", Price = 0m, Currency = "USD", BillingCycle = "M", BillingIntervalMonths = 1, IsActive = true, IsPublic = true, IsDefault = false, SortOrder = 1, PopularBadge = false, Version = 1, EffectiveFrom = DateTime.UtcNow };
        var ds = new Mock<ISubscriptionPlanDataService>();
        ds.Setup(d => d.GetById(10)).ReturnsAsync(plan);

        var sut = new UserServiceValidator(ds.Object, NullLogger<UserServiceValidator>.Instance);
        var id = await sut.ValidateAndGetSubscriptionPlanIdAsync(10);
        id.Should().Be(10);
    }

    [Fact(DisplayName = "ValidateAndGetSubscriptionPlanIdAsync throws when plan not found")]
    public async Task ValidateAndGetSubscriptionPlanIdAsync_NotFound_Throws()
    {
        var ds = new Mock<ISubscriptionPlanDataService>();
        ds.Setup(d => d.GetById(99)).ReturnsAsync((SubscriptionPlanEntity?)null);

        var sut = new UserServiceValidator(ds.Object, NullLogger<UserServiceValidator>.Instance);
        var act = async () => await sut.ValidateAndGetSubscriptionPlanIdAsync(99);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact(DisplayName = "ValidateAndGetSubscriptionPlanIdAsync throws when inactive")]
    public async Task ValidateAndGetSubscriptionPlanIdAsync_Inactive_Throws()
    {
        var plan = new SubscriptionPlanEntity { Id = 11, Name = "P", DisplayName = "P", Price = 0m, Currency = "USD", BillingCycle = "M", BillingIntervalMonths = 1, IsActive = false, IsPublic = true, IsDefault = false, SortOrder = 1, PopularBadge = false, Version = 1, EffectiveFrom = DateTime.UtcNow };
        var ds = new Mock<ISubscriptionPlanDataService>();
        ds.Setup(d => d.GetById(11)).ReturnsAsync(plan);

        var sut = new UserServiceValidator(ds.Object, NullLogger<UserServiceValidator>.Instance);
        var act = async () => await sut.ValidateAndGetSubscriptionPlanIdAsync(11);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact(DisplayName = "ValidateAuthenticationMethod throws when no password for admin")]
    public void ValidateAuthenticationMethod_NoPassword_Throws()
    {
        var ds = new Mock<ISubscriptionPlanDataService>();
        var sut = new UserServiceValidator(ds.Object, NullLogger<UserServiceValidator>.Instance);

        var req = new HoneyHub.Users.Api.Sdk.Requests.AdminCreateUserRequest
        {
            Username = "admin",
            Email = "a@b.com",
            Password = ""
        };

        var act = () => sut.ValidateAuthenticationMethod(req);
        act.Should().Throw<ArgumentException>();
    }
}
