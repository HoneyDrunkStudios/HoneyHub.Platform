using System.ComponentModel.DataAnnotations;

namespace HoneyHub.Users.AppService.Models.Requests;

/// <summary>
/// Request model for creating a user with external authentication provider.
/// Validates external provider credentials and user profile information.
/// </summary>
public class CreateExternalUserRequest
{
    /// <summary>
    /// The username for the new user account.
    /// Must be unique within the system and follow username conventions.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 256 characters")]
    public required string UserName { get; set; }

    /// <summary>
    /// The email address for the new user account.
    /// Must be unique within the system and will be used for notifications.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
    public required string Email { get; set; }

    /// <summary>
    /// The external authentication provider name.
    /// Examples: "Google", "Microsoft", "GitHub", etc.
    /// </summary>
    [Required(ErrorMessage = "Provider is required")]
    [StringLength(100, ErrorMessage = "Provider must not exceed 100 characters")]
    public required string Provider { get; set; }

    /// <summary>
    /// The unique identifier from the external provider.
    /// This links the user account to their external provider account.
    /// </summary>
    [Required(ErrorMessage = "ProviderId is required")]
    [StringLength(256, ErrorMessage = "ProviderId must not exceed 256 characters")]
    public required string ProviderId { get; set; }

    /// <summary>
    /// Display name from the external provider for user interface purposes.
    /// May be different from the username.
    /// </summary>
    [StringLength(100, ErrorMessage = "ProviderDisplayName must not exceed 100 characters")]
    public string? ProviderDisplayName { get; set; }

    /// <summary>
    /// Optional phone number for the user account.
    /// Used for two-factor authentication and notifications.
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(32, ErrorMessage = "Phone number must not exceed 32 characters")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates whether the email address has been verified by the external provider.
    /// External providers typically verify email addresses during their authentication flow.
    /// </summary>
    public bool EmailConfirmed { get; set; } = false;

    /// <summary>
    /// Optional subscription plan ID for the user.
    /// If not provided, the default subscription plan will be assigned.
    /// </summary>
    public int? SubscriptionPlanId { get; set; }

    /// <summary>
    /// The creator of this user account for audit purposes.
    /// Typically the username or system identifier performing the creation.
    /// </summary>
    [Required(ErrorMessage = "CreatedBy is required")]
    [StringLength(100, ErrorMessage = "CreatedBy must not exceed 100 characters")]
    public required string CreatedBy { get; set; }
}
