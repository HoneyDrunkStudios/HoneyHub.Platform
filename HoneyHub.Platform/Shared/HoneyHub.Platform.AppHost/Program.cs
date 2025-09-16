using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Params (local secrets OK for Local only)
var saPassword = builder.AddParameter("SqlSaPassword", secret: true);
var seqAdminPassword = builder.AddParameter("SeqAdminPassword", secret: true);

// Seq only for Local
IResourceBuilder<ContainerResource>? seq = null;
if (builder.Environment.IsDevelopment()) // Local AppHost
{
    seq = builder.AddContainer("seq", "datalust/seq:latest")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithEnvironment("SEQ_FIRSTRUN_ADMINPASSWORD", seqAdminPassword)
        .WithHttpEndpoint(name: "ui", port: 5341, targetPort: 80)
        .WithVolume("seq-data", "/data");
}

IResourceBuilder<ProjectResource> WireApp(IResourceBuilder<ProjectResource> app)
{
    // Tell the app which tier it's in (Local/Dev/Prod). Local when AppHost runs.
    app.WithEnvironment("HONEYHUB_ENV", builder.Environment.IsDevelopment() ? "Local" : "Dev");

    // Common stuff (DB etc.)
    app.WithEnvironment(
        "ConnectionStrings__DefaultConnection",
        $"Server=localhost,1433;Database=HoneyHub.Users.Database;User ID=sa;Password={saPassword};Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True");

    // Serilog base config (console as 0)
    app.WithEnvironment("Serilog__WriteTo__0__Name", "Console")
       .WithEnvironment("Serilog__WriteTo__0__Args__formatter", "Serilog.Formatting.Compact.RenderedCompactJsonFormatter, Serilog.Formatting.Compact");

    // When Local, add Seq sink
    if (seq is not null)
    {
        app.WithEnvironment("Serilog__WriteTo__1__Name", "Seq")
           .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seq.GetEndpoint("ui"));
    }

    return app;
}

WireApp(builder.AddProject<Projects.HoneyHub_Users_Api>("usersapi"));
WireApp(builder.AddProject<Projects.HoneyHub_Admin>("honeyhub-admin"));

builder.Build().Run();
