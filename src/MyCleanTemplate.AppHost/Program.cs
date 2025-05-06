var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyCleanTemplate_API>("mycleantemplate-api");
builder.AddContainer("Prometheus", "prom/prometheus").WithEndpoint(9090, 9090);
builder.Build().Run();
