using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.udap_fhirserver_devdays>("udap-fhirserver-devdays");

builder.AddProject<Projects.udap_authserver_devdays>("udap-authserver-devdays");

builder.AddProject<Projects.udap_certificates_server_devdays>("udap-certificates-server-devdays");

builder.AddProject<Projects.udap_idp_server_devdays>("udap-idp-server-devdays");


var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithVolumeMount("../grafana/config", "/etc/grafana")
    .WithVolumeMount("../grafana/dashboards", "/var/lib/grafana/dashboards")
    .WithEndpoint(containerPort: 3000, hostPort: 3000, name: "grafana-http", scheme: "http");

builder.AddContainer("prometheus", "prom/prometheus")
    .WithVolumeMount("../prometheus", "/etc/prometheus")
    .WithEndpoint(9090, hostPort: 8889);


builder.Build().Run();
