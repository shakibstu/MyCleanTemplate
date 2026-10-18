# MyCleanTemplate

A sample **Clean Architecture** Web API on **.NET 10**. It shows one way to put the usual pieces together: CQRS with a mediator, domain events, EF Core, JWT authentication with permission-based authorization and MFA, OpenTelemetry tracing, compile-time dependency injection, and tests that check traces and architecture rules.

> **This is a sample / starting point, not a production-ready service.** Secrets are committed, an admin user with a known password is seeded, and the `Test*` feature exists only to demonstrate the patterns. See [Before you build on it](#before-you-build-on-it).

## Contents

- [Tech stack](#tech-stack)
- [Solution layout](#solution-layout)
- [Getting started](#getting-started)
- [Configuration](#configuration)
- [How a request flows](#how-a-request-flows)
- [Adding a feature](#adding-a-feature)
- [Dependency injection via source generator](#dependency-injection-via-source-generator)
- [Authentication and authorization](#authentication-and-authorization)
- [Paging](#paging)
- [Observability](#observability)
- [Error handling](#error-handling)
- [Testing](#testing)
- [Build settings and packages](#build-settings-and-packages)
- [Using it as a `dotnet new` template](#using-it-as-a-dotnet-new-template)
- [Before you build on it](#before-you-build-on-it)

## Tech stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10, ASP.NET Core controllers, nullable reference types everywhere |
| Mediator / CQRS | [MediatRR](https://www.nuget.org/packages/MediatRR) (requests, pipeline behaviors, background notifications) |
| Validation | FluentValidation, run as a mediator pipeline behavior |
| Persistence | EF Core 10 + SQLite, ASP.NET Core Identity stores |
| Auth | JWT bearer, permission-based policies, TOTP MFA |
| DI | Custom Roslyn source generator (attribute-driven registration, Castle DynamicProxy interception) |
| Observability | OpenTelemetry traces, metrics and logs over OTLP, viewed in the .NET Aspire dashboard |
| Orchestration | .NET Aspire AppHost, Docker Compose |
| Tests | xUnit v3 + `WebApplicationFactory` (integration), NetArchTest (architecture), MSTest (unit), Shouldly |

## Solution layout

```
src/
  MyCleanTemplate.Domain                Entities, value objects, domain events, repository interfaces, exceptions
  MyCleanTemplate.Application.Contract  Commands, queries, DTOs and their validators (the public use-case surface)
  MyCleanTemplate.Application           Handlers, pipeline behaviors, ports such as IIdentityService
  MyCleanTemplate.Infrastructure        EF Core DbContext, repositories, migrations, seeders, Identity and JWT
  MyCleanTemplate.API                   Controllers, middleware, exception handler, Program.cs
  MyCleanTemplate.ServiceDefaults       Aspire service defaults (health checks, service discovery, resilience)
  MyCleanTemplate.AppHost               Aspire AppHost that runs the API with the dashboard
  MyCleanTemplate.SourceGenerator       Roslyn generator for DI registration and interception
  MyCleanTemplate.Common                Small shared helpers
  SourceGTest                           Scratch project for trying out the source generator
test/
  MyCleanTemplate.IntegrationTest       End-to-end tests against the real pipeline with in-memory SQLite
  MyCleanTemplate.ArchitectureTest      Layering rules enforced with NetArchTest
  MyCleanTemplate.UnitTest              Unit test project (placeholder)
```

Dependencies point inwards:

```
API ──► Infrastructure ──► Application ──► Application.Contract ──► Domain
```

Domain has no dependency on the outer layers. Application defines what it needs from the outside world as interfaces (for example `ITestRepository` in Domain, `IIdentityService` in Application), and Infrastructure implements them. `MyCleanTemplate.ArchitectureTest` fails if a layer starts depending on an outer one.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Optional: Docker, for the Compose setup

### Run the API

```bash
dotnet run --project src/MyCleanTemplate.API
```

Then open Swagger at <https://localhost:7022/swagger>.

On startup the API applies any pending EF Core migrations to `app.db` (SQLite) and runs the data seeders, so there is nothing to set up. The seeders create:

- one sample `Test` entity
- an `Admin` role holding every admin permission
- an `Admin` user with password `Admin123!`

### Run with the Aspire dashboard

```bash
dotnet run --project src/MyCleanTemplate.AppHost
```

The console prints the dashboard URL. The AppHost points the API's OTLP exporter at the dashboard, so traces, metrics and structured logs show up there without further setup.

### Run with Docker Compose

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| API (HTTP) | <http://localhost:3215/swagger> |
| Aspire dashboard | <http://localhost:18888> |

Compose runs the [standalone Aspire dashboard](https://learn.microsoft.com/dotnet/aspire/fundamentals/dashboard/standalone) and points the API's OTLP exporter at it, so you get the same traces, metrics and logs view as with the AppHost. Login is turned off for local use.

The dashboard's OTLP endpoint is also published on `localhost:4317`, which is the exporter's default. So you can start only the dashboard and run the API from your IDE:

```bash
docker compose up aspire-dashboard
dotnet run --project src/MyCleanTemplate.API
```

### Database migrations

Migrations live in `src/MyCleanTemplate.Infrastructure/Migrations`. To add one:

```bash
dotnet ef migrations add <Name> -p src/MyCleanTemplate.Infrastructure -s src/MyCleanTemplate.API
```

You don't need to run `database update`; the API applies migrations at startup.

## Configuration

Configuration uses the standard ASP.NET Core sources: `appsettings.json`, `appsettings.{Environment}.json`, environment variables and user secrets.

| Setting | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | EF Core connection string (SQLite by default) |
| `Jwt:Issuer`, `Jwt:Audience` | Values written to and checked on every token |
| `Jwt:SigningKey` | HMAC signing key, at least 32 characters |
| `Jwt:TokenExpirationMinutes` | Token lifetime (default 60) |
| `Jwt:ClockSkewSeconds` | Allowed clock difference when validating expiry (default 30) |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Where telemetry is sent (default `http://localhost:4317`) |

The `Jwt` section is validated at startup, so a missing or too-short key stops the app immediately with a clear error.

To use another database, change the connection string and the EF Core provider in `InfrastructureServiceRegistration`, then regenerate the migrations.

## How a request flows

Using `POST /api/test/CreateTest` as the example:

1. **Controller**: `TestController` maps the HTTP model to a `CreateTestCommand` and calls `IMediator.Send`. Controllers contain no business logic.
2. **Pipeline behaviors**: `RequestTracingBehavior` opens a span with the request and response serialized as tags. `ValidationBehavior` runs the FluentValidation validators from `Application.Contract` and throws a `ValidationException` if any fail.
3. **Handler**: `TesCreateCommandHandler` calls `ITestRepository.CreateTest`.
4. **Repository**: `TestRepository` (Infrastructure) is wrapped in a Castle proxy by `LogRepositoryInterceptor`, which records a span with the method name, arguments and result. The repository raises a `TestAddedEvent` on the entity and commits.
5. **Domain events**: `ApplicationDbContext.SaveChangesAsync` publishes the entity's domain events through the mediator.
6. **Notification handlers**: MediatRR queues the notification and runs its handlers on a background worker, in their own DI scope. The trace context travels in `BaseEvent.Properties`, so the handler span links back to the original request.

## Adding a feature

Using an `Order` feature as the example:

1. **Domain**: add the `Order` entity (deriving from `BaseEntity`), any domain events, and an `IOrderRepository : IBaseRepository<Order>`.
2. **Application.Contract**: add `CreateOrderCommand : ICommand<Guid>` (or `IQuery<T>` for reads) and an `AbstractValidator<CreateOrderCommand>`.
3. **Application**: add `CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>`. Request handlers are registered automatically.
4. **Infrastructure**: add an `IEntityTypeConfiguration<Order>` under `Persistence/Configurations`, a `DbSet` in `ApplicationDbContext.Dbsets.cs`, and an `OrderRepository : BaseRepository<Order>, IOrderRepository`. It inherits registration and tracing from `BaseRepository`.
5. **API**: add a controller action that sends the command.
6. **Migration**: run `dotnet ef migrations add AddOrders ...`.

Notification handlers are **not** auto-registered. Add them in `ApplicationServiceRegistration`:

```csharp
services.AddNotificationHandler<OrderPlaced, SendConfirmationHandler>(retryPolicy: null);
```

Nullable reference types are on everywhere, which affects two things:

- **EF Core** treats non-nullable properties as required columns. Use `string?` (or `.IsRequired(false)`) for optional ones.
- **MVC** treats non-nullable properties on request models as required. Optional fields on request models must be nullable, or the request is rejected with 400.

## Dependency injection via source generator

There's no assembly scanning at runtime. `MyCleanTemplate.SourceGenerator` reads attributes at compile time and generates an `InjectFromSourceGenerator()` extension per project. The layer registration methods call it (`AddInfrastructureServices`, `AddApplicationServices`, `AddDomainServices`).

| Attribute | Effect |
|---|---|
| `[ScopedDependency<IFoo>]` / `[TransientDependency<IFoo>]` / `[SingletonDependency<IFoo>]` | Registers the class as `IFoo` with that lifetime |
| `[TransientDependency(typeof(IPipelineBehavior<,>))]` | Open-generic registration (used for the pipeline behaviors) |
| `[InheritableTransientInterceptInterfaces<TInterceptor>]` | On a base class: every derived class is registered by its interfaces, wrapped in a Castle proxy with `TInterceptor` |
| `[Inheritable{Scoped,Transient,Singleton}<T>]`, `...Interfaces`, `...Interception<T>` | Variations of the above for inherited registration and interception |

Example from the codebase:

```csharp
[ScopedDependency<IIdentityService>]
internal class IdentityService(...) : IIdentityService { ... }

[InheritableTransientInterceptInterfaces<LogRepositoryInterceptor>]
internal abstract class BaseRepository<T>(ApplicationDbContext context) : IBaseRepository<T> { ... }
```

## Authentication and authorization

### Signing in

```http
POST /api/auth/SignIn
{ "username": "Admin", "password": "Admin123!" }
```

```json
{ "token": "eyJ...", "expiresIn": 3600, "tokenType": "Bearer" }
```

Send the token as `Authorization: Bearer <token>`. Swagger has an **Authorize** button for this.

The JWT carries the user's roles and permissions as claims. Five failed attempts (wrong password or wrong MFA code) lock the account for a few minutes; these are the ASP.NET Core Identity defaults.

### Roles and permissions

Use a named policy for role checks:

```csharp
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
```

For fine-grained access, use permissions. A permission is a string `Permission.{Service}.{Feature}.{Action}`, for example `Permission.Identity.Users.Create`. All permissions are declared in `Infrastructure/Authentication/AppPermission.cs`. A user's effective permissions are the union of their roles' permissions and their own.

Protect an endpoint with:

```csharp
[MustHavePermission(AppService.Identity, AppFeature.Users, AppAction.Create)]
```

`PermissionPolicyProvider` builds the policy on demand, so you don't need to register policies by hand.

### Multi-factor authentication (TOTP)

1. Sign in with the password, then call `GET /api/auth/GetMFAKey`. You get a base32 key to add to an authenticator app. The key is only returned while MFA is not yet enabled.
2. Confirm with `POST /api/auth/VerifyMFAKey` and `{ "securityCode": "123456" }`. This enables MFA for the current user.
3. From then on, sign-in also needs the code: `{ "username": "...", "password": "...", "token": "123456" }`. A password alone returns `401`.

Endpoints marked `[RequireMfa]` (for example `POST /api/auth/CreateProtectedResource`) accept only tokens issued after an MFA check.

## Paging

`Application.Contract` defines `PageRequest` (page, page size up to 200, optional sort column and direction) and `Page<T>` (items, total count, page metadata). In Infrastructure, `ToPageAsync` turns any `IQueryable<T>` into a page:

```csharp
var page = await context.Tests.AsNoTracking().ToPageAsync(request, cancellationToken);
```

## Observability

- **Traces**: one custom span per layer. `LogRequestResponseMiddleware` creates an API span that carries the request and response bodies. `RequestTracingBehavior` and the notification behaviors create mediator spans, and `LogRepositoryInterceptor` creates repository spans. EF Core instrumentation adds the SQL commands underneath.
- **Metrics**: ASP.NET Core, HttpClient and runtime instrumentation, plus the app's own meter (`TelemetryHelper.Meter`).
- **Logs**: sent through the OpenTelemetry logging provider. Only `Warning` and above are exported (see the filter in `Program.cs`).
- **Exporter**: OTLP, configured with the standard `OTEL_EXPORTER_OTLP_ENDPOINT` variable. If it's unset, it defaults to `http://localhost:4317`.
- **Health checks** (Development only):
  - `/alive`: the process is up
  - `/ready`: the app can reach the database
  - `/health`: every check passes

## Error handling

Throw one of the domain exceptions from `Domain/Exceptions/Base` and `GlobalExceptionHandler` turns it into a `ProblemDetails` response with the matching status code:

| Exception | Status |
|---|---|
| `BadRequestException`, `ValidationException` | 400 |
| `UnAuthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `NotFoundException` | 404 |
| anything else | 500 |

The `errors` dictionary you pass to the exception is returned in the response.

## Testing

```bash
dotnet test
```

- **`MyCleanTemplate.ArchitectureTest`** checks the layering rules with NetArchTest. For example, Application must not reference Infrastructure or the API, and controllers must live in the API project.
- **`MyCleanTemplate.IntegrationTest`** starts the real API with `WebApplicationFactory`, replacing the database with in-memory SQLite. Techniques worth looking at:
  - **Asserting on traces**: an in-memory OpenTelemetry exporter collects spans per test run, so tests can check what happened, not just the HTTP response (see `TestTest.CreateTest`).
  - **Asserting on background notifications**: `NotificationProbe` records events that reached a handler. Tests wait on it because MediatRR handlers run on a background worker (see `DomainEventTest`).
  - **Auth flow**: `AuthTest` covers MFA enrollment, sign-in with and without a code, and account lockout. It generates TOTP codes itself.
  - **Direct mediator calls**: `BaseIntegrationTest` exposes `Sender` (an `IMediator`) for sending queries and commands without HTTP.
- **`MyCleanTemplate.UnitTest`** is a placeholder for fast, isolated tests.

## Build settings and packages

- [`Directory.Build.props`](Directory.Build.props) applies to every project. It sets the latest C# version, nullable reference types, implicit usings and the latest analyzers, and enforces the code style from [`.editorconfig`](.editorconfig) during build.
- [`Directory.Packages.props`](Directory.Packages.props) holds every package version (central package management). Project files list `<PackageReference Include="..." />` without versions. To upgrade a package, change it there.

## Using it as a `dotnet new` template

The repository includes a [`.template.config`](.template.config/template.json), so it can be installed as a solution template. `MyCleanTemplate` is replaced with your project name throughout:

```bash
dotnet new install .
dotnet new Shaklean -n Contoso.Orders
```

To remove it again: `dotnet new uninstall <path-to-this-folder>`.

## Before you build on it

Things that are fine in a sample but need changing in a real service:

- **Secrets**: the JWT signing key is a placeholder in `appsettings.json` and a fixed value in `docker-compose.override.yml`. Move it to user-secrets or a secret store.
- **Seeded admin**: `UserRoleSeeder` creates `Admin` / `Admin123!` in every environment.
- **Database**: SQLite with migrations applied at startup. For a real deployment, use a server database and run migrations as a deployment step.
- **Sample code**: `TestEntity`, `TestController`, the `Test*` handlers (which log at error level on purpose) and `SourceGTest` exist only for demonstration.
- **Payloads in traces**: `LogRequestResponseMiddleware` and the tracing behaviors put full request and response payloads on spans, including passwords on `SignIn`. Redact or turn them off for anything sensitive.
- **License**: no license file is included yet. Add one before publishing.
