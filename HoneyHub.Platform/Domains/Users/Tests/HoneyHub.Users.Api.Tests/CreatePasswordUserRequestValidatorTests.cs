using FluentAssertions;
using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.Api.Validation.Users;

namespace HoneyHub.Users.Api.Tests;

public class CreatePasswordUserRequestValidatorTests
{
    private readonly CreatePasswordUserRequestValidator _validator = new();

    [Fact(DisplayName = "Valid request passes validation")]
    public void Validate_PayloadValid_IsValid()
    {
        var request = new CreatePasswordUserRequest
        {
            Username = "valid.user",
            Email = "valid@example.com",
            Password = "Str0ng!Pass",
            SubscriptionPlanId = 1
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Weak password fails complexity rule")]
    public void Validate_PasswordTooWeak_HasValidationError()
    {
        var request = new CreatePasswordUserRequest
        {
            Username = "valid.user",
            Email = "valid@example.com",
            Password = "password",
            SubscriptionPlanId = 1
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePasswordUserRequest.Password));
    }

    [Fact(DisplayName = "Username equal to email fails business rule proxy validation")]
    public void Validate_UsernameEqualsEmail_HasValidationError()
    {
        var request = new CreatePasswordUserRequest
        {
            Username = "same@example.com",
            Email = "same@example.com",
            Password = "Str0ng!Pass",
            SubscriptionPlanId = 1
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot be identical", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "Username with invalid chars fails")]
    public void Validate_UsernameInvalidChars_HasValidationError()
    {
        var request = new CreatePasswordUserRequest
        {
            Username = "invalid*!",
            Email = "valid@example.com",
            Password = "Str0ng!Pass"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePasswordUserRequest.Username));
    }

    [Fact(DisplayName = "Email invalid format fails")]
    public void Validate_EmailInvalidFormat_HasValidationError()
    {
        var request = new CreatePasswordUserRequest
        {
            Username = "valid.user",
            Email = "not-an-email",
            Password = "Str0ng!Pass"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePasswordUserRequest.Email));
    }

    [Fact(DisplayName = "Password min length boundary fails at 7")]
    public void Validate_PasswordBelowMinLen_HasValidationError()
    {
        var request = new CreatePasswordUserRequest
        {
            Username = "valid.user",
            Email = "valid@example.com",
            Password = "Aa1!aaa" // 7 chars
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Password max length boundary valid at 128")]
    public void Validate_PasswordAtMaxLen_IsValid()
    {
        var basePwd = "Aa1!";
        var repeated = string.Concat(Enumerable.Repeat(basePwd, 32)); // 128 chars
        var request = new CreatePasswordUserRequest
        {
            Username = "valid.user",
            Email = "valid@example.com",
            Password = repeated
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Password above max length fails at 129+")]
    public void Validate_PasswordAboveMaxLen_HasValidationError()
    {
        var basePwd = "Aa1!";
        var repeated = string.Concat(Enumerable.Repeat(basePwd, 32)) + "X"; // 129 chars
        var request = new CreatePasswordUserRequest
        {
            Username = "valid.user",
            Email = "valid@example.com",
            Password = repeated
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
