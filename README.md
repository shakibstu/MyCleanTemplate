# MyCleanTemplate

A .NET 10 Clean Architecture solution template for building Web APIs. Installable as a `dotnet new` template (`Shaklean`), it ships with sensible defaults for layering, dependency injection (via a custom source generator), persistence, observability, and containerization.

## Features

- **Clean Architecture layering** — `Domain`, `Application`, `Infrastructure`, `API` with `*.Contract` boundaries.
- **.NET 10** with nullable reference types and implicit usings.
- **.NET Aspire AppHost** for local orchestration and service discovery (`MyCleanTemplate.AppHost`).
- **EF Core** persistence (`ApplicationDbContext`) with migrations and interceptors.
- **DI source generator** (`MyCleanTemplate.SourceGenerator`) — register services by attribute (`[Singleton]`, `[Scoped]`, `[Transient]`) instead of hand-wiring each one.
- **OpenTelemetry** traces, metrics, and logs exported via OTLP (Jaeger-ready).
- **Global exception handling** and request/response logging middleware.
- **Swagger / OpenAPI** in Development.
- **Docker** support (`Dockerfile` + `docker-compose`).
- **Unit and integration test** projects pre-wired.

## Solution Layout

```
MyCleanTemplate/
├── src/
│   ├── MyCleanTemplate.API                   # HTTP entry point, controllers, middleware
│   ├── MyCleanTemplate.AppHost               # .NET Aspire orchestrator
│   ├── MyCleanTemplate.Application           # Use cases / application services
│   ├── MyCleanTemplate.Application.Contract  # Application-layer interfaces & DTOs
│   ├── MyCleanTemplate.Domain                # Entities, value objects, domain events
│   ├── MyCleanTemplate.Infrastructure        # EF Core, messaging, external concerns
│   ├── MyCleanTemplate.Common                # Cross-cutting helpers
│   ├── MyCleanTemplate.ServiceDefaults       # Aspire service defaults (telemetry, health)
│   └── MyCleanTemplate.SourceGenerator       # Roslyn generator for DI registration
├── test/
│   ├── MyCleanTemplate.UnitTest
│   └── MyCleanTemplate.IntegrationTest
├── docker-compose.yml
└── MyCleanTemplate.sln
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker (optional, for `docker-compose` or container debugging)

## Getting Started

### Option A — Run the solution directly

```bash
git clone <repo-url>
cd MyCleanTemplate
dotnet restore
dotnet build
dotnet run --project src/MyCleanTemplate.API
```

The API is exposed on the ports defined in `launchSettings.json` / `docker-compose.override.yml`. Swagger UI is available at `/swagger` in Development.

### Option B — Run via .NET Aspire

```bash
dotnet run --project src/MyCleanTemplate.AppHost
```

This brings up the API together with any configured Aspire resources and opens the Aspire dashboard.

### Option C — Docker Compose

```bash
docker compose up --build
```

The API is published on `http://localhost:3215` and a Jaeger UI is available on `http://localhost:16686`.

## Install as a `dotnet new` Template

The repository is also a [template package](.template.config/template.json). To install it locally:

```bash
dotnet new install .
dotnet new shaklean -n MyNewService
```

`shortName: shaklean` and `sourceName: MyCleanTemplate` mean the template engine will rename projects and namespaces to match the value you pass to `-n`.

To uninstall:

```bash
dotnet new uninstall <path-to-this-folder>
```

## Configuration

Configuration follows the standard ASP.NET Core layering (`appsettings.json`, `appsettings.{Environment}.json`, environment variables, user secrets).

Key connection strings:

| Setting                              | Purpose                                  |
| ------------------------------------ | ---------------------------------------- |
| `ConnectionStrings:DefaultConnection`| EF Core database connection              |
| `ConnectionStrings:Jeager`           | OTLP endpoint for traces (e.g. Jaeger)   |

In Docker the SQLite default (`Data Source=app.db;`) is used; swap in SQL Server or Postgres by changing the connection string and the EF Core provider in `InfrastructureServiceRegistration`.

## Dependency Injection via Source Generator

Decorate a class with one of the lifetime attributes from `MyCleanTemplate.SourceGenerator` and it will be picked up automatically when `InjectFromSourceGenerator()` is called in `Program.cs`:

```csharp
[Scoped(typeof(IMyService))]
public sealed class MyService : IMyService { /* ... */ }
```

Supported attributes: `[Singleton]`, `[Scoped]`, `[Transient]`.

## Observability

OpenTelemetry is wired in `Program.cs` for:

- **Traces** — ASP.NET Core, HttpClient, EF Core
- **Metrics** — runtime, ASP.NET Core, HttpClient
- **Logs** — Microsoft logging bridged to OTLP

Set `ConnectionStrings:Jeager` to your collector's OTLP gRPC endpoint (default in compose: `http://jeager:4317`).

## Tests

```bash
dotnet test
```

- **`MyCleanTemplate.UnitTest`** — fast, isolated unit tests.
- **`MyCleanTemplate.IntegrationTest`** — boots the API in-process (`WebApplicationFactory`) for end-to-end coverage.

## License

No license file is currently included. Add one before publishing.
