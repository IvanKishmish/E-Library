<div align="center">

# 📚 E-Library API

### A production-ready RESTful API for digital library management

[![.NET](https://img.shields.io/badge/.NET_9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23_13-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-0078D4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/en-us/apps/aspnet)
[![EF Core](https://img.shields.io/badge/EF_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

[![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-blueviolet?style=flat-square)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Error Handling](https://img.shields.io/badge/Errors-ErrorOr_Pattern-red?style=flat-square)](https://github.com/amantinband/error-or)
[![Validation](https://img.shields.io/badge/Validation-FluentValidation-brightgreen?style=flat-square)](https://docs.fluentvalidation.net/)
[![Caching](https://img.shields.io/badge/Cache-IDistributedCache_(Redis--ready)-orange?style=flat-square)](https://learn.microsoft.com/en-us/dotnet/core/extensions/caching)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Key Features](#-key-features)
- [Database Schema](#-database-schema)
- [Getting Started](#-getting-started)
- [Environment Variables](#-environment-variables)
- [API Endpoints](#-api-endpoints)
- [Caching Layer](#-caching-layer)
- [Roadmap](#-roadmap)

---

## 🔍 Overview

**E-Library API** — is a backend service for managing a digital library, built on the principles of **Clean Architecture** with a focus on production-ready patterns and best practices.

The project demonstrates practical application of modern patterns: functional error handling through **ErrorOr**, automatic validation via **FluentValidation**, intelligent caching through **Cache-Aside**, and database-side filtering with **IQueryable**.

---

## 🛠 Tech Stack

| Layer | Technology |
|-----|-----------|
| **Runtime** | .NET 9 / C# 13 |
| **Web Framework** | ASP.NET Core |
| **ORM** | Entity Framework Core |
| **Database** | PostgreSQL |
| **Caching** | `IDistributedCache` (Memory → Redis-ready) |
| **Validation** | FluentValidation |
| **Error Handling** | ErrorOr |
| **Config** | `.env` files |

---

## 🏗 Architecture

The project implements **Clean Architecture** principles with clear separation of responsibilities:

```
┌─────────────────────────────────────────────────────┐
│                   Presentation Layer                 │
│              Controllers (ASP.NET Core)              │
│        DTO In ──► Validation (FluentValidation)      │
└───────────────────────┬─────────────────────────────┘
                        │ Validated Request
┌───────────────────────▼─────────────────────────────┐
│                  Application Layer                   │
│            Services (Service Pattern)                │
│    Business Logic + Cache-Aside + ErrorOr Results    │
└───────────────────────┬─────────────────────────────┘
                        │ Domain Operations
┌───────────────────────▼─────────────────────────────┐
│               Infrastructure Layer                   │
│          Repositories (via BaseService)              │
│           EF Core ──► PostgreSQL                     │
└─────────────────────────────────────────────────────┘
```

### 🔄 Request Flow

```
HTTP Request
    │
    ▼
[Controller]  ──► FluentValidation ──► (400 Bad Request if invalid)
    │
    ▼
[Service]     ──► Cache Check (IDistributedCache)
    │               │
    │               ├── HIT  ──► Return cached DTO
    │               │
    │               └── MISS ──► [Repository] ──► PostgreSQL
    │                               │
    │                               └──► Cache.Set() ──► Return DTO
    │
    ▼
ErrorOr<T>    ──► .IsError ? Problem() : Ok(value)
```

---

## ✨ Key Features

### 🔴 ErrorOr — Functional Error Handling

Instead of `try/catch` and unexpected exceptions — explicit error types through `ErrorOr<T>`. Each service method returns a result that either contains the value or an error. No exceptions go unhandled, and the flow is predictable.

```csharp
// Service returns ErrorOr<BookDto>
public async Task<ErrorOr<BookDto>> GetByIdAsync(Guid id)
{
    var book = await _repository.GetByIdAsync(id);
    if (book is null)
        return Errors.Book.NotFound;

    return book.ToDto();
}

// Controller handles result declaratively
var result = await _bookService.GetByIdAsync(id);
return result.Match(Ok, Problem);
```

---

### 🟢 FluentValidation — Automatic Validation

Validation of incoming DTOs occurs automatically before reaching the controller through a pipeline.

```csharp
public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0).LessThanOrEqualTo(9999);
        RuleFor(x => x.AuthorId).NotEmpty();
    }
}
```

---

### 🟠 Cache-Aside Pattern — Smart Caching

Implemented through `IDistributedCache` with Memory Cache by default and full Redis compatibility.

```
┌────────────────── Cache-Aside Flow ──────────────────┐
│                                                      │
│  Request ──► Cache? ──YES──► Return Cached Value     │
│                 │                                    │
│                NO                                    │
│                 │                                    │
│                 ▼                                    │
│            Database Query                            │
│                 │                                    │
│                 ▼                                    │
│         Cache.Set(key, value, ttl)                   │
│                 │                                    │
│                 ▼                                    │
│            Return Value                              │
│                                                      │
│  ─ ─ ─ ─ ─ Cache Invalidation ─ ─ ─ ─ ─ ─           │
│                                                      │
│  Mutating Op ──► Service ──► Repository              │
│  (Create/Update/Delete)          │                   │
│                                  ▼                   │
│                          Cache.Remove(key)           │
│                      (prevents stale data)           │
└──────────────────────────────────────────────────────┘
```

**Cache Keys Strategy:**
```
books:all              → list of all books
books:{id}             → specific book
books:filter:{hash}    → filter results
```

**Cache Invalidation** is triggered automatically on any data mutation (Create / Update / Delete), ensuring cache freshness and data consistency.

---

### 🔵 Flexible Filtering via IQueryable

Book filtering is performed **on the database side** via `IQueryable` — no unnecessary data is loaded into memory. All filters are combined into a single SQL query.

```csharp
// Supported filters:
GET /api/books?title=clean&minPrice=10&maxPrice=50&categoryId=...

// Implementation (executed as a single SQL query):
query = query
    .WhereIf(!string.IsNullOrEmpty(filter.Title),
        b => b.Title.ToLower().Contains(filter.Title.ToLower()))  // case-insensitive
    .WhereIf(filter.MinPrice.HasValue,
        b => b.Price >= filter.MinPrice!.Value)
    .WhereIf(filter.MaxPrice.HasValue,
        b => b.Price <= filter.MaxPrice!.Value)
    .WhereIf(filter.CategoryId.HasValue,
        b => b.CategoryId == filter.CategoryId!.Value);
```

---

## 🗄 Database Schema

```
┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│   Authors    │       │    Books     │       │  Categories  │
├──────────────┤       ├──────────────┤       ├──────────────┤
│ Id (PK)      │◄──┐   │ Id (PK)      │  ┌──► │ Id (PK)      │
│ FirstName    │   │   │ Title        │  │    │ Name         │
│ LastName     │   │   │ Description  │  │    │ Description  │
│ Bio          │   └───│ AuthorId(FK) │  │    └──────────────┘
│ CreatedAt    │       │ CategoryId(FK│──┘
└──────────────┘       │ Price        │
                       │ CreatedAt    │
                       └──────────────┘

Relationships: Book → Author (Many-to-One)
              Book → Category (Many-to-One)
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/your-username/e-library-api.git
cd e-library-api

# 2. Configure environment variables (details below)
cp .env.example .env
# Edit .env according to your configuration

# 3. Restore dependencies
dotnet restore

# 4. Apply database migrations
dotnet ef database update --project src/ELibrary.Infrastructure

# 5. Run the project
dotnet run --project src/ELibrary.API
```

API will be available at: `https://localhost:7001` / `http://localhost:5001`

Swagger UI: `https://localhost:7001/swagger`

---

## ⚙ Environment Variables

The project uses `.env` files for secure configuration storage. **Never commit `.env` to the repository!**

Create `.env` in the project root:

```env
# .env

# Database
POSTGRES_CONNECTION_STRING=Host=localhost;Port=5432;Database=elibrary;Username=postgres;Password=your_password

# Application
ASPNETCORE_ENVIRONMENT=Development

# Cache (for future Redis)
# REDIS_CONNECTION_STRING=localhost:6379
```

`.env.example` (safe version for the repository):

```env
# .env.example

POSTGRES_CONNECTION_STRING=Host=localhost;Port=5432;Database=elibrary;Username=YOUR_USER;Password=YOUR_PASSWORD
ASPNETCORE_ENVIRONMENT=Development
# REDIS_CONNECTION_STRING=localhost:6379
```

---

## 📡 API Endpoints

### Books

| Method | Endpoint | Description |
|-------|----------|------|
| `GET` | `/api/books` | List of books (with filtering) |
| `GET` | `/api/books/{id}` | Book by ID |
| `POST` | `/api/books` | Create book |
| `PUT` | `/api/books/{id}` | Update book |
| `DELETE` | `/api/books/{id}` | Delete book |

### Authors

| Method | Endpoint | Description |
|-------|----------|------|
| `GET` | `/api/authors` | List of authors |
| `GET` | `/api/authors/{id}` | Author by ID |
| `POST` | `/api/authors` | Create author |
| `PUT` | `/api/authors/{id}` | Update author |
| `DELETE` | `/api/authors/{id}` | Delete author |

### Categories

| Method | Endpoint | Description |
|-------|----------|------|
| `GET` | `/api/categories` | List of categories |
| `GET` | `/api/categories/{id}` | Category by ID |
| `POST` | `/api/categories` | Create category |
| `PUT` | `/api/categories/{id}` | Update category |
| `DELETE` | `/api/categories/{id}` | Delete category |

### Filter Parameters for `GET /api/books`

```
?title=string          — search by title (case-insensitive)
?minPrice=decimal      — minimum price
?maxPrice=decimal      — maximum price
?categoryId=guid       — filter by category
```

---

## 🗺 Roadmap

- [x] Clean Architecture + Service/Repository Pattern
- [x] ErrorOr functional error handling
- [x] FluentValidation pipeline
- [x] Cache-Aside Pattern (Memory Cache)
- [x] Cache Invalidation on mutations
- [x] IQueryable filtering (DB-side)
- [x] `.env` configuration
- [ ] 🐳 **Docker & Docker Compose** — containerization of API + PostgreSQL
- [ ] 🔴 **Redis Integration** — switching from Memory to Redis (architecture ready)
- [ ] 🔐 **JWT Authentication** — registration, login, secured endpoints
- [ ] 👤 **Role-based Authorization** — Admin / Reader roles
- [ ] 📄 **Pagination** — cursor-based pagination for lists
- [ ] 📊 **Logging** — structured logging via Serilog
- [ ] 🧪 **Unit & Integration Tests** — service layer coverage

---

<div align="center">

Made with ☕ and C#

</div>
