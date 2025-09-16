using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

Console.WriteLine("=== HoneyHub Local DB Deployer ===");

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var server = config["Server"] ?? "localhost,1433";
var user = config["User"] ?? "sa";
var password = config["Password"];
var encrypt = !bool.TryParse(config["Encrypt"], out var e) || e;
var trust = !bool.TryParse(config["TrustServerCertificate"], out var t) || t;
var buildConfig = config["Configuration"] ?? "Debug";

if (string.IsNullOrWhiteSpace(password))
{
    Fail("Password missing. Set in appsettings.json (Password).");
    Pause();
    return 1;
}

// candidate roots: domains, src, then solution root as fallback
var solutionRoot = FindSolutionRoot() ?? Directory.GetCurrentDirectory();
var roots = new List<string>();
var domainsPath = Path.Combine(solutionRoot, "domains");
var srcPath = Path.Combine(solutionRoot, "src");
if (Directory.Exists(domainsPath)) roots.Add(domainsPath);
if (Directory.Exists(srcPath)) roots.Add(srcPath);
if (roots.Count == 0) roots.Add(solutionRoot);

Console.WriteLine($"Solution root: {solutionRoot}");
Console.WriteLine("Search roots:");
foreach (var r in roots) Console.WriteLine($" - {r}");

var sqlprojFiles = roots
    .SelectMany(r => Directory.EnumerateFiles(r, "*.sqlproj", SearchOption.AllDirectories))
    .Distinct(StringComparer.OrdinalIgnoreCase)   // de-dupe
    .ToList();

if (sqlprojFiles.Count == 0)
{
    Warn("No .sqlproj files found under the search roots above.");
    Pause();
    return 1;
}

Console.WriteLine($"Found {sqlprojFiles.Count} SQL projects.");

foreach (var proj in sqlprojFiles)
{
    try
    {
        Console.WriteLine($"\n--- Processing {proj} ---");
        var dbName = GetTargetDbName(proj) ?? Path.GetFileNameWithoutExtension(proj);
        Console.WriteLine($"Database name: {dbName}");

        Console.WriteLine("Building project...");
        var dacpac = BuildDacpac(proj, buildConfig);
        Console.WriteLine($"Built dacpac: {dacpac}");

        var cs = $"Server={server};Database={dbName};User ID={user};Password={password};Encrypt={encrypt};TrustServerCertificate={trust}";
        Console.WriteLine("Deploying dacpac...");
        PublishDacpac(dacpac, cs);

        Success($"Published {Path.GetFileName(proj)} → {dbName}");
    }
    catch (Exception ex)
    {
        Fail($"Failed: {proj}\n{ex.Message}");
        Pause();
        return 1;
    }
}

Console.WriteLine("\n=== Done! All databases published successfully. ===");
Pause();
return 0;

// ----------------- helpers -----------------

static string? FindSolutionRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    for (int i = 0; i < 10 && dir is not null; i++, dir = dir.Parent)
    {
        if (dir.EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly).Any())
            return dir.FullName;
    }
    return null;
}

static string? GetTargetDbName(string sqlprojPath)
{
    var x = XDocument.Load(sqlprojPath);
    var names = x.Root?
        .Elements("PropertyGroup")
        .Elements("TargetDatabaseName")
        .Select(e => e.Value)
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .ToList();
    return names?.FirstOrDefault();
}

static bool UsesSdkStyle(string sqlprojPath)
{
    var x = XDocument.Load(sqlprojPath);
    return x.Root?.Attribute("Sdk") != null;
}

static string BuildDacpac(string sqlprojPath, string configuration)
{
    var projDir = Path.GetDirectoryName(sqlprojPath)!;
    var usesSdk = UsesSdkStyle(sqlprojPath);

    var exe = usesSdk ? "dotnet" : GetMsBuildPathOrFallback();
    var args = usesSdk
        ? $"build \"{sqlprojPath}\" -c {configuration}"
        : $"\"{sqlprojPath}\" /t:Build /p:Configuration={configuration}";

    var psi = new ProcessStartInfo(exe, args)
    {
        WorkingDirectory = projDir,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8
    };

    using var p = Process.Start(psi)!;
    var so = p.StandardOutput.ReadToEnd();
    var se = p.StandardError.ReadToEnd();
    p.WaitForExit();

    if (p.ExitCode != 0)
        throw new Exception($"Build failed for {sqlprojPath}\n{se}\n{so}");

    var binDir = Path.Combine(projDir, "bin", configuration);
    var dacpac = Directory.EnumerateFiles(binDir, "*.dacpac", SearchOption.AllDirectories)
                          .OrderByDescending(File.GetLastWriteTimeUtc)
                          .FirstOrDefault();

    return dacpac is null ? throw new Exception($"No .dacpac produced for {sqlprojPath}") : dacpac;
}

static void PublishDacpac(string dacpacPath, string connectionString)
{
    using var pkg = DacPackage.Load(dacpacPath);
    var dbName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
    var svc = new DacServices(connectionString);
    var opts = new DacDeployOptions
    {
        CreateNewDatabase = true,
        BlockOnPossibleDataLoss = false,
        AllowIncompatiblePlatform = true
    };
    svc.Deploy(pkg, dbName, true, opts);
}

static string GetMsBuildPathOrFallback()
{
    var vswhere = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        "Microsoft Visual Studio", "Installer", "vswhere.exe");

    if (File.Exists(vswhere))
    {
        var psi = new ProcessStartInfo
        {
            FileName = vswhere,
            Arguments = "-latest -requires Microsoft.Component.MSBuild -find MSBuild\\**\\Bin\\MSBuild.exe",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var p = Process.Start(psi)!;
        var path = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            return path;
    }

    var candidates = new[]
    {
        @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        @"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    };
    foreach (var c in candidates)
        if (File.Exists(c)) return c;

    return "msbuild"; // fallback: PATH
}

static void Pause()
{
    Console.WriteLine("\nPress any key to close...");
    Console.ReadKey(true);
}

static void Success(string msg)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"✔ {msg}");
    Console.ResetColor();
}

static void Warn(string msg)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"⚠ {msg}");
    Console.ResetColor();
}

static void Fail(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"✖ {msg}");
    Console.ResetColor();
}
