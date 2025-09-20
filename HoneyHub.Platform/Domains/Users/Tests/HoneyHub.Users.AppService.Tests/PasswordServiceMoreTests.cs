using FluentAssertions;
using HoneyHub.Users.AppService.Services.SecurityServices;
using HoneyHub.Users.AppService.Services.SecurityServices.InternalModels;
using Microsoft.Extensions.Options;

namespace HoneyHub.Users.AppService.Tests.SecurityServices;

public class PasswordServiceMoreTests
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

    [Fact(DisplayName = "Hash with different salts differs")]
    public void HashPassword_DifferentSalts_ProducesDifferentHash()
    {
        var svc = CreateService();
        const string password = "Str0ng!Pass";

        var s1 = svc.CreateSalt();
        var s2 = svc.CreateSalt();

        var h1 = svc.HashPassword(password, s1);
        var h2 = svc.HashPassword(password, s2);

        h1.Should().NotBe(h2);
    }

    [Fact(DisplayName = "Verify wrong password returns false")]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var svc = CreateService();
        var salt = svc.CreateSalt();
        var hash = svc.HashPassword("Correct!1", salt);

        var ok = svc.VerifyPassword("Wrong!1", hash, salt);
        ok.Should().BeFalse();
    }
}
