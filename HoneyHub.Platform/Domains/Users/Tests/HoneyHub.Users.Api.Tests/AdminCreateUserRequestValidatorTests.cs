using FluentAssertions;
using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.Api.Validation.Users;

namespace HoneyHub.Users.Api.Tests;

public class AdminCreateUserRequestValidatorTests
{
    private readonly AdminCreateUserRequestValidator _validator = new();

    [Fact(DisplayName = "Valid admin request with password passes validation")]
    public void Validate_WithPassword_IsValid()
    {
        var request = new AdminCreateUserRequest
        {
            Username = "admin.user",
            Email = "admin@example.com",
            Password = "Str0ng!Pass"
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Missing password fails validation")]
    public void Validate_MissingPassword_HasValidationError()
    {
        var request = new AdminCreateUserRequest
        {
            Username = "admin.user",
            Email = "admin@example.com",
            Password = ""
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Weak password fails complexity rule")]
    public void Validate_PasswordTooWeak_HasValidationError()
    {
        var request = new AdminCreateUserRequest
        {
            Username = "admin.user",
            Email = "admin@example.com",
            Password = "password"
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
