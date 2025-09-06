using System.ComponentModel.DataAnnotations;

namespace HoneyHub.Users.AppService.Models.Requests;

/// <summary>
/// Request model for creating a user with password-based authentication.
/// Validates input data and enforces business rules for password user creation.
/// </summary>
public class CreatePasswordUserRequest
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
    /// The password for the new user account.
    /// Will be securely hashed using Argon2id algorithm before storage.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public required string Password { get; set; }

    /// <summary>
    /// Optional phone number for the user account.
    /// Used for two-factor authentication and notifications.
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(32, ErrorMessage = "Phone number must not exceed 32 characters")]
    public string? PhoneNumber { get; set; }

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
