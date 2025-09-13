using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// keep the SA password out of source (set via user-secrets on AppHost project)
var saPassword = builder.AddParameter("SqlSaPassword", secret: true);

// Users API points to your existing local SQL (Docker Desktop) on localhost:1433
builder.AddProject<Projects.HoneyHub_Users_Api>("usersapi")
    .WithEnvironment(
        "ConnectionStrings__DefaultConnection",
        $"Server=localhost,1433;Database=HoneyHub.Users.Database;User ID=sa;Password={saPassword};Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True");

builder.Build().Run();
