namespace HoneyHub.Users.AppService.Services.SecurityServices.InternalModels;

public class PasswordHashingOptions
{
	public int DegreeOfParallelism { get; set; }
	public int Iterations { get; set; }
	public int MemorySize { get; set; }
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
