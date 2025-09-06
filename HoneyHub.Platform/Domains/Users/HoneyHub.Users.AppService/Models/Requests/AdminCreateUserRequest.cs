using System.ComponentModel.DataAnnotations;

namespace HoneyHub.Users.AppService.Models.Requests;

/// <summary>
/// Request model for administrative user creation with elevated privileges.
/// Allows administrators to create users with specific settings and bypass certain validations.
/// </summary>
public class AdminCreateUserRequest
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
    /// Optional password for password-based authentication.
    /// If provided, will be securely hashed. If not provided, user must use external authentication.
    /// </summary>
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string? Password { get; set; }

    /// <summary>
    /// Optional external authentication provider name.
    /// Required if external authentication is being configured.
    /// </summary>
    [StringLength(100, ErrorMessage = "Provider must not exceed 100 characters")]
    public string? Provider { get; set; }

    /// <summary>
    /// Optional unique identifier from the external provider.
    /// Required if Provider is specified.
    /// </summary>
    [StringLength(256, ErrorMessage = "ProviderId must not exceed 256 characters")]
    public string? ProviderId { get; set; }

    /// <summary>
    /// Optional display name from the external provider.
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
    /// Whether the email address should be marked as confirmed.
    /// Administrators can bypass email verification for new accounts.
    /// </summary>
    public bool EmailConfirmed { get; set; } = false;

    /// <summary>
    /// Whether the phone number should be marked as confirmed.
    /// Administrators can bypass phone verification for new accounts.
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; } = false;

    /// <summary>
    /// Whether the user account should be active upon creation.
    /// Administrators can create inactive accounts for later activation.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether two-factor authentication should be enabled for this user.
    /// Administrators can enforce MFA for security-sensitive accounts.
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;

    /// <summary>
    /// Whether the user account should be subject to lockout policies.
    /// Administrators may disable lockout for service accounts.
    /// </summary>
    public bool LockoutEnabled { get; set; } = true;

    /// <summary>
    /// Subscription plan ID for the user.
    /// Administrators can assign specific subscription plans during creation.
    /// If not provided, the default subscription plan will be assigned.
    /// </summary>
    public int? SubscriptionPlanId { get; set; }

    /// <summary>
    /// The administrator performing this user creation for audit purposes.
    /// Must be the username or identifier of the authenticated administrator.
    /// </summary>
    [Required(ErrorMessage = "CreatedBy is required")]
    [StringLength(100, ErrorMessage = "CreatedBy must not exceed 100 characters")]
    public required string CreatedBy { get; set; }
}
