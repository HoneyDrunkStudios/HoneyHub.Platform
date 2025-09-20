using FluentAssertions;
using HoneyHub.Users.AppService.Services.SecurityServices;
using HoneyHub.Users.AppService.Services.SecurityServices.InternalModels;
using Microsoft.Extensions.Options;

namespace HoneyHub.Users.AppService.Tests.SecurityServices;

public class PasswordServiceTests
{
    private static PasswordService CreateService(PasswordHashingOptions? options = null)
    {
        options ??= new PasswordHashingOptions
        {
            DegreeOfParallelism = PasswordHashingOptions.Development.DegreeOfParallelism,
            Iterations = PasswordHashingOptions.Development.Iterations,
            MemorySize = PasswordHashingOptions.Development.MemorySize,
            HashLength = PasswordHashingOptions.Development.HashLength
        };
        return new PasswordService(Options.Create(options));
    }

    [Fact(DisplayName = "CreateSalt returns base64 string")]
    public void CreateSalt_Always_ReturnsBase64()
    {
        var svc = CreateService();

        var salt = svc.CreateSalt();

        salt.Should().NotBeNullOrWhiteSpace();
        Action act = () => Convert.FromBase64String(salt);
        act.Should().NotThrow();
    }

    [Fact(DisplayName = "HashPassword deterministic for same inputs")]
    public void HashPassword_SameInputs_ProducesDeterministicHash()
    {
        var svc = CreateService();
        const string password = "Str0ng!Pass";
        var salt = svc.CreateSalt();

        var h1 = svc.HashPassword(password, salt);
        var h2 = svc.HashPassword(password, salt);

        h1.Should().Be(h2);
    }

    [Fact(DisplayName = "VerifyPassword returns true when correct")]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var svc = CreateService();
        var salt = svc.CreateSalt();
        const string password = "Str0ng!Pass";

        var hash = svc.HashPassword(password, salt);
        var ok = svc.VerifyPassword(password, hash, salt);

        ok.Should().BeTrue();
    }

    [Fact(DisplayName = "VerifyPassword returns false on invalid base64")]
    public void VerifyPassword_InvalidBase64_ReturnsFalse()
    {
        var svc = CreateService();
        var ok = svc.VerifyPassword("pass", "not-base64", "also-not-base64");
        ok.Should().BeFalse();
    }
}
