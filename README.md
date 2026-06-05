<div align="center">

# 📚 ELibrary

### Advanced Architectural Pet Project for Digital Library Management

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-5C2D91?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![EF Core](https://img.shields.io/badge/EF_Core-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![MediatR](https://img.shields.io/badge/MediatR-Pipeline-FF6B6B?style=for-the-badge)](https://github.com/jbogard/MediatR)
[![Serilog](https://img.shields.io/badge/Serilog-Structured_Logging-B5152B?style=for-the-badge)](https://serilog.net/)
[![FluentValidation](https://img.shields.io/badge/FluentValidation-Rules-37814A?style=for-the-badge)](https://fluentvalidation.net/)
[![ErrorOr](https://img.shields.io/badge/ErrorOr-Result_Pattern-FF8C00?style=for-the-badge)](https://github.com/amantinband/error-or)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)

---

*A high-level educational pet project built on **.NET 9** to explore and demonstrate clean development patterns: Clean Architecture, strict CQRS with MediatR pipelines, custom distributed caching/eviction, functional error handling, and automated database management.*

</div>

---

## Table of Contents

- [Overview](#-overview)
- [Clean Architecture Layer Breakdown](#️-clean-architecture-layer-breakdown)
- [Solution Directory Structure](#-solution-directory-structure)
- [MediatR Pipeline Behaviors — The Crown Jewel](#-mediatr-pipeline-behaviors--the-crown-jewel)
- [Functional Error Handling with ErrorOr](#-functional-error-handling-with-erroror)
- [Data Persistence & Auto-Migrations Pipeline](#️-data-persistence--auto-migrations-pipeline)
- [Local Environment Setup](#️-local-environment-setup)
- [API Endpoint Reference](#-api-endpoint-reference)

---

## 🔍 Overview

**ELibrary** is a high-level educational RESTful API backend designed to master clean architecture patterns around the core domain of a digital library system — **Authors**, **Books**, and **Categories** — demonstrating how an architecture scales gracefully and remains maintainable as requirements evolve, while avoiding basic student-level CRUD approaches entirely.

This project is not a CRUD tutorial. It is a carefully engineered system that demonstrates how every layer of a modern .NET backend — from the innermost domain entities to the outermost HTTP response formatting — can be designed with strict separation of concerns, automated cross-cutting behaviors, and zero tolerance for leaky abstractions.

**The key engineering pillars of this system are:**

- **Clean Architecture + DDD concepts** — business rules live in isolated layers that have zero knowledge of infrastructure, HTTP, or frameworks.
- **CQRS via MediatR** — every state-changing action is a `Command`; every read is a `Query`. They are never mixed.
- **Automated MediatR Pipeline Behaviors** — logging, validation, caching, and cache invalidation all happen automatically in the pipeline without a single line of repetitive code inside handlers.
- **Functional Result Pattern with ErrorOr** — no exception-driven control flow for business failures. Errors are first-class domain values returned explicitly.
- **Intelligent Distributed Caching** — queries opt into caching via a marker interface; mutations opt into cache eviction via another. The pipeline handles the rest.
- **Structured Logging via Serilog** — every request and response is captured with rich metadata, written asynchronously to both console and daily rolling log files.
- **Automated Database Migration** — the application self-manages its schema at startup with zero developer intervention required.

---

## 🏗️ Clean Architecture Layer Breakdown

The solution is organized into four distinct projects, each with a single, well-defined responsibility. The dependency rule is enforced at the project reference level: inner layers know nothing about outer layers.

```
Dependency Flow (strictly enforced):

  WebApi  ──────►  Application  ──────►  Domain
    │                                       ▲
    └──────►  Persistence  ─────────────────┘
```

No arrow ever points inward toward `WebApi` or `Persistence` from the core layers. This is enforced not by convention, but by .NET project references — if you accidentally try to reference `WebApi` from `Domain`, the build fails.

---

### Layer 1 — `ELibrary.Domain` (The Core)

**Project type:** Class Library (no framework references)

This is the innermost ring of the architecture. It contains the raw business concepts of the system, completely free of any framework dependency — no Entity Framework, no ASP.NET Core, no MediatR.

**`Entity.cs`** — The abstract base class for all domain entities. It carries the primary key (`Guid Id`) and serves as the type-safe root of the entity hierarchy. Using a `Guid` instead of a database-generated integer is a deliberate choice: it makes entity identity deterministic at the application layer and safe for distributed systems.

**`IAuditable.cs`** — A marker interface that enforces the audit trail contract. Any entity implementing `IAuditable` must expose `CreatedAt` and `UpdatedAt` timestamp properties. The `AppDbContext` intercepts `SaveChanges` and automatically populates these fields, removing the responsibility from every individual handler.

**`Author.cs`, `Book.cs`, `Category.cs`** — The core domain entities. Each inherits from `Entity` and carries only the properties that belong to the business domain. There is no navigation property bloat, no DTO mixing, and no validation annotation pollution.

> **Architectural Principle:** The Domain layer must be portable. You must be able to take it out of this solution, drop it into a console app, a test project, or a different framework entirely, and have it compile without modification. The moment a domain entity requires a framework to run, you have violated the boundary.

---

### Layer 2 — `ELibrary.Application` (The Orchestration Engine)

**Project type:** Class Library — references only `Domain`

This is where the business logic lives. It defines *what* the system can do through `Commands` and `Queries`, declares the contracts that infrastructure must implement through interfaces, and wires together the automated cross-cutting pipeline through MediatR behaviors.

**`Features/`** — Organized by domain aggregate (`Authors/`, `Books/`, `Categories/`), each containing a `Commands/` and `Queries/` folder. Every feature follows the vertical slice approach: a command or query is a self-contained unit with its own request class, response type, validator, and handler all co-located.

**`Common/Interfaces/`** — Contains the repository contracts (`IAuthorRepository`, `IBookRepository`, `ICategoryRepository`) and the `IUnitOfWork` abstraction. These are the ports of the Hexagonal Architecture pattern — the Application layer declares what it needs; the `Persistence` layer provides the adapters. The Application layer never knows that PostgreSQL exists.

Two special marker interfaces live here that power the caching pipeline:

- **`ICachableQuery`** — a query implements this to opt into automatic caching behavior. It exposes a `CacheKey` property and a `CacheDuration` that the `CachingBehaviour` uses to store and retrieve responses from `IDistributedCache`.
- **`IInvalidateCacheCommand`** — a command implements this to declare which cache keys should be evicted upon its successful execution. The `CacheInvalidationBehavior` reads the keys from this interface and removes them automatically.

**`Common/Behaviours/`** — The four pipeline behaviors that intercept every MediatR request. See the dedicated deep-dive section below.

> **Architectural Principle:** The Application layer may reference abstractions from the .NET BCL (like `ILogger<T>`, `IDistributedCache`) but must never reference a concrete implementation. `IDistributedCache` is fine. `StackExchange.Redis` is not.

---

### Layer 3 — `ELibrary.Persistence` (The Infrastructure Adapter)

**Project type:** Class Library — references `Application` and `Domain`

This layer is the concrete implementation of every contract defined in `Application`. It knows about PostgreSQL, Entity Framework Core, and migration management. The rest of the system does not.

**`AppDbContext.cs`** — The EF Core database context. It overrides `SaveChangesAsync` to intercept `IAuditable` entities and stamp `CreatedAt`/`UpdatedAt` automatically. It is configured with the design-time factory pattern, allowing `dotnet ef` CLI commands to work without a running host.

**`EntityTypeConfigurations/`** — Each entity has a dedicated configuration class (`AuthorConfiguration`, `BookConfiguration`, `CategoryConfiguration`) implementing `IEntityTypeConfiguration<T>`. This keeps the `AppDbContext` clean and single-responsibility: it maps `DbSet` properties but delegates all column naming, constraints, indexes, and relationship configurations to the dedicated configuration classes.

**`Repositories/`** — Concrete implementations of `IAuthorRepository`, `IBookRepository`, `ICategoryRepository`, and `IUnitOfWork`. They accept `AppDbContext` through constructor injection and translate domain operations into EF Core LINQ queries. Handlers talk to interfaces; repositories talk to EF Core. The handler never sees `DbSet`.

**`MigrationService.cs`** — A static infrastructure service that encapsulates the startup migration logic. It accepts an `IServiceProvider`, resolves the `AppDbContext` and `ILogger`, and executes `MigrateAsync()` within a guarded try/catch. Exceptions are wrapped in an `InvalidOperationException` and rethrown, causing the application host to halt immediately — a deliberate fail-fast strategy.

**`DependencyInjection.cs`** — A static extension method on `IServiceCollection` that registers all Persistence-layer services: the `AppDbContext` with its Npgsql connection, and all repository and Unit of Work implementations. Called once from `Program.cs`.

---

### Layer 4 — `ELibrary.WebApi` (The Delivery Mechanism)

**Project type:** ASP.NET Core Web API — references `Application` and `Persistence`

The outermost layer. It is responsible for one thing: translating HTTP requests into MediatR commands/queries and translating the results back into HTTP responses. It has no business logic. It has no data access. It is a thin delivery mechanism.

**`Controllers/ApiController.cs`** — The abstract base controller from which all feature controllers inherit. It encapsulates the `ISender` MediatR interface and provides the `HandleResult<T>(ErrorOr<T> result)` method — a single, reusable mapping function that converts `ErrorOr` success values into `Ok(value)` and failure values into RFC 7807 `ProblemDetails` responses.

**`DataExtensions.cs`** — An extension method on `IApplicationBuilder` that calls `MigrationService.ApplyMigrationsAsync` at startup. This cleanly separates the ASP.NET Core-specific wiring (needing `IApplicationBuilder`) from the pure migration logic (which only needs `IServiceProvider`).

**`Program.cs`** — The application entry point. It registers services, builds the middleware pipeline, triggers migrations, and starts the host. Clean, declarative, and free of implementation details.

---

## 📁 Solution Directory Structure

```
ELibrary/
├── global.json                          # Pins .NET SDK version for reproducible builds
├── ELibrary.slnx                        # Visual Studio Solution file
├── .gitignore
│
└── src/
    ├── Core/
    │   ├── ELibrary.Domain/             # ① Innermost ring — pure business concepts
    │   │   ├── Common/
    │   │   │   └── IAuditable.cs        # Audit trail marker interface
    │   │   ├── Entities/
    │   │   │   ├── Entity.cs            # Abstract base with Guid Id
    │   │   │   ├── Author.cs
    │   │   │   ├── Book.cs
    │   │   │   └── Category.cs
    │   │   └── GlobalUsings.cs
    │   │
    │   └── ELibrary.Application/        # ② Orchestration — CQRS, pipeline, interfaces
    │       ├── Common/
    │       │   ├── Behaviours/
    │       │   │   ├── LoggingBehaviour.cs          # Outer-most: timing & metadata
    │       │   │   ├── CacheInvalidationBehavior.cs # Evicts stale cache on mutations
    │       │   │   ├── CachingBehaviour.cs          # Cache hit/miss for queries
    │       │   │   └── ValidationBehavior.cs        # FluentValidation gate
    │       │   └── Interfaces/
    │       │       ├── IAuthorRepository.cs
    │       │       ├── IBookRepository.cs
    │       │       ├── ICategoryRepository.cs
    │       │       ├── IUnitOfWork.cs
    │       │       ├── ICachableQuery.cs            # Opt-in caching marker
    │       │       └── IInvalidateCacheCommand.cs   # Opt-in eviction marker
    │       ├── Features/
    │       │   ├── Authors/
    │       │   │   ├── Commands/        # CreateAuthor, UpdateAuthor, DeleteAuthor
    │       │   │   └── Queries/         # GetAuthor, GetAllAuthors
    │       │   ├── Books/
    │       │   │   ├── Commands/
    │       │   │   └── Queries/
    │       │   └── Categories/
    │       │       ├── Commands/
    │       │       └── Queries/
    │       ├── DependencyInjection.cs
    │       └── GlobalUsings.cs
    │
    ├── Infrastructure/
    │   └── ELibrary.Persistence/        # ③ Infrastructure adapter — EF Core + PostgreSQL
    │       ├── Context/
    │       │   └── AppDbContext.cs      # EF Core DbContext with audit interception
    │       ├── EntityTypeConfigurations/
    │       │   ├── AuthorConfiguration.cs
    │       │   ├── BookConfiguration.cs
    │       │   └── CategoryConfiguration.cs
    │       ├── Migrations/              # Auto-generated EF Core migrations
    │       ├── Repositories/
    │       │   ├── AuthorRepository.cs
    │       │   ├── BookRepository.cs
    │       │   ├── CategoryRepository.cs
    │       │   └── UnitOfWork.cs
    │       ├── DependencyInjection.cs
    │       ├── GlobalUsings.cs
    │       └── MigrationService.cs      # Startup migration executor
    │
    └── Presentation/
        └── ELibrary.WebApi/             # ④ Delivery mechanism — HTTP in, HTTP out
            ├── Controllers/
            │   ├── ApiController.cs     # Abstract base with ErrorOr mapping
            │   ├── AuthorController.cs
            │   ├── BookController.cs
            │   └── CategoryController.cs
            ├── Properties/
            │   └── launchSettings.json
            ├── .env                     # Runtime secrets (gitignored)
            ├── appsettings.json
            ├── appsettings.Development.json
            ├── DataExtensions.cs        # IApplicationBuilder migration extension
            ├── GlobalUsings.cs
            └── Program.cs
```

---

## ⚡ MediatR Pipeline Behaviors — The Crown Jewel

The single most architecturally significant feature of ELibrary is its **automated MediatR pipeline**. Cross-cutting concerns — logging, validation, caching, cache invalidation — are not implemented in handlers. They are not utility methods called manually. They are **pipeline behaviors** that execute automatically for every request that passes through MediatR.

This means a handler that creates an Author looks like this:

```csharp
public async Task<ErrorOr<AuthorResponse>> Handle(
    CreateAuthorCommand request,
    CancellationToken cancellationToken)
{
    var author = new Author(request.Name, request.Biography);
    await _repository.AddAsync(author);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return author.ToResponse();
}
```

No logging calls. No validation calls. No cache clearing calls. The pipeline does all of it.

---

### The Execution Pipeline

Every request travels through the following sequence. The registered order is critical and intentional:

```
HTTP Request
     │
     ▼
┌─────────────────────────────────────────┐
│         LoggingBehaviour                │  ← outermost: sees everything
│  ┌───────────────────────────────────┐  │
│  │    CacheInvalidationBehavior      │  │  ← only active for IInvalidateCacheCommand
│  │  ┌─────────────────────────────┐  │  │
│  │  │     CachingBehaviour        │  │  │  ← only active for ICachableQuery
│  │  │  ┌───────────────────────┐  │  │  │
│  │  │  │  ValidationBehavior   │  │  │  │  ← innermost gate before handler
│  │  │  │  ┌─────────────────┐  │  │  │  │
│  │  │  │  │    Handler      │  │  │  │  │  ← pure business logic
│  │  │  │  └─────────────────┘  │  │  │  │
│  │  │  └───────────────────────┘  │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
     │
     ▼
HTTP Response
```

---

### `LoggingBehaviour<TRequest, TResponse>`

**Registration:** `cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>))` — applies to **every** MediatR request without exception.

**Position:** Outermost behavior. This is deliberate and critical. Because it wraps everything else in the pipeline, it captures:
- Validation failures thrown by `ValidationBehavior`
- Cache hit short-circuits from `CachingBehaviour`
- Business logic errors returned from handlers
- Unhandled exceptions from any inner behavior

**Behavior:**

```
→ Log: "Executing request: CreateAuthorCommand"
  → [Inner pipeline executes]
← Log: "Request CreateAuthorCommand completed in 47ms"
  (or)
← Log: [ERROR] "Request CreateAuthorCommand failed after 12ms" + exception details
```

It uses `Stopwatch` to measure elapsed execution time for every request, giving you free performance observability across the entire command/query surface.

`LogCritical` is used on failures because a failure in the pipeline isn't a warning — it means the system could not fulfill a business contract and the on-call team needs to know.

---

### `CacheInvalidationBehavior<TRequest, TResponse>`

**Registration:** `cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>))` — registered globally but only activates for requests implementing `IInvalidateCacheCommand`.

**Position:** Second from the outside. It runs *after* `LoggingBehaviour` has started timing but *before* `CachingBehaviour`, ensuring cache state is cleaned up on the way back out after a successful mutation.

**The `IInvalidateCacheCommand` contract:**

```csharp
public interface IInvalidateCacheCommand
{
    IEnumerable<string> CacheKeysToInvalidate { get; }
}
```

A command that creates a new book might implement it like this:

```csharp
public record CreateBookCommand(string Title, Guid AuthorId)
    : IRequest<ErrorOr<BookResponse>>, IInvalidateCacheCommand
{
    public IEnumerable<string> CacheKeysToInvalidate =>
        ["all-books", $"author-{AuthorId}-books"];
}
```

**Behavior:** The behavior calls `next()` first, allowing the handler to execute. Only on a **successful, error-free result** does it iterate through `CacheKeysToInvalidate` and call `IDistributedCache.RemoveAsync()` for each key.

```
→ Pass through to Handler
← Handler returns ErrorOr<T>
  If result.IsError: do nothing, stale cache is not a concern
  If result has value:
    → _cache.RemoveAsync("all-books")
    → _cache.RemoveAsync("author-{id}-books")
← Return result to LoggingBehaviour
```

This design is critical: **cache is only evicted when the mutation actually succeeded**. If the database write fails and the handler returns an error, the cache is left untouched — which is the correct behavior.

---

### `CachingBehaviour<TRequest, TResponse>`

**Registration:** `cfg.AddOpenBehavior(typeof(CachingBehaviour<,>))` — registered globally but only activates for requests implementing `ICachableQuery`.

**Position:** Third from outside, sitting between `CacheInvalidationBehavior` and `ValidationBehavior`.

**The `ICachableQuery` contract:**

```csharp
public interface ICachableQuery
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}
```

A query opts in like this:

```csharp
public record GetAllBooksQuery : IRequest<ErrorOr<List<BookResponse>>>, ICachableQuery
{
    public string CacheKey => "all-books";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
```

**Full Cache Hit / Miss flow:**

```
→ Check if TRequest implements ICachableQuery
  If NOT: pass through immediately (zero overhead)
  If YES:
    → _cache.GetAsync(request.CacheKey)
    
    [CACHE HIT]
      → Deserialize cached bytes to TResponse
      → Return immediately — Handler is NEVER called
    
    [CACHE MISS]
      → Call next() → Handler executes → DB query runs
      ← Receive ErrorOr<T> from Handler
      
      If result.IsError:
        → Do NOT cache — error responses must never be stored
        ← Return error result
      
      If result has value:
        → Serialize TResponse to bytes
        → _cache.SetAsync(CacheKey, bytes, absoluteExpiration: CacheDuration)
        ← Return result
```

The guard against caching error results is a subtle but important detail. Without it, a transient database error could poison the cache and cause every subsequent request to receive a cached failure response for the duration of the cache TTL.

---

### `ValidationBehavior<TRequest, TResponse>`

**Registration:** `cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))` — uses `AddBehavior` (not `AddOpenBehavior`) because it has a generic constraint (`where TResponse : IErrorOr`) that not all requests satisfy.

**Position:** Innermost behavior, the last gate before the handler.

**Behavior:**

```csharp
// If no validators are registered for this request, pass through
if (!validators.Any()) return await next(cancellationToken);

// Run all validators concurrently
var validationResults = await Task.WhenAll(
    validators.Select(v => v.ValidateAsync(context, cancellationToken)));

// Collect failures
var failures = validationResults.SelectMany(r => r.Errors).ToList();

// Short-circuit and return validation errors as ErrorOr failures
if (failures.Count > 0)
{
    var errors = failures.Select(f =>
        Error.Validation(code: f.PropertyName, description: f.ErrorMessage)).ToList();

    return (TResponse)(dynamic)errors;
}
```

Note that validators run **concurrently** via `Task.WhenAll`. For a request with multiple registered validators, they execute in parallel rather than sequentially — a meaningful performance win for complex validation scenarios.

Critically, validation failures are returned as `ErrorOr` failure values, **not exceptions**. The handler is never invoked. The `LoggingBehaviour` sees a clean result (no exception), and the controller receives a list of `Error.Validation` values which it maps to a `422 Unprocessable Entity` `ProblemDetails` response.

---

## 💼 Functional Error Handling with ErrorOr

ELibrary completely abandons exception-driven control flow for business logic outcomes. Instead, every command and query handler returns `ErrorOr<T>` — a discriminated union that is either a success value of type `T` or one-or-more `Error` domain objects.

**Why this matters:**

Exceptions are expensive (stack unwinding), invisible to the type system (callers don't know a method can fail without reading the source), and semantically wrong for business failures (a "book not found" is not an exceptional situation). The `ErrorOr` library makes failures first-class, explicit, and zero-overhead for the happy path.

**A handler returning a domain error:**

```csharp
public async Task<ErrorOr<BookResponse>> Handle(
    GetBookByIdQuery request,
    CancellationToken cancellationToken)
{
    var book = await _repository.GetByIdAsync(request.Id, cancellationToken);

    if (book is null)
        return Error.NotFound("Book.NotFound", $"Book with ID {request.Id} was not found.");

    return book.ToResponse();
}
```

**The base `ApiController` maps results:**

```csharp
protected IActionResult HandleResult<T>(ErrorOr<T> result)
{
    return result.Match(
        value => Ok(value),
        errors => Problem(errors)
    );
}

private IActionResult Problem(IEnumerable<Error> errors)
{
    var firstError = errors.First();

    var statusCode = firstError.Type switch
    {
        ErrorType.NotFound    => StatusCodes.Status404NotFound,
        ErrorType.Validation  => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Conflict    => StatusCodes.Status409Conflict,
        _                     => StatusCodes.Status500InternalServerError
    };

    return Problem(
        statusCode: statusCode,
        title: firstError.Description,
        extensions: new Dictionary<string, object?> { ["errors"] = errors });
}
```

A controller action is reduced to a single, elegant expression:

```csharp
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetById(Guid id)
{
    var result = await _sender.Send(new GetBookByIdQuery(id));
    return HandleResult(result);
}
```

The controller has no `if/else`, no `try/catch`, no status code arithmetic. It sends the query and handles the result. The `ApiController` base class absorbs all the mapping complexity once, for all controllers, forever.

**Error type to HTTP status mapping:**

| `ErrorType`        | HTTP Status Code           | Scenario                                  |
|--------------------|----------------------------|-------------------------------------------|
| `ErrorType.NotFound`   | `404 Not Found`        | Entity with given ID does not exist       |
| `ErrorType.Validation` | `422 Unprocessable Entity` | FluentValidation rule(s) failed       |
| `ErrorType.Conflict`   | `409 Conflict`         | Duplicate entity, optimistic concurrency  |
| `ErrorType.Failure`    | `500 Internal Server Error`| Unexpected domain or infrastructure failure |

All responses conform to **RFC 7807 Problem Details for HTTP APIs**, providing a consistent, machine-readable error contract for API consumers.

---

## 🗄️ Data Persistence & Auto-Migrations Pipeline

### Entity Framework Core Configuration

EF Core 9 is configured Code-First. The schema is derived from entity classes and their configurations — no SQL DDL is written by hand.

Each entity has an isolated `IEntityTypeConfiguration<T>` class that defines its table name, column types, constraints, indexes, and relationships:

```csharp
public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title).IsRequired().HasMaxLength(500);
        builder.HasOne(b => b.Author)
               .WithMany(a => a.Books)
               .HasForeignKey(b => b.AuthorId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

The `AppDbContext` discovers all configurations via `modelBuilder.ApplyConfigurationsFromAssembly(...)`, keeping the context class clean and free of hundreds of lines of fluent API calls.

### The Startup Migration Pipeline

Rather than requiring developers to manually run `dotnet ef database update` before every deployment or after every schema change, ELibrary implements a **fully automated startup migration pipeline** that executes on every application launch:

```
Application Start (Program.cs)
         │
         ▼
await app.ApplyMigrationsAsync()     ← DataExtensions.cs (WebApi)
         │
         ▼
Creates IServiceScope
         │
         ▼
MigrationService.ApplyMigrationsAsync(IServiceProvider)  ← MigrationService.cs (Persistence)
         │
         ├── Resolve AppDbContext
         ├── Resolve ILogger<AppDbContext>
         ├── Log: "Checking for pending migrations..."
         │
         ▼
context.Database.IsRelational()
         │  true
         ▼
await context.Database.MigrateAsync()
         │
         ├── [DB exists, all migrations applied] → no-op, instant
         ├── [DB exists, pending migrations]     → apply deltas
         └── [DB does not exist]                 → CREATE DATABASE + apply all
         │
         ▼
Log: "Migrations applied successfully!"
         │
         ▼
Application continues to serve requests
```

**Why this approach is architecturally clean:**

The migration logic lives entirely in `MigrationService` (Persistence layer) and only depends on `IServiceProvider` — no ASP.NET Core types. The `DataExtensions.cs` in `WebApi` provides a thin `IApplicationBuilder` extension that creates the scope and delegates to `MigrationService`. This means:

- `Persistence` layer contains migration logic ✓
- `WebApi` contains ASP.NET Core wiring ✓
- Neither layer pollutes the other's responsibility ✓
- If the migration fails, the application throws `InvalidOperationException` and halts — a deliberate **fail-fast strategy** that prevents a partially-initialized application from serving corrupted or inconsistent data.

---

## 🛠️ Local Environment Setup

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 9.0+ | Runtime and build toolchain |
| PostgreSQL | 15+ | Primary database |
| Git | Any | Source control |

### Step 1 — Clone the Repository

```bash
git clone https://github.com/your-org/ELibrary.git
cd ELibrary
```

### Step 2 — Configure the Environment File

Create the `.env` file inside `src/Presentation/ELibrary.WebApi/`:

```bash
touch src/Presentation/ELibrary.WebApi/.env
```

Populate it with your local PostgreSQL credentials:

```env
# PostgreSQL Connection
DB_HOST=localhost
DB_PORT=5432
DB_NAME=elibrary_db
DB_USER=postgres
DB_PASSWORD=your_password_here

# Connection String (assembled from above by DotNetEnv at runtime)
CONNECTION_STRING=Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}
```

> **Security Note:** The `.env` file is listed in `.gitignore` and must never be committed to source control. All secrets are loaded at runtime via `DotNetEnv` — they never appear in `appsettings.json` or any tracked configuration file.

### Step 3 — Restore Dependencies

```bash
dotnet restore ELibrary.slnx
```

### Step 4 — Build the Solution

```bash
dotnet build ELibrary.slnx --configuration Release
```

### Step 5 — Run the Application

```bash
dotnet run --project src/Presentation/ELibrary.WebApi/ELibrary.WebApi.csproj
```

On first launch, the application will:

1. Load the `.env` file via `DotNetEnv`
2. Connect to PostgreSQL
3. Automatically create the database if it does not exist
4. Apply all pending EF Core migrations
5. Begin serving requests

The Swagger UI will be available at: `https://localhost:{port}/swagger`

### Step 6 — Generate a New Migration (Development Only)

When you add or modify domain entities and need to update the schema:

```bash
dotnet ef migrations add YourMigrationName \
  --project src/Infrastructure/ELibrary.Persistence \
  --startup-project src/Presentation/ELibrary.WebApi \
  --output-dir Migrations
```

The next application launch will automatically apply it.

---

## 📊 API Endpoint Reference

All endpoints return `application/json`. Error responses conform to RFC 7807 `ProblemDetails`.

### Authors

| Method | Path | Description | Cached | Success | Error Codes |
|--------|------|-------------|--------|---------|-------------|
| `GET` | `/api/authors` | Retrieve all authors | ✅ Yes | `200 OK` | — |
| `GET` | `/api/authors/{id}` | Retrieve author by ID | ✅ Yes | `200 OK` | `404` |
| `POST` | `/api/authors` | Create a new author | ❌ Invalidates | `201 Created` | `422` |
| `PUT` | `/api/authors/{id}` | Update existing author | ❌ Invalidates | `200 OK` | `404`, `422` |
| `DELETE` | `/api/authors/{id}` | Delete author by ID | ❌ Invalidates | `204 No Content` | `404` |

### Books

| Method | Path | Description | Cached | Success | Error Codes |
|--------|------|-------------|--------|---------|-------------|
| `GET` | `/api/books` | Retrieve all books | ✅ Yes | `200 OK` | — |
| `GET` | `/api/books/{id}` | Retrieve book by ID | ✅ Yes | `200 OK` | `404` |
| `POST` | `/api/books` | Create a new book | ❌ Invalidates | `201 Created` | `422` |
| `PUT` | `/api/books/{id}` | Update existing book | ❌ Invalidates | `200 OK` | `404`, `422` |
| `DELETE` | `/api/books/{id}` | Delete book by ID | ❌ Invalidates | `204 No Content` | `404` |

### Categories

| Method | Path | Description | Cached | Success | Error Codes |
|--------|------|-------------|--------|---------|-------------|
| `GET` | `/api/categories` | Retrieve all categories | ✅ Yes | `200 OK` | — |
| `GET` | `/api/categories/{id}` | Retrieve category by ID | ✅ Yes | `200 OK` | `404` |
| `POST` | `/api/categories` | Create a new category | ❌ Invalidates | `201 Created` | `422` |
| `PUT` | `/api/categories/{id}` | Update existing category | ❌ Invalidates | `200 OK` | `404`, `422` |
| `DELETE` | `/api/categories/{id}` | Delete category by ID | ❌ Invalidates | `204 No Content` | `404` |

### HTTP Status Code Reference

| Code | Meaning | Trigger |
|------|---------|---------|
| `200 OK` | Request succeeded | GET, PUT with valid resource |
| `201 Created` | Resource created | POST with valid payload |
| `204 No Content` | Resource deleted | DELETE with valid ID |
| `404 Not Found` | Resource absent | ID does not exist in DB |
| `422 Unprocessable Entity` | Validation failed | FluentValidation rules rejected the payload |
| `409 Conflict` | State conflict | Duplicate, concurrency violation |
| `500 Internal Server Error` | Unexpected failure | Unhandled infrastructure error |

---

<div align="center">

Built with precision and care using Clean Architecture principles.

**[⬆ Back to top](#-elibrary)**

</div>
