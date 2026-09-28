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
  SimpleBlog.Infrastructure  # EF Core, Identity, JWT, seeding
  SimpleBlog.Api             # Controllers, auth, middleware, Swagger
frontend/
  nextjs/                    # Next.js frontend (professional, responsive)
  blazor/                    # Blazor WebAssembly frontend
docker-compose.yml
docs/                        # spec + development plan
```

## Features
- Email/password auth (JWT). "Continue with Google" is stubbed in the UI, ready to wire up.
- Roles: **Viewer**, **Blogger**, **Admin** — enforced server-side.
- Post workflow: `Draft → PendingReview → Published / Rejected`.
- Blogger dashboard (create/edit/submit, stats). Admin moderation queue (approve/reject).
- Author notifications on approve/reject; Outbox row written on submit.
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
| SQL Server | localhost:1433 |

---

## API overview
| Method | Route | Access |
| --- | --- | --- |
| POST | `/api/auth/register` · `/api/auth/login` | Anonymous |
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