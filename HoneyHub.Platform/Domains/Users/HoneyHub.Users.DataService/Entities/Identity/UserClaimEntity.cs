namespace HoneyHub.Users.DataService.Entities.Identity;

/// <summary>
/// User claim entity aligned with dbo.UserClaim.
/// </summary>
public class UserClaimEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }

    // Navigation
    public required Users.UserEntity User { get; set; }
}
