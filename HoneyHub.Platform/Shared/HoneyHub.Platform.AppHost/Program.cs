var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.HoneyHub_Platform_ApiService>("apiservice");

builder.AddProject<Projects.HoneyHub_Platform_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
