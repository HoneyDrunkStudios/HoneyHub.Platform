using HoneyHub.Users.AppService.Services.SecurityServices.InternalModels;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace HoneyHub.Users.AppService.Services.SecurityServices;

public class PasswordService : IPasswordService
{
	private readonly PasswordHashingOptions _options;

	public PasswordService(IConfiguration configuration)
	{
		var section = configuration.GetSection("PasswordHashing");
		_options = new PasswordHashingOptions
		{
			DegreeOfParallelism = int.TryParse(section["DegreeOfParallelism"], out var dop) ? dop : PasswordHashingOptions.Development.DegreeOfParallelism,
			Iterations = int.TryParse(section["Iterations"], out var iters) ? iters : PasswordHashingOptions.Development.Iterations,
			MemorySize = int.TryParse(section["MemorySize"], out var mem) ? mem : PasswordHashingOptions.Development.MemorySize,
			HashLength = int.TryParse(section["HashLength"], out var len) ? len : PasswordHashingOptions.Development.HashLength
		};
	}

	public string CreateSalt()
	{
		var salt = RandomNumberGenerator.GetBytes(24);
		return Convert.ToBase64String(salt);
	}

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
