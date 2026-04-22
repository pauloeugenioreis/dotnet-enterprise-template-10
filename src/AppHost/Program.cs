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
    .WithExternalHttpEndpoints();

// Web Projects
builder.AddProject<Projects.App>("blazor-app")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
