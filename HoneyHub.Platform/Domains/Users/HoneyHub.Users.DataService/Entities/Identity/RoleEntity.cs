namespace HoneyHub.Users.DataService.Entities.Identity;

/// <summary>
/// Role entity aligned with dbo.Role table.
/// </summary>
public class RoleEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public required string ConcurrencyStamp { get; set; }

    // Navigation
    public ICollection<UserRoleEntity> Users { get; set; } = [];
    public ICollection<RoleClaimEntity> Claims { get; set; } = [];
}
