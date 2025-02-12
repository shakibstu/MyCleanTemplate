var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyCleanTemplate_API>("mycleantemplate-api");

builder.Build().Run();
