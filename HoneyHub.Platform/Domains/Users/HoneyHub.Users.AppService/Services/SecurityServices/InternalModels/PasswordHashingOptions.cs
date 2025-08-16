using Microsoft.Extensions.Options;

namespace HoneyHub.Users.AppService.Services.SecurityServices.InternalModels;

/// <summary>
/// Configuration options for password hashing using Argon2id algorithm.
/// Provides development and production presets with appropriate security/performance trade-offs.
/// </summary>
public class PasswordHashingOptions
{
    /// <summary>
    /// Number of parallel threads for Argon2id computation. 
    /// Should match or be less than available CPU cores.
    /// </summary>
    public int DegreeOfParallelism { get; set; }

    /// <summary>
    /// Number of iterations for Argon2id algorithm.
    /// Higher values increase security but reduce performance.
    /// </summary>
    public int Iterations { get; set; }

    /// <summary>
    /// Memory size in bytes for Argon2id computation.
    /// Higher values increase security but consume more memory.
    /// </summary>
    public int MemorySize { get; set; }

    /// <summary>
    /// Length of the resulting hash in bytes.
    /// Typical values are 16-32 bytes for adequate security.
    /// </summary>
    public int HashLength { get; set; }

    // Development-friendly settings (fast but less secure)
    public static readonly PasswordHashingOptions Development = new()
    {
        DegreeOfParallelism = 1,
        Iterations = 1,           // Minimal iterations for dev
        MemorySize = 8 * 1024,    // 8 KB instead of 32 KB
        HashLength = 16
    };

    // Production settings (secure but slower)
    public static readonly PasswordHashingOptions Production = new()
    {
        DegreeOfParallelism = Environment.ProcessorCount,
        Iterations = 4,
        MemorySize = 64 * 1024,   // 64 MB
        HashLength = 32
    };
}

/// <summary>
/// Validates PasswordHashingOptions configuration to ensure secure and functional parameters.
/// Prevents misconfiguration that could lead to security vulnerabilities or runtime errors.
/// </summary>
public class PasswordHashingOptionsValidator : IValidateOptions<PasswordHashingOptions>
{
    public ValidateOptionsResult Validate(string? name, PasswordHashingOptions options)
    {
        var failures = new List<string>();

        if (options.DegreeOfParallelism < 1)
            failures.Add("DegreeOfParallelism must be at least 1");

        if (options.DegreeOfParallelism > Environment.ProcessorCount * 2)
            failures.Add($"DegreeOfParallelism should not exceed {Environment.ProcessorCount * 2} (2x processor count)");

        if (options.Iterations < 1)
            failures.Add("Iterations must be at least 1");

        if (options.Iterations > 100)
            failures.Add("Iterations should not exceed 100 for reasonable performance");

        if (options.MemorySize < 1024)
            failures.Add("MemorySize must be at least 1024 bytes (1 KB)");

        if (options.MemorySize > 1024 * 1024 * 1024)
            failures.Add("MemorySize should not exceed 1 GB for reasonable memory usage");

        if (options.HashLength < 16)
            failures.Add("HashLength must be at least 16 bytes for adequate security");

        if (options.HashLength > 128)
            failures.Add("HashLength should not exceed 128 bytes for reasonable storage");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
