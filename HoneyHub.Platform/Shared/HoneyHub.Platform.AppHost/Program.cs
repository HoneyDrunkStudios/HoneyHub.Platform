using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ---------- Parameters ----------
var saPassword = builder.AddParameter("SqlSaPassword", secret: true);
var seqAdminPassword = builder.AddParameter("SeqAdminPassword", secret: true);
var grafanaAdminPassword = builder.AddParameter("GrafanaAdminPassword", secret: true);

// ---------- Local-only containers ----------
IResourceBuilder<ContainerResource>? seq = null;
IResourceBuilder<ContainerResource>? otel = null;

if (builder.Environment.IsDevelopment())
{
    // Seq (logs UI)
    seq = builder.AddContainer("seq", "datalust/seq:latest")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithEnvironment("SEQ_FIRSTRUN_ADMINPASSWORD", seqAdminPassword)
        .WithHttpEndpoint(name: "ui", port: 5341, targetPort: 80)
        .WithVolume("seq-data", "/data");

    // OpenTelemetry Collector
    // Config at: ./observability/otel-collector.yaml
    otel = builder.AddContainer("otel-collector", "otel/opentelemetry-collector-contrib:latest")
        .WithBindMount("./observability/otel-collector.yaml", "/etc/otelcol/config.yaml")
        .WithArgs("--config=/etc/otelcol/config.yaml")
        .WithHttpEndpoint(name: "otlpgrpc", port: 4317, targetPort: 4317)   // gRPC
        .WithHttpEndpoint(name: "otlphttp", port: 4318, targetPort: 4318);  // HTTP

    // Prometheus (scrapes collector’s :8889)
    // Config at: ./observability/prometheus.yaml
    builder.AddContainer("prometheus", "prom/prometheus:latest")
        .WithBindMount("./observability/prometheus.yaml", "/etc/prometheus/prometheus.yml")
        .WithHttpEndpoint(name: "ui", port: 9090, targetPort: 9090)
        .WithVolume("prom-data", "/prometheus");

    // Grafana (pre-provision Prometheus datasource)
    // Config at: ./observability/grafana-datasource.yaml
    builder.AddContainer("grafana", "grafana/grafana:latest")
      .WithBindMount("./observability/grafana-datasource.yaml", "/etc/grafana/provisioning/datasources/datasource.yaml")
      .WithBindMount("./observability/grafana-dashboards.yaml", "/etc/grafana/provisioning/dashboards/dashboards.yaml")
      .WithBindMount("./observability/dashboards", "/var/lib/grafana/dashboards")
      .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
      .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", grafanaAdminPassword)
      .WithHttpEndpoint(name: "ui", port: 3000, targetPort: 3000)
      .WithVolume("grafana-data", "/var/lib/grafana");


    // --- Tempo (stores traces locally; Grafana reads on :3200)
    //     Config at: ./observability/tempo.yaml
    builder.AddContainer("tempo", "grafana/tempo:latest")
        .WithBindMount("./observability/tempo.yaml", "/etc/tempo.yaml")
        .WithArgs("-config.file=/etc/tempo.yaml")
        .WithHttpEndpoint(name: "ui", port: 3200, targetPort: 3200)
        // Optional persistent storage
        .WithVolume("tempo-data", "/var/tempo");

    // --- Loki (stores logs locally; Grafana reads on :3100)
    //     Config at: ./observability/loki.yaml
    builder.AddContainer("loki", "grafana/loki:latest")
        .WithBindMount("./observability/loki.yaml", "/etc/loki/config.yaml")
        .WithArgs("-config.file=/etc/loki/config.yaml")
        .WithHttpEndpoint(name: "http", port: 3100, targetPort: 3100)
        .WithVolume("loki-data", "/loki");
}

// ---------- Helper to stamp common env + OTEL for each app ----------
IResourceBuilder<ProjectResource> WireApp(IResourceBuilder<ProjectResource> app)
{
    var env = builder.Environment.IsDevelopment() ? "Local" : "Dev";

    // Tier flag (Local/Dev/Prod)
    app.WithEnvironment("HONEYHUB_ENV", env);

    // DB connection (adjust per app if needed)
    app.WithEnvironment(
        "ConnectionStrings__DefaultConnection",
        $"Server=localhost,1433;Database=HoneyHub.Users.Database;User ID=sa;Password={saPassword};Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True");

    // Serilog base (console 0)
    app.WithEnvironment("Serilog__WriteTo__0__Name", "Console")
       .WithEnvironment("Serilog__WriteTo__0__Args__formatter", "Serilog.Formatting.Compact.RenderedCompactJsonFormatter, Serilog.Formatting.Compact");

    // Seq sink in local
    if (seq is not null)
    {
        app.WithEnvironment("Serilog__WriteTo__1__Name", "Seq")
           .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seq.GetEndpoint("ui"));
    }

    // ---------- OpenTelemetry ----------
    app.WithEnvironment("OTEL_SERVICE_NAME", app.Resource.Name) // e.g., "usersapi", "honeyhub-admin"
       .WithEnvironment("OTEL_RESOURCE_ATTRIBUTES", $"deployment.environment={env}")
       .WithEnvironment("OTEL_TRACES_SAMPLER", "parentbased_traceidratio")
       .WithEnvironment("OTEL_TRACES_SAMPLER_ARG", env == "Local" ? "1.0" : "0.2")
       .WithEnvironment("OTEL_DOTNET_EXPERIMENTAL_ENABLEOTLPLOGS", "true");

    // Point services at collector in local
    if (otel is not null)
    {
        app.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otel.GetEndpoint("otlpgrpc"));
        // If you want HTTP instead of gRPC:
        // app.WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf");
    }

    return app;
}

// ---------- Register your projects ----------
WireApp(builder.AddProject<Projects.HoneyHub_Users_Api>("usersapi"));
WireApp(builder.AddProject<Projects.HoneyHub_Admin>("honeyhub-admin"));

// ---------- Run ----------
builder.Build().Run();
