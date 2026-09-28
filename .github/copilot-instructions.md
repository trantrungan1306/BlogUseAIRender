# SimpleBlog – Copilot Instructions

## Project overview
SimpleBlog is a full product: **Blog + CMS + Moderation + Authentication + Notification System**.
It is built as a Clean Architecture .NET solution with two frontends (Blazor and Next.js) that both
talk to a single ASP.NET Core REST API. No frontend ever touches the database directly.

```
Frontend (Blazor / Next.js)
    ↓ HTTP (REST)
ASP.NET Core API
    ↓
Application (business logic)
    ↓
Infrastructure (EF Core, Identity, Redis, Service Bus)
    ↓
Database (SQL Server)
```

## Solution structure
| Project | Responsibility |
| --- | --- |
| `SimpleBlog.Core` | Domain: Entities, Enums, Interfaces, ValueObjects, domain rules |
| `SimpleBlog.Application` | Features, DTOs, Commands, Queries, Services, Validators |
| `SimpleBlog.Infrastructure` | EF Core, SQL Server, Identity, Google auth, Redis, Azure Service Bus, Repositories, Email |
| `SimpleBlog.Api` | Controllers, AuthN/AuthZ, Middleware, Swagger, exception handling |
| `SimpleBlog.Blazor` | Blazor (.NET 10) frontend |
| `SimpleBlog.NextJs` | Next.js (React) frontend |
| `SimpleBlog.Worker` | .NET background worker consuming the message queue |
| `SimpleBlog.Tests` | Unit + Integration tests |
| `docker-compose.yml` | Orchestrates all services |

## Dependency rules (Clean Architecture)
- `Core` depends on nothing.
- `Application` depends only on `Core`.
- `Infrastructure` depends on `Application` + `Core`.
- `Api` depends on `Application` + `Infrastructure` (composition root only).
- Frontends depend on the API over HTTP — never reference `Infrastructure` or the database.
- Reference interfaces from `Core`/`Application`; inject concrete implementations from `Infrastructure`.

## Domain model
- Entities: `User` (`ApplicationUser`), `Post`, `Category`, `Comment`, `Review`, `Notification`, `OutboxMessage`.
- `ApplicationUser`: `Id`, `Email`, `DisplayName`, `GoogleId`, `AvatarUrl`, `CreatedAt`.
- `PostStatus` enum: `Draft`, `PendingReview`, `Published`, `Rejected`.
- Roles: `Viewer`, `Blogger`, `Admin`.

## Blog workflow (enforce in Application layer)
```
Draft → (Submit) → PendingReview → (Approve) → Published
                                 → (Reject)  → Rejected
```
- **Viewer**: read published posts only.
- **Blogger**: create/edit own posts, submit, view own posts. Cannot approve/reject/publish.
- **Admin**: manage users & posts, approve, reject, publish.

## Authentication
- Two options: email/password **and** Continue with Google (OAuth 2.0 / OpenID Connect).
- Use ASP.NET Core Identity. Persist Google users with `GoogleId`.
- Always apply role-based authorization on API endpoints. Never trust the client for role checks.

## Messaging & reliability
- Use the **Outbox pattern**: write domain events to an `OutboxMessage` table in the same DB transaction.
- The Worker polls the outbox and publishes to Azure Service Bus, then consumers create notifications.
- `OutboxMessage`: `Id`, `Type`, `Payload`, `CreatedAt`, `ProcessedAt`, `RetryCount`.
- Event example: `{ "eventType": "PostSubmitted", "postId": 123, "authorId": 456 }`.

## Caching
- Redis is added after the basic version is stable.
- Use cache-aside for read-heavy endpoints (e.g. `GET /api/posts`). Invalidate on writes.
- Redis may also back rate limiting, distributed locks, and session-related data.

## API conventions
- REST, JSON, versionable routes under `/api`.
- Representative endpoints:
  - `POST /api/auth/login`, `POST /api/auth/google`
  - `GET /api/posts`, `GET /api/posts/{id}`, `POST /api/posts`, `PUT /api/posts/{id}`
  - `POST /api/posts/{id}/submit|approve|reject`
- Return DTOs, never EF entities. Validate input with validators in the Application layer.
- Centralize error handling in middleware; return consistent problem-details responses.

## Coding conventions
- Backend: C# / .NET 10, nullable enabled, async/await for I/O, DI everywhere.
- Frontend (Next.js): TypeScript, `app/` router, typed API clients in `services/`, shared `types/`.
- Frontend (Blazor): component-based pages under Home / Dashboard / Admin areas.
- UI should look like a real SaaS product, not a tutorial.

## Testing
- Unit tests: Application, Domain, Services.
- Integration tests: API.
- Cover workflow + authorization rules, e.g. `SubmitPost_Should_Change_Status_To_PendingReview`,
  `Blogger_Should_Not_Be_Able_To_Approve`, `Admin_Should_Be_Able_To_Approve`, `Viewer_Should_Not_Create_Post`.

## When generating code
1. Respect the layer boundaries above.
2. Enforce the post workflow and role permissions server-side.
3. Prefer commands/queries + DTOs over anemic controllers doing business logic.
4. Add or update tests for new behavior.
5. Keep secrets out of source; use configuration/user-secrets/environment variables.
