# SimpleBlog – Product Specification

## 1. Goal
Build a complete product: **Blog + CMS + Moderation + Authentication + Notification System** that looks
and behaves like a small real-world SaaS, not a tutorial.

Capabilities:
- 👤 Viewer, ✍️ Blogger, 🛡️ Admin roles
- 🔐 Login with Google + email/password
- 📝 Create / Edit blog posts
- 🔍 Moderation workflow
- 🔔 Notifications via 📬 message queue
- ⚡ Redis caching
- 🧪 Unit + Integration tests
- 🐳 Docker + ☁️ Deploy
- 🎨 Professional UI, 🖥️ Blazor + ⚛️ Next.js frontends

## 2. Architecture
```
                         ┌───────────────────────┐
                         │       USERS           │
                         └───────────┬───────────┘
                                     │
                     ┌───────────────┴───────────────┐
                     ▼                               ▼
          ┌──────────────────┐             ┌──────────────────┐
          │  Blazor Frontend │             │ Next.js Frontend │
          │  .NET 10 / Blazor│             │ React / Next.js  │
          └────────┬─────────┘             └────────┬─────────┘
                   └───────────────┬────────────────┘
                                   │ HTTP
                                   ▼
                       ┌──────────────────────┐
                       │   ASP.NET Core 10 API│
                       │ Auth / Blog / Mod /  │
                       │ Users / Notifications│
                       └──────────┬───────────┘
             ┌────────────────────┼────────────────────┐
             ▼                    ▼                    ▼
      ┌─────────────┐      ┌─────────────┐     ┌──────────────┐
      │ SQL Server  │      │    Redis    │     │ Message Queue│
      │ EF Core 10  │      │   Cache     │     │  Azure SB    │
      └─────────────┘      └─────────────┘     └──────┬───────┘
                                                      ▼
                                             ┌──────────────────┐
                                             │ Background Worker│
                                             │   .NET Worker    │
                                             └──────────────────┘
```

Key principle: **both frontends call the API only** — never the database directly. This makes it easy
to add Mobile / React / Angular / Flutter clients later against the same API.

## 3. Solution structure
```
SimpleBlog
├── SimpleBlog.Api            # REST API (controllers, auth, middleware, swagger)
├── SimpleBlog.Application    # Business logic (features, DTOs, commands, queries, validators)
├── SimpleBlog.Core           # Domain (entities, enums, interfaces, value objects)
├── SimpleBlog.Infrastructure # EF Core, Identity, Google auth, Redis, Service Bus, repos, email
├── SimpleBlog.Blazor         # Blazor frontend
├── SimpleBlog.NextJs         # Next.js frontend
├── SimpleBlog.Worker         # Background worker
├── SimpleBlog.Tests          # Unit + integration tests
└── docker-compose.yml
```

## 4. Domain model
| Entity | Notes |
| --- | --- |
| `ApplicationUser` | `Id`, `Email`, `DisplayName`, `GoogleId`, `AvatarUrl`, `CreatedAt` |
| `Post` | Title, content, author, category, `PostStatus` |
| `Category` | Name/slug for grouping posts |
| `Comment` | User comment on a post |
| `Review` | Moderation record (approve/reject + admin note) |
| `Notification` | Per-user notification created by the worker |
| `OutboxMessage` | `Id`, `Type`, `Payload`, `CreatedAt`, `ProcessedAt`, `RetryCount` |

`PostStatus`: `Draft`, `PendingReview`, `Published`, `Rejected`.
Roles: `Viewer`, `Blogger`, `Admin`.

## 5. Authentication & authorization
- **Email/password** and **Continue with Google** (OAuth 2.0 / OpenID Connect).
- ASP.NET Core Identity stores users and roles; Google users persist a `GoogleId`.
- Role permissions:
  - **Viewer**: read published posts.
  - **Blogger**: create post, edit own post, submit post, view own posts. Cannot approve/reject/publish.
  - **Admin**: manage users, manage posts, approve, reject, publish.
- Authorization is enforced server-side on every protected endpoint.

### Login UI
```
┌─────────────────────────────────┐
│          Welcome Back           │
│ Email    [____________________] │
│ Password [•••••••_____________] │
│            [ Login ]            │
│ ─────────── OR ───────────────  │
│    [ Continue with Google ]     │
└─────────────────────────────────┘
```

## 6. Blog workflow
```
Draft → (Submit) → PendingReview → (Approve) → Published
                                 → (Reject)  → Rejected
```

## 7. Messaging (Outbox + Service Bus)
On submit:
```
Blogger → POST /api/posts/{id}/submit → Application → DB (+ Outbox row, same transaction)
        → Worker polls Outbox → Azure Service Bus → Worker consumer → Notification
```
- Event example: `{ "eventType": "PostSubmitted", "postId": 123, "authorId": 456 }`.
- Worker handles `PostSubmitted`: find admins → create notifications.
- The Outbox pattern prevents the "DB saved but message send failed" inconsistency.

## 8. Caching (Redis)
- Added after the basic version is stable.
- Cache-aside on read-heavy endpoints:
  - First request: API → Redis MISS → SQL Server → populate Redis → response.
  - Later: API → Redis HIT → response.
- Also usable for rate limiting, distributed locking, and session-related data.

## 9. Frontends
### Blazor (`SimpleBlog.Blazor`)
```
Home:      Blog, Categories, Search, Login
Dashboard: My Posts, Create Post, Edit Post, Profile
Admin:     Dashboard, Users, Posts, Pending Reviews, Notifications
```

### Next.js (`SimpleBlog.NextJs`)
```
app/  (page.tsx, blog/, login/, dashboard/, admin/)
components/  lib/  services/  hooks/  types/
```
All data access goes through the API, e.g. `await fetch(`${API_URL}/api/posts`)`.

## 10. API surface (representative)
| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/api/auth/login` | Email/password login |
| POST | `/api/auth/google` | Google login |
| GET | `/api/posts` | List published posts |
| GET | `/api/posts/{id}` | Get post |
| POST | `/api/posts` | Create post (Blogger) |
| PUT | `/api/posts/{id}` | Update own post (Blogger) |
| POST | `/api/posts/{id}/submit` | Submit for review (Blogger) |
| POST | `/api/posts/{id}/approve` | Approve (Admin) |
| POST | `/api/posts/{id}/reject` | Reject (Admin) |

## 11. Testing
```
SimpleBlog.Tests
├── Unit         (Application, Domain, Services)
├── Integration  (API)
└── TestFixtures
```
Representative cases:
- `CreatePost_Should_Create_Draft`
- `SubmitPost_Should_Change_Status_To_PendingReview`
- `Blogger_Should_Not_Be_Able_To_Approve`
- `Admin_Should_Be_Able_To_Approve`
- `Viewer_Should_Not_Create_Post`

## 12. Docker & deployment
`docker compose up` runs: Blazor, Next.js, API, Worker, SQL Server, Redis.
```
localhost:xxxx → Blazor
localhost:xxxx → Next.js
localhost:xxxx → API
localhost:1433 → SQL Server
localhost:6379 → Redis
```

## 13. Non-functional requirements
- Professional, SaaS-grade UI.
- Secure auth, server-side authorization, secrets kept out of source control.
- Reliable messaging via Outbox.
- Testable, layered architecture with clear boundaries.
