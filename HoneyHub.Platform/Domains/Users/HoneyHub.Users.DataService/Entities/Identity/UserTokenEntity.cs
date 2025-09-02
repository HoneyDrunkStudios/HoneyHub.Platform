namespace HoneyHub.Users.DataService.Entities.Identity;

/// <summary>
/// User token entity aligned with dbo.UserToken. Composite key (UserId, LoginProvider, Name).
/// </summary>
public class UserTokenEntity
{
    public int UserId { get; set; }
    public required string LoginProvider { get; set; }
    public required string Name { get; set; }
    public string? Value { get; set; }

    // Navigation
    public required Users.UserEntity User { get; set; }
}
