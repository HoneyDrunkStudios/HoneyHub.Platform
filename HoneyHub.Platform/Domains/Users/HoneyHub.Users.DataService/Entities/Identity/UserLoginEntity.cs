namespace HoneyHub.Users.DataService.Entities.Identity;

/// <summary>
/// External login entity aligned with dbo.UserLogin. Composite key (LoginProvider, ProviderKey).
/// </summary>
public class UserLoginEntity
{
    public required string LoginProvider { get; set; }
    public required string ProviderKey { get; set; }
    public string? ProviderDisplayName { get; set; }
    public int UserId { get; set; }

    // Navigation
    public required Users.UserEntity User { get; set; }
}
