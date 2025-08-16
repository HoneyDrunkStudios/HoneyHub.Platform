using HoneyHub.Users.AppService.Services.SecurityServices.InternalModels;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace HoneyHub.Users.AppService.Services.SecurityServices;

/// <summary>
/// Application service responsible for secure password operations using Argon2id hashing algorithm.
/// Implements industry-standard cryptographic practices for password security.
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly PasswordHashingOptions _options;

    /// <summary>
    /// Initializes a new instance of PasswordService with validated configuration options.
    /// Follows Dependency Inversion Principle by depending on IOptions abstraction.
    /// </summary>
    /// <param name="options">Validated password hashing configuration options</param>
    public PasswordService(IOptions<PasswordHashingOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <summary>
    /// Generates a cryptographically secure random salt for password hashing.
    /// Uses 24 bytes (192 bits) of entropy for strong security.
    /// </summary>
    /// <returns>Base64-encoded salt string</returns>
    public string CreateSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(24);
        return Convert.ToBase64String(salt);
    }

    /// <summary>
    /// Hashes a password using Argon2id algorithm with the provided salt.
    /// Argon2id provides resistance against both side-channel and time-memory trade-off attacks.
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <param name="salt">Base64-encoded salt for the hash operation</param>
    /// <returns>Base64-encoded password hash</returns>
    /// <exception cref="ArgumentException">Thrown when password or salt is null or whitespace</exception>
    public string HashPassword(string password, string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(salt);

        var saltBytes = Convert.FromBase64String(salt);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = saltBytes,
            DegreeOfParallelism = _options.DegreeOfParallelism,
            Iterations = _options.Iterations,
            MemorySize = _options.MemorySize
        };

        return Convert.ToBase64String(argon2.GetBytes(_options.HashLength));
    }
}
