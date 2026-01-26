## Blogsphere.Webapp.Bff

Backend-for-Frontend (BFF) service for the Blogsphere Webapp. This repository hosts a versioned REST API that aggregates/mediates calls to downstream services (e.g., User API, API Gateway) and provides cross-cutting concerns like authentication, caching, logging, and observability.

### Table of contents

- [What this repository is](#what-this-repository-is)
- [Tech stack](#tech-stack)
- [Solution structure](#solution-structure)
- [API surface (current)](#api-surface-current)
- [Run locally](#run-locally)
- [Run with Docker](#run-with-docker)
- [Configuration](#configuration)
- [Adding new endpoints / features](#adding-new-endpoints--features)
- [Testing](#testing)
- [Load testing](#load-testing)
- [License](#license)

### What this repository is

- **Purpose**: Provide a stable API tailored to the Blogsphere Webapp, while hiding internal service topology and enabling composition (the “BFF” pattern).
- **Scope**: The solution is intentionally **evolving**. Expect more endpoints, providers, and features to be added over time.

### Tech stack

- **Runtime/SDK**: .NET 8 (`net8.0`)
- **API**: ASP.NET Core Web API + API Versioning
- **CQRS**: MediatR (+ FluentValidation where applicable)
- **Auth**: JWT Bearer authentication (authorization used on selected endpoints)
- **Caching**: Redis (StackExchange.Redis) + health checks
- **Observability**:
  - Structured logging with **Serilog**
  - Correlation via `CorrelationId` request header (generated if missing)
  - OpenTelemetry exporters (Jaeger/Zipkin/OTLP/console depending on environment)
- **API docs**: Swagger/OpenAPI (with examples and versioning helpers)

### Solution structure

The solution is organized in a clean-ish layering approach:

- **`src/Blogsphere.Webapp.Bff.API`**
  - ASP.NET Core entrypoint (`Program.cs`)
  - Versioned controllers under `Controllers/v1`
  - Middleware pipeline (correlation, request logging, global exception handling)
  - Swagger wiring and host settings
- **`src/Blogsphere.Webapp.Bff.Application`**
  - Application contracts (providers, caching, identity, token exchange)
  - CQRS features and handlers (MediatR)
  - Mapping/helpers used by the API layer
- **`src/Blogsphere.Webapp.Bff.Domain`**
  - Domain entities, DTOs, enums, and configuration option models
  - Shared API response/result models
- **`src/Blogsphere.Webapp.Bff.Infrastructure`**
  - Implementations for providers, caching, token exchange, identity helpers
  - HTTP client configuration and infrastructure DI
  - Health checks (e.g., Redis)
- **`src/Blogsphere.Webapp.Bff.Swagger`**
  - Swagger filters, helpers, and example payloads

### API surface (current)

All controllers inherit from `BaseApiController`, which standardizes:

- **Route shape**: `api/v{version}/[controller]`
- **Correlation**: reads `CorrelationId` header (or generates one)
- **User context**: builds a `RequestInformation` that includes current user (via `IIdentityService`)
- **Result mapping**: `OkOrFailure(...)` converts application `Result<T>` to HTTP responses

Endpoints currently present:

- **GET** `api/v1/status`
  - Returns the health status of the application.
- **GET** `api/v1/test` *(requires authentication)*
  - Development/testing endpoint used to exercise the CQRS + provider pipeline.
  - This endpoint is expected to evolve or be removed as real features are added.

### Run locally

Prerequisites:

- **.NET SDK 8**
- Optional local dependencies depending on features you’re working on:
  - Redis
  - Elasticsearch
  - Tracing backend (e.g., Jaeger)

From the repository root:

```bash
dotnet restore "src/Blogsphere.Webapp.Bff.sln"
dotnet run --project "src/Blogsphere.Webapp.Bff.API/Blogsphere.Webapp.Bff.API.csproj"
```

Notes:

- In `DEBUG`, the API is configured to listen on `http://localhost:5003`.
- Swagger/OpenAPI is enabled via the API project’s configuration and DI.

### Run with Docker

Docker Compose files live under `src/`:

- `src/docker-compose.yml`
- `src/docker-compose.override.yml`

The compose configuration expects an **external Docker network** called `blogsphere_dev_net`.
If it doesn’t exist yet, create it:

```bash
docker network create blogsphere_dev_net
```

Start the API container (from the repo root):

```bash
docker compose -f "src/docker-compose.yml" -f "src/docker-compose.override.yml" up --build
```

Ports:

- **Host** `8003` → **Container** `8080`

### Configuration

Configuration is loaded from:

- **`appsettings.json`** (baseline defaults)
- **Environment variables** (recommended for Docker and secrets)

Common settings (names match `appsettings.json` / env var overrides):

- **`ApiDescription` / `ApiOriginHost`**: API metadata / origin host used by Swagger configuration
- **`AppConfigurations__*`**: app identifier, environment, cache expiration, etc.
- **`Logging__*`**: Serilog behavior (console and optional Elasticsearch sink)
- **`ConnectionString__Redis`**: Redis connection string
- **`Elasticsearch__Uri`**: Elasticsearch endpoint (if enabled)
- **`IdentityGroupAccess__Authority` / `IdentityGroupAccess__Audience`**: auth/identity settings
- **`ProviderSettings__*`**: downstream API base URLs and credentials
- **`Jaeger__Host` / `Jaeger__Port`**: tracing exporter settings (if used)

Security / secrets:

- **Do not commit secrets** (client secrets, subscription keys, tokens).
- For Docker, secrets are already wired as environment variables in `docker-compose.override.yml`.
- For local development, prefer **environment variables** or **.NET user-secrets**.
  - If you use an `.env` file, keep it **out of git** (this repo already has `src/.env` for local use).

### Adding new endpoints / features

This repository is designed to grow. A typical flow to add a feature is:

- **API layer**
  - Add a versioned controller/action under `src/Blogsphere.Webapp.Bff.API/Controllers/v1`
  - Secure it with `[Authorize]` if needed
  - Return application results using `OkOrFailure(...)` for consistent response mapping
- **Application layer (CQRS)**
  - Add a feature folder under `src/Blogsphere.Webapp.Bff.Application/Features/...`
  - Add Query/Command + Handler using MediatR
  - Add validation (FluentValidation) where appropriate
- **Domain**
  - Add/extend entities/DTOs/enums and option models used by the feature
- **Infrastructure**
  - Add or extend provider implementations (HTTP clients, token exchange, caching)
  - Register new services in the appropriate DI extension methods
- **Swagger**
  - Add/extend examples and filters under `src/Blogsphere.Webapp.Bff.Swagger` as the API surface grows

Suggested documentation hygiene as you add features:

- Keep a running **“Endpoints”** list (method + route + short description) in this README.
- Add a short **“Design notes”** subsection for non-obvious decisions.
- Document any new required config keys under [Configuration](#configuration).

### Testing

Test projects live under `src/`:

- **Integration / endpoint tests**: `src/Blogsphere.Webapp.Bff.API.IntegrationTests`
  - In-memory API hosting via `WebApplicationFactory<Program>`
  - Test auth + deterministic health checks + fake downstream providers
- **Unit / business logic tests**: `src/Blogsphere.Webapp.Bff.Application.UnitTests`
  - CQRS handler tests with mocked providers and a real AutoMapper profile

Run all tests (from repo root):

```bash
dotnet test "src/Blogsphere.Webapp.Bff.sln"
```

Run a single test project:

```bash
dotnet test "src/Blogsphere.Webapp.Bff.API.IntegrationTests/Blogsphere.Webapp.Bff.API.IntegrationTests.csproj"
dotnet test "src/Blogsphere.Webapp.Bff.Application.UnitTests/Blogsphere.Webapp.Bff.Application.UnitTests.csproj"
```

### Load testing

There is a small k6 load test script under `src/loadtest/`.

Example:

```bash
k6 run "src/loadtest/test-controller.k6.js"
```

### License

Licensed under the Apache License 2.0. See `LICENSE`.