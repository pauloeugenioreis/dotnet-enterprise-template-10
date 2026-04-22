var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure Resources
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("ProjectTemplateDb");

var redis = builder.AddRedis("redis");

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

var mongodb = builder.AddMongoDB("mongodb")
    .AddDatabase("ProjectTemplateMongo");

// Api Project
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(mongodb)
    .WithEndpoint(endpointName: "https", callback: endpoint => endpoint.Port = 7196)
    .WithEndpoint(endpointName: "http", callback: endpoint => endpoint.Port = 5125)
    .WithExternalHttpEndpoints();

// Web Projects
builder.AddProject<Projects.App>("blazor-app")
    .WithReference(api)
    .WithExternalHttpEndpoints();

// Modern Web Projects (Angular, React, Vue)
builder.AddNpmApp("angular-web", "../UI/Web/Angular", "start")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.AddNpmApp("react-web", "../UI/Web/React", "dev")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https"))
    .WithExternalHttpEndpoints();

builder.AddNpmApp("vue-web", "../UI/Web/Vue", "dev")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https"))
    .WithExternalHttpEndpoints();

builder.Build().Run();
