---
name: "aspire-resource-dependencies"
description: "Model Aspire resource dependencies so application startup waits on the correct infrastructure boundary"
domain: "aspire, orchestration"
confidence: "high"
source: "observed"
---

## Context
Use this when wiring infrastructure resources in an Aspire AppHost, especially when a container server resource also exposes child resources such as databases. The right `WaitFor()` target is the resource that represents what the consuming app actually needs to use.

## Patterns
- Keep infrastructure server resources and child resources in separate variables.
- Use `WithReference()` for the specific child resource the app consumes, such as a database connection.
- Use `WaitFor()` for the child resource the app consumes when Aspire documents that child resource as the availability boundary, such as `PostgresDatabaseResource`.
- Let Aspire generate development credentials unless the password is intentionally supplied from configuration or secrets.
- Prefer explicit variable names like `postgresServer` and `postgresDatabase` so the dependency graph is obvious during review.

## Examples
```csharp
var postgresServer = builder.AddPostgres("postgres");
var postgresDatabase = postgresServer.AddDatabase("DefaultConnection", "nutridash");

builder.AddProject<Projects.NutriDash_Web>("web")
    .WithReference(postgresDatabase)
    .WaitFor(postgresDatabase);
```

## Anti-Patterns
- Chaining `AddPostgres().AddDatabase()` into a single variable and then passing that variable to both `WithReference()` and `WaitFor()`.
- Assuming the parent server resource is always the correct `WaitFor()` target when the consumer actually depends on a child database resource.
- Declaring a secret parameter for a container password without also wiring a real configuration source for it.
- Using vague variable names that hide whether a resource is a server or a database.
