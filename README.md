# SimpleBlog

A full-stack blogging platform with **authentication, role-based moderation, and notifications**, built with
**ASP.NET Core (Clean Architecture)** and **two frontends** — **Next.js** and **Blazor WebAssembly** — that
both consume the same REST API.

> Product spec: [docs/spec.md](docs/spec.md) · Development plan: [docs/development-plan.md](docs/development-plan.md)

## Stack
| Layer | Tech |
| --- | --- |
| API | ASP.NET Core 8, EF Core, SQL Server, ASP.NET Identity, JWT |
| Architecture | Core → Application → Infrastructure → Api (dependencies point inward) |
| Frontend A | Next.js 14 (App Router) + TypeScript + Tailwind CSS |
| Frontend B | Blazor WebAssembly (.NET 8) |
| Infra | Docker Compose (SQL Server + API + both frontends) |

## Project layout
```
SimpleBlog.sln
src/
  SimpleBlog.Core            # Entities, enums, domain constants
  SimpleBlog.Application     # DTOs, service interfaces + business logic, workflow rules
  SimpleBlog.Infrastructure  # EF Core, Identity, JWT, Redis cache, Google auth, seeding
  SimpleBlog.Api             # Controllers, auth, middleware, Swagger
  SimpleBlog.Worker          # Background worker: outbox -> admin notifications
tests/
  SimpleBlog.Tests           # xUnit tests (workflow + authorization)
frontend/
  nextjs/                    # Next.js frontend (professional, responsive)
  blazor/                    # Blazor WebAssembly frontend
docker-compose.yml
docs/                        # spec + development plan
```

## Features
- Email/password auth (JWT) **and Google OAuth** (`POST /api/auth/google`, validates a Google ID token).
- Roles: **Viewer**, **Blogger**, **Admin** — enforced server-side.
- Post workflow: `Draft → PendingReview → Published / Rejected`.
- Blogger dashboard (create/edit/submit, stats). Admin moderation queue (approve/reject).
- **Outbox pattern**: `PostSubmitted` events are written in the same transaction, then the **Worker**
  polls the outbox and notifies admins.
- **Redis cache-aside** on the published posts list (falls back to in-memory when Redis isn't configured).
- xUnit tests covering the workflow and authorization rules.
- Responsive, SaaS-style UI on both frontends.

## Demo accounts (seeded on first run)
| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@simpleblog.local` | `Admin123!$` |
| Blogger | `blogger@simpleblog.local` | `Blogger123!$` |

New sign-ups get the **Blogger** role automatically.

---

## Run locally (recommended for development)

### 1. Database
You need SQL Server reachable at `localhost,1433`. The quickest way:
```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```
The connection string lives in [src/SimpleBlog.Api/appsettings.json](src/SimpleBlog.Api/appsettings.json).
The database schema and seed data are created automatically on startup (`EnsureCreated`).

### 2. API
```powershell
dotnet run --project src/SimpleBlog.Api
```
- API: http://localhost:5080
- Swagger: http://localhost:5080/swagger

### 3a. Next.js frontend
```powershell
cd frontend/nextjs
copy .env.local.example .env.local
npm install
npm run dev
```
Open http://localhost:3000

### 3b. Blazor frontend (optional)
```powershell
cd frontend/blazor
dotnet run
```
Open the URL printed in the console. API base URL is configured in
[frontend/blazor/wwwroot/appsettings.json](frontend/blazor/wwwroot/appsettings.json).

### 4. Background worker (optional)
Processes the outbox and creates admin notifications when a post is submitted:
```powershell
dotnet run --project src/SimpleBlog.Worker
```

### 5. Tests
```powershell
dotnet test
```

### Optional: Redis & Google
- **Redis**: set `ConnectionStrings:Redis` (e.g. `localhost:6379`) in
  [src/SimpleBlog.Api/appsettings.json](src/SimpleBlog.Api/appsettings.json). When empty, an in-memory
  distributed cache is used automatically.
- **Google OAuth**: set `Google:ClientId` in the API config, then the Next.js login/register pages show a
  real Google button (set `NEXT_PUBLIC_GOOGLE_CLIENT_ID` in `.env.local`). The client sends the Google ID
  token to `POST /api/auth/google`.
- **Azure Service Bus** (optional): set `ConnectionStrings:ServiceBus` (or `ServiceBus:ConnectionString`)
  and `ServiceBus:QueueName` for both the API and the Worker. When configured, the Worker **relays** outbox
  events to the queue and a **consumer** turns them into notifications. When left empty, the Worker handles
  events in-process — no broker required.

---

## Run everything with Docker
```powershell
docker compose up --build
```
| Service | URL |
| --- | --- |
| Next.js | http://localhost:3000 |
| Blazor | http://localhost:5000 |
| API | http://localhost:5080/swagger |
| Worker | (no port — background service) |
| SQL Server | localhost:1433 |
| Redis | localhost:6379 |

---

## API overview
| Method | Route | Access |
| --- | --- | --- |
| POST | `/api/auth/register` · `/api/auth/login` | Anonymous |
| POST | `/api/auth/google` | Anonymous |
| GET | `/api/auth/me` | Authenticated |
| GET | `/api/posts` · `/api/posts/{id}` · `/api/posts/slug/{slug}` | Anonymous (published) |
| GET | `/api/posts/mine` | Authenticated |
| GET | `/api/posts/pending` | Admin |
| POST | `/api/posts` | Blogger, Admin |
| PUT | `/api/posts/{id}` | Owner / Admin |
| POST | `/api/posts/{id}/submit` | Owner / Admin |
| POST | `/api/posts/{id}/approve` · `/reject` | Admin |
| GET | `/api/categories` | Anonymous |
| GET | `/api/notifications` | Authenticated |

## Notes
- To switch from `EnsureCreated` to migrations: add EF Core migrations
  (`dotnet ef migrations add InitialCreate -p src/SimpleBlog.Infrastructure -s src/SimpleBlog.Api`)
  and change `EnsureCreatedAsync()` to `MigrateAsync()` in
  [DbSeeder](src/SimpleBlog.Infrastructure/Persistence/DbSeeder.cs).
- Redis caching, Azure Service Bus and the background Worker are described in the spec and are the next
  phases in the development plan.
- Keep secrets out of source: override `Jwt:Key` and the connection string via environment variables or
  user-secrets in real deployments.