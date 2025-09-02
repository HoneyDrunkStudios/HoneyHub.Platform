namespace HoneyHub.Users.DataService.Entities.Identity;

/// <summary>
/// Refresh token entity aligned with dbo.RefreshToken. Stores only hashes.
/// </summary>
public class RefreshTokenEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public required string TokenHash { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReasonRevoked { get; set; }

    // Navigation
    public required Users.UserEntity User { get; set; }
}
