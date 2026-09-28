# SimpleBlog – Development Plan

An incremental plan: get a working vertical slice first, then layer on moderation, messaging, caching,
extra frontend, and deployment. Each phase should end with a runnable, tested state.

## Phase 0 – Solution scaffolding
- [ ] Create solution `SimpleBlog` and projects: `Core`, `Application`, `Infrastructure`, `Api`, `Tests`.
- [ ] Wire up project references per Clean Architecture (Core ← Application ← Infrastructure ← Api).
- [ ] Add `.editorconfig`, nullable enabled, central package management (optional).
- [ ] Add solution to Git; add `.gitignore`.
- **Done when**: solution builds and `SimpleBlog.Api` runs with a health endpoint.

## Phase 1 – Domain + persistence
- [ ] Define entities: `Post`, `Category`, `Comment`, `Review`, `Notification`, `OutboxMessage`.
- [ ] Define `PostStatus` enum and domain rules for status transitions.
- [ ] Add `AppDbContext` (EF Core 10) + configurations in `Infrastructure`.
- [ ] Create initial migration and update the database (SQL Server).
- **Done when**: migrations apply and tables exist.

## Phase 2 – Authentication & authorization
- [ ] Add ASP.NET Core Identity with `ApplicationUser` (`GoogleId`, `DisplayName`, `AvatarUrl`, `CreatedAt`).
- [ ] Seed roles: `Viewer`, `Blogger`, `Admin`.
- [ ] Implement email/password login → `POST /api/auth/login` (JWT or cookie).
- [ ] Implement Google OAuth/OIDC → `POST /api/auth/google`.
- [ ] Apply role-based authorization policies.
- **Done when**: users can log in both ways and receive a valid session/token with roles.

## Phase 3 – Blog CRUD (vertical slice)
- [ ] Application features: `CreatePost`, `UpdatePost`, `GetPublishedPosts`, `GetPostById` (commands/queries + DTOs + validators).
- [ ] API controllers: `GET /api/posts`, `GET /api/posts/{id}`, `POST /api/posts`, `PUT /api/posts/{id}`.
- [ ] Enforce: Blogger creates/edits own posts as `Draft`; Viewer reads published only.
- [ ] Add Swagger + centralized exception-handling middleware.
- **Done when**: a Blogger can create/edit a draft and Viewers can list published posts via the API.

## Phase 4 – Moderation workflow
- [ ] Features: `SubmitPost`, `ApprovePost`, `RejectPost` with status-transition enforcement.
- [ ] Endpoints: `POST /api/posts/{id}/submit|approve|reject`.
- [ ] Authorization: only Admin can approve/reject/publish.
- [ ] Persist `Review` records for moderation history.
- **Done when**: full `Draft → PendingReview → Published/Rejected` flow works with correct permissions.

## Phase 5 – Blazor frontend
- [ ] Create `SimpleBlog.Blazor`; typed HTTP client to the API.
- [ ] Pages: Home (Blog, Categories, Search, Login), Dashboard (My Posts, Create/Edit, Profile),
      Admin (Dashboard, Users, Posts, Pending Reviews, Notifications).
- [ ] Professional UI theme (SaaS look).
- **Done when**: end-to-end flows work through the Blazor UI against the API.

## Phase 6 – Messaging (Outbox + Worker + Service Bus)
- [ ] Add `OutboxMessage` writes inside the same transaction as domain changes (e.g. on submit).
- [ ] Create `SimpleBlog.Worker`; poll outbox, publish to Azure Service Bus, mark `ProcessedAt`/`RetryCount`.
- [ ] Consume `PostSubmitted` → find admins → create `Notification` records.
- [ ] Surface notifications in the Admin UI.
- **Done when**: submitting a post reliably produces admin notifications via the queue.

## Phase 7 – Redis caching
- [ ] Add Redis; cache-aside for `GET /api/posts` (and other read-heavy endpoints).
- [ ] Invalidate cache on create/update/publish.
- [ ] Optional: rate limiting / distributed locks.
- **Done when**: repeat reads hit Redis and writes invalidate correctly.

## Phase 8 – Next.js frontend
- [ ] Scaffold `SimpleBlog.NextJs` (App Router, TypeScript).
- [ ] Structure: `app/` (page, blog, login, dashboard, admin), `components/`, `lib/`, `services/`, `hooks/`, `types/`.
- [ ] Typed API client in `services/`; auth (email/password + Google) against the API.
- **Done when**: Next.js delivers the same core flows as Blazor against the shared API.

## Phase 9 – Testing
- [ ] Unit tests: Domain rules, Application services, validators.
- [ ] Integration tests: API endpoints (auth + workflow + authorization).
- [ ] Key cases: `CreatePost_Should_Create_Draft`, `SubmitPost_Should_Change_Status_To_PendingReview`,
      `Blogger_Should_Not_Be_Able_To_Approve`, `Admin_Should_Be_Able_To_Approve`, `Viewer_Should_Not_Create_Post`.
- **Done when**: tests pass in CI and cover workflow + authorization.

## Phase 10 – Docker & deployment
- [ ] Dockerfiles for API, Worker, Blazor, Next.js.
- [ ] `docker-compose.yml` with API, Worker, Blazor, Next.js, SQL Server (1433), Redis (6379).
- [ ] `docker compose up` runs the whole stack locally.
- [ ] Deploy to cloud (☁️) with environment-based configuration and secrets.
- **Done when**: the full stack runs via one command and is deployable.

## Cross-cutting checklist
- [ ] Secrets via user-secrets/environment variables (never committed).
- [ ] Consistent problem-details error responses.
- [ ] DTOs at API boundaries (no EF entities leaked).
- [ ] Logging + basic observability.
- [ ] Update tests alongside each feature.

## Suggested milestones
1. **MVP**: Phases 0–4 (API + auth + blog + moderation).
2. **UI**: Phase 5 (Blazor).
3. **Async + performance**: Phases 6–7 (messaging + Redis).
4. **Second frontend**: Phase 8 (Next.js).
5. **Quality + ship**: Phases 9–10 (tests + Docker/deploy).
