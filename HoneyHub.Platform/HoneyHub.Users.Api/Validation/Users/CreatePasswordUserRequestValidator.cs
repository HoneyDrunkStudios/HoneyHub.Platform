using FluentValidation;
using HoneyHub.Users.Api.Sdk.Requests;

namespace HoneyHub.Users.Api.Validation.Users;

/// <summary>
/// FluentValidation validator for CreatePasswordUserRequest.
/// Implements technical validation rules for password-based user creation.
/// Business rules remain in the domain layer via UserServiceValidator.
/// </summary>
public class CreatePasswordUserRequestValidator : AbstractValidator<CreatePasswordUserRequest>
{
    public CreatePasswordUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .Length(1, 256)
            .WithMessage("Username must be between 1 and 256 characters.")
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .Length(1, 256)
            .WithMessage("Email must be between 1 and 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128)
            .WithMessage("Password must not exceed 128 characters.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
            .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.");

        // Note: SubscriptionPlanId validation removed as it's handled by business rules in UserServiceValidator
        // The property may be nullable or have specific business logic for default values

        // Business rule validation: Username and Email cannot be identical
        RuleFor(x => x)
            .Must(x => !string.Equals(x.Username?.Trim(), x.Email?.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Username and email address cannot be identical.")
            .WithName("UsernameEmailUniqueness");
    }
}
