namespace HoneyHub.Users.DataService.Entities.Identity;

/// <summary>
/// Join entity for many-to-many relation between User and Role (dbo.UserRole).
/// Composite key is (UserId, RoleId).
/// </summary>
public class UserRoleEntity
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    // Navigation
    public required Users.UserEntity User { get; set; }
    public required RoleEntity Role { get; set; }
}
