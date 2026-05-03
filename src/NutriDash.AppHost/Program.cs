using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder.AddPostgres("postgres")
    .WithImage("postgres")
    .WithImageTag("15");
var postgresDatabase = postgresServer.AddDatabase("DefaultConnection", "nutridash");

builder
    .AddProject<Projects.NutriDash_Web>("web")
    .WithReference(postgresDatabase)
    .WaitFor(postgresDatabase)
    .WithExternalHttpEndpoints();

var app = builder.Build();
await app.RunAsync();
