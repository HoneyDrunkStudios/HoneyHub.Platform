var builder = DistributedApplication.CreateBuilder(args);

// Register only the Users API for now since other projects are not present in this solution
builder.AddProject<Projects.HoneyHub_Users_Api>("honeyhub-users-api")
       .WithExternalHttpEndpoints();

builder.Build().Run();
