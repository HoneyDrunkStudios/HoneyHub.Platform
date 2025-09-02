namespace HoneyHub.Users.DataService.Entities.Identity;

/// <summary>
/// Role claim entity aligned with dbo.RoleClaim.
/// </summary>
public class RoleClaimEntity
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }

    // Navigation
    public required RoleEntity Role { get; set; }
}
