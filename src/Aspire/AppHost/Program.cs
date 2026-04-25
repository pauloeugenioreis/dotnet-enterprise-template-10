var builder = DistributedApplication.CreateBuilder(args);

// Database Selection Logic
var dbType = builder.Configuration["DB_TYPE"]?.ToLowerInvariant() ?? "postgresql";

// Event Sourcing always needs PostgreSQL (Marten)
var pgEventsServer = builder.AddPostgres("events-server");
var eventsDb = pgEventsServer.AddDatabase("ProjectTemplateEvents");

IResourceBuilder<IResourceWithConnectionString> projectDb;

switch (dbType)
{
    case "sqlserver":
        projectDb = builder.AddSqlServer("sqlserver-server").AddDatabase("ProjectTemplateDb");
        break;
    case "mysql":
        projectDb = builder.AddMySql("mysql-server").AddDatabase("ProjectTemplateDb");
        break;
    case "oracle":
        projectDb = builder.AddOracle("oracle-server")
            .AddDatabase("FREEPDB1");
        break;
    case "postgresql":
    default:
        // Use the events server as the main server for PostgreSQL to save resources
        projectDb = pgEventsServer.AddDatabase("ProjectTemplateDb");
        dbType = "postgresql";
        break;
}

var redis = builder.AddRedis("redis");

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

var mongodb = builder.AddMongoDB("mongodb")
    .AddDatabase("ProjectTemplateMongo");

// Api Project
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(projectDb)
    .WithReference(eventsDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(mongodb)
    .WithEnvironment("AppSettings__Infrastructure__Database__DatabaseType", dbType)
    .WithEnvironment("AppSettings__Infrastructure__Database__ConnectionString", projectDb)
    .WithEnvironment("AppSettings__Infrastructure__EventSourcing__ConnectionString", eventsDb)
    .WithEnvironment("AppSettings__Infrastructure__MongoDB__ConnectionString", mongodb)
    .WithEnvironment("AppSettings__Infrastructure__Redis__ConnectionString", redis)
    .WithEnvironment("AppSettings__Infrastructure__RabbitMQ__ConnectionString", rabbitmq)
    .WithExternalHttpEndpoints();

// Web Projects
builder.AddProject<Projects.App>("blazor-app")
    .WithReference(api)
    .WithExternalHttpEndpoints();

// Modern Web Projects (Angular, React, Vue)
builder.AddNpmApp("angular-web", "../../UI/Web/Angular", "start")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithEnvironment("API_URL", api.GetEndpoint("https"))
    .WithExternalHttpEndpoints();

builder.AddNpmApp("react-web", "../../UI/Web/React", "dev")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithEnvironment("VITE_API_BASE_URL", "")
    .WithEnvironment("API_TARGET_URL", api.GetEndpoint("https"))
    .WithExternalHttpEndpoints();

builder.AddNpmApp("vue-web", "../../UI/Web/Vue", "dev")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithEnvironment("VITE_API_BASE_URL", "")
    .WithEnvironment("API_TARGET_URL", api.GetEndpoint("https"))
    .WithExternalHttpEndpoints();

builder.Build().Run();
