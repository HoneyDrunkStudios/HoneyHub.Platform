using FluentAssertions;
using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.Api.Validation.Users;

namespace HoneyHub.Users.Api.Tests.Validation.Users;

public class CreateExternalUserRequestValidatorTests
{
    private readonly CreateExternalUserRequestValidator _validator = new();

    //TODO: Re-enable when working on external provider integration, needs to be enum 
    //[Fact(DisplayName = "Valid external request passes validation")]
    //public void Validate_PayloadValid_IsValid()
    //{
    //    var request = new CreateExternalUserRequest
    //    {
    //        Username = "valid.user",
    //        Email = "valid@example.com",
    //        ProviderId = "google-oauth2", 
    //        SubscriptionPlanId = 1
    //    };

    //    var result = _validator.Validate(request);
    //    result.IsValid.Should().BeTrue();
    //}

    [Fact(DisplayName = "Missing provider id fails validation")]
    public void Validate_MissingProviderId_HasValidationError()
    {
        var request = new CreateExternalUserRequest
        {
            Username = "valid.user",
            Email = "valid@example.com",
            ProviderId = ""
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateExternalUserRequest.ProviderId));
    }

    [Fact(DisplayName = "Username equal to email fails")]
    public void Validate_UsernameEqualsEmail_HasValidationError()
    {
        var request = new CreateExternalUserRequest
        {
            Username = "same@example.com",
            Email = "same@example.com",
            ProviderId = "123"
        };

        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
