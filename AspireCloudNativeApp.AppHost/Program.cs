var builder = DistributedApplication.CreateBuilder(args);

// 3 types of resources: container, executable, and project
// * Container:
//      builder.AddContainer(name: "", image: "", tag: "");
// * Executable:
//      builder.AddExecutable(name: "", command: "", workingDirectory: "", args: ["", ""]);
// * Project:
//      builder.AddProject<Projects.AspireCloudNativeApp_ApiService>("apiservice");

var cache = builder
    .AddRedis("cache")
    .WithRedisCommander("redis-cmdr");

// Sample API container [from: https://github.com/dotnet/dotnet-docker/tree/main/samples/aspnetapp]
//
// When you add a container resource, .NET Aspire automatically assigns a
// random port to the container. To specify a container port, configure the
// container resource with the desired port:

var sampleApiContainer = builder
    .AddContainer(name: "sampleapi", image: "mcr.microsoft.com/dotnet/samples", tag: "aspnetapp")
    .WithHttpEndpoint(hostPort: 8000, containerPort: 8080, name: "sampleApiEndpoint");

var sampleApiEndpoint = sampleApiContainer
    .GetEndpoint("sampleApiEndpoint");

// * Creates a container resource named `sampleApi`, from the `mcr.microsoft.com/dotnet/samples:aspnetapp` image
// * Binds the host to port 8000 and the container port to 8080

// --------------------------------------------------------------------------------------------
// 🤬 - can't get SSL/TLS working locally with cosmos emulator
//var cosmosDb = builder.AddAzureCosmosDB("cosmos")
//    .UseEmulator(port: 8085);

//cosmosDb.AddDatabase("db");
// --------------------------------------------------------------------------------------------

// in Preview4 --> var db1 = builder.AddSqlServer("sql1").PublishAsAzureSqlDatabase().AddDatabase("db1");
//var sql = builder.AddSqlServer("sql");

// SQL Server container is configured with an auto-generated password by default
// but doesn't support any auto-creation of databases or running scripts on startup so we have to do it manually.
// To have a persistent volume mount across container instances, it must be named (VolumeMountType.Named).
// Using a persistent volume mount requires a stable password rather than the default generated one.

var sqlpw = builder.Configuration["sqlpassword"];
var sql = builder.AddSqlServerContainer("sql", sqlpw)
    .WithVolumeMount("VolumeMount.sqlserver.data", "/var/opt/mssql", VolumeMountType.Named);

    //// Mount the init scripts directory into the container.
    //.WithVolumeMount("./sqlserverconfig", "/usr/config", VolumeMountType.Bind)
    //// Mount the SQL scripts directory into the container so that the init scripts run.
    //.WithVolumeMount("../DatabaseContainers.ApiService/data/sqlserver", "/docker-entrypoint-initdb.d", VolumeMountType.Bind)
    //// Run the custom entrypoint script on startup.
    //.WithArgs("/usr/config/entrypoint.sh");

// Add the database to the application model so that it can be referenced by other resources.
sql.AddDatabase("sqldb");        

var weatherApi = builder
    .AddProject<Projects.AspireCloudNativeApp_WeatherApi>("weatherapi")
    //.WithReference(cosmosDb)
    .WithReference(sql);

builder
    .AddProject<Projects.AspireCloudNativeApp_Web>("webfrontend")
    .WithReference(cache)
    .WithReference(weatherApi)
    .WithReference(sampleApiEndpoint);

// webfrontend Environment variable: services__sampleApi__0 = endpoint://localhost:65256    

builder.Build().Run();