var builder = DistributedApplication.CreateBuilder(args);

// Projects
var api = builder.AddProject("gearhawk-api", "../GearHawk.API/GearHawk.API.csproj")
    // Inject Azure SQL connection string via environment
    .WithEnvironment("ConnectionStrings__GearHawk", builder.Configuration["ConnectionStrings:GearHawk"] ?? "");

_ = builder.AddProject("gearhawk-web", "../GearHawk.Web/GearHawk.Web.csproj")
    .WithReference(api); // service discovery: web depends on api

builder.Build().Run();
