# TaskMaster Enterprise — Roadmap

> **Inicio del proyecto:** 2026-04-11  
> **Snapshot actual:** 2026-04-25  
> **Velocidad estimada:** 34 SP / sprint (2 semanas)  
> **Story Points:** Fibonacci (1, 2, 3, 5, 8, 13, 21, 34)

---

## Resumen de Fases

| Fase | Módulos | Story Points | Estado |
|------|---------|-------------|--------|
| Fase 1: Cimientos | M1, M2, M3 | 47 SP | ✅ Completada (~95%) |
| Fase 2: Core + Seguridad | M4, M5 | 92 SP | 🔲 Pendiente |
| Fase 3: Ecosistema | M6, M7 | 42 SP | 🔲 Pendiente |
| Fase 4: Asincronía + Tiempo Real | M8, M9 | 42 SP | 🔲 Pendiente |
| Fase 5: DevOps | M10, M11 | 34 SP | 🔲 Pendiente |
| **TOTAL** | | **257 SP** | |

---

## Sprint 0 — Cimientos y Arquitectura Base
**Duración:** 2 semanas | **Período:** 2026-04-11 → 2026-04-25  
**Estado:** ✅ Completado

| # | Tarea | SP | Estado |
|---|-------|----|--------|
| | **Módulo 1: Setup** | | |
| 1.1 | Estructura de solución .NET 10 (Domain, Application, Infrastructure, Api) | 2 | ✅ |
| 1.2 | SpaProxy + variables de entorno (.env, appsettings) | 3 | ✅ |
| 1.3 | fetchFromApi wrapper (SSR/CSR, internal/public URL) | 3 | ✅ |
| 1.4 | docker-compose (PostgreSQL 18 + Redis 8.4) con healthchecks | 2 | ✅ |
| 1.5 | Next.js App Router base (layout, page, error) + shadcn/ui | 3 | ✅ |
| | **Módulo 2: Clean Architecture** | | |
| 2.1 | Layer division + proyectos separados | 2 | ✅ |
| 2.2 | CQRS con MediatR (GetSystemInfoQuery) | 5 | ✅ |
| 2.3 | ValidationBehavior (FluentValidation pipeline) | 3 | ✅ |
| 2.4 | TransactionBehavior + ICommand marker interfaces | 3 | ✅ |
| 2.5 | Result\<T\> pattern (Success/Failure) | 2 | ✅ |
| 2.6 | DI modular (Application + Infrastructure) | 2 | ✅ |
| 2.7 | Global exception middleware | 3 | ✅ |
| | **Módulo 3: Persistencia** | | |
| 3.1 | EF Core + Npgsql + snake_case naming convention | 3 | ✅ |
| 3.2 | ApplicationDbContext + ApplyConfigurationsFromAssembly | 2 | ✅ |
| 3.3 | BaseEntity (Id, CreatedAt, UpdatedAt) + Enums | 1 | ✅ |
| 3.4 | TaskItem (entidad rica, factory Create, private setters) | 3 | ✅ |
| 3.5 | TaskItemConfiguration (IEntityTypeConfiguration) | 2 | ✅ |
| 3.6 | UnitOfWork (IUnitOfWork + implementación con transacciones) | 2 | ✅ |
| 3.7 | InitialCreate migration | 2 | ✅ |
| 3.8 | DomainException base + InvalidCredentialsException | 1 | ✅ |
| **Total Sprint 0** | | **44 SP** | |

> **Nota:** 3 SP de deuda técnica (exception middleware) se trasladan al Sprint 1.

---

## Sprint 1 — Autenticación y Autorización
**Duración:** 2 semanas | **Período:** 2026-04-28 → 2026-05-09  
**Objetivo:** Módulo 4 completo + deuda técnica Sprint 0

| # | Tarea | SP | Estado |
|---|-------|----|--------|
| | **Deuda técnica M2** | | |
| D1 | Global exception middleware (ValidationException, DomainException, fallback 500) | 3 | ✅ |
| D2 | CORS explícito para producción | 1 | ✅ |
| | **Módulo 4: Auth — Backend** | | |
| 4.1 | ApplicationUser (IdentityUser\<Guid\> + DisplayName + RefreshTokens) | 3 | 🔲 |
| 4.2 | RefreshToken entity + Configuration | 2 | 🔲 |
| 4.3 | ApplicationDbContext hereda de IdentityDbContext | 2 | 🔲 |
| 4.4 | ASP.NET Core Identity setup (DI, password policy) | 3 | 🔲 |
| 4.5 | ITokenService + JwtTokenService (access 15min + refresh 7d) | 5 | 🔲 |
| 4.6 | RegisterCommand + Validator + Handler | 3 | 🔲 |
| 4.7 | LoginCommand + Validator + Handler | 3 | 🔲 |
| 4.8 | RefreshTokenCommand + Handler (refresh token rotation) | 3 | 🔲 |
| 4.9 | LogoutCommand + Handler (revocar refresh token) | 2 | 🔲 |
| 4.10 | AuthController (register, login, refresh, logout, me) | 3 | 🔲 |
| 4.11 | Role policies (Admin, User) en DI | 2 | 🔲 |
| 4.12 | Migración: AddIdentityAndRefreshTokens | 1 | 🔲 |
| | **Módulo 4: Auth — Frontend** | | |
| 4.13 | auth.ts (NextAuth v5 + JWT provider) | 5 | 🔲 |
| 4.14 | middleware.ts (proteger /dashboard/*, /tasks/*) | 2 | 🔲 |
| 4.15 | Páginas login y registro | 3 | 🔲 |
| 4.16 | Server Actions: signIn, signOut wrappers | 2 | 🔲 |
| **Total Sprint 1** | | **47 SP** | |

> Sprint más pesado por identidad. Si excede velocidad, 4.13-4.16 se mueven a Sprint 2.

---

## Sprint 2 — Task Management: Dominio y Comandos
**Duración:** 2 semanas | **Período:** 2026-05-12 → 2026-05-23  
**Objetivo:** Módulo 5 — entidades de dominio, state machine, comandos

| # | Tarea | SP | Estado |
|---|-------|----|--------|
| | **Módulo 5: Dominio** | | |
| 5.1 | Category entity + CategoryConfiguration | 2 | 🔲 |
| 5.2 | Tag entity + TaskItemTag (tabla de unión) + Configurations | 3 | 🔲 |
| 5.3 | TaskItem: nuevas propiedades (AssignedToId, CategoryId, DueDate) | 2 | 🔲 |
| 5.4 | TaskState.Overdue = 5 + state machine (TransitionTo con validación) | 5 | 🔲 |
| 5.5 | TaskItem.LogWork (CompletedWork, RemainingWork, invariantes) | 3 | 🔲 |
| 5.6 | TaskItem.AssignTo / Unassign | 1 | 🔲 |
| 5.7 | TaskItem.SetDueDate / AddTag / RemoveTag | 2 | 🔲 |
| 5.8 | Migración: AddCategoriesTagsAndUserRelations | 2 | 🔲 |
| | **Módulo 5: Comandos** | | |
| 5.9 | CreateTaskCommand + Validator + Handler | 5 | 🔲 |
| 5.10 | UpdateTaskCommand + Validator + Handler | 5 | 🔲 |
| 5.11 | DeleteTaskCommand + Handler (soft delete → Removed) | 2 | 🔲 |
| 5.12 | ChangeTaskStateCommand + Handler | 3 | 🔲 |
| 5.13 | AssignTaskCommand + Handler | 2 | 🔲 |
| 5.14 | LogWorkCommand + Validator + Handler | 3 | 🔲 |
| **Total Sprint 2** | | **40 SP** | |

---

## Sprint 3 — Task Management: Queries y Frontend
**Duración:** 2 semanas | **Período:** 2026-05-26 → 2026-06-06  
**Objetivo:** Módulo 5 consultas + Frontend + inicio Módulo 6

| # | Tarea | SP | Estado |
|---|-------|----|--------|
| | **Módulo 5: Queries** | | |
| 5.15 | PagedResult\<T\> model | 1 | 🔲 |
| 5.16 | TaskSummaryDto + TaskDetailDto | 2 | 🔲 |
| 5.17 | GetTasksQuery (paginación + filtros: state, priority, assignedTo, categoryId, search) | 8 | 🔲 |
| 5.18 | GetTaskByIdQuery | 2 | 🔲 |
| 5.19 | TasksController (CRUD + state + assign + log-work) | 5 | 🔲 |
| 5.20 | CategoriesController (CRUD admin) | 2 | 🔲 |
| | **Módulo 5: Frontend** | | |
| 5.21 | Página /tasks (Server Component con filtros iniciales) | 3 | 🔲 |
| 5.22 | TaskList + TaskCard (Client Components) | 5 | 🔲 |
| 5.23 | TaskFilters + TaskStateSelector | 3 | 🔲 |
| 5.24 | Server Actions: createTask, updateTask, deleteTask, logWork | 5 | 🔲 |
| 5.25 | Página /tasks/new y /tasks/[id] | 3 | 🔲 |
| | **Módulo 6: Setup Azurite** | | |
| 6.1 | Azurite en docker-compose | 2 | 🔲 |
| 6.2 | IBlobStorageService interface (Domain) | 1 | 🔲 |
| **Total Sprint 3** | | **42 SP** | |

---

## Sprint 4 — Blob Storage + Reportes
**Duración:** 2 semanas | **Período:** 2026-06-09 → 2026-06-20  
**Objetivo:** Módulo 6 completo + Módulo 7 completo

| # | Tarea | SP | Estado |
|---|-------|----|--------|
| | **Módulo 6: Blob Storage** | | |
| 6.3 | BlobStorageService (Azure.Storage.Blobs SDK) | 5 | 🔲 |
| 6.4 | TaskAttachment entity + Configuration | 3 | 🔲 |
| 6.5 | UploadAttachmentCommand + Validator + Handler | 5 | 🔲 |
| 6.6 | DeleteAttachmentCommand + Handler | 2 | 🔲 |
| 6.7 | GetAttachmentDownloadQuery + Handler | 3 | 🔲 |
| 6.8 | AttachmentsController | 3 | 🔲 |
| 6.9 | Frontend: AttachmentUploader (drag-and-drop) | 5 | 🔲 |
| 6.10 | Frontend: AttachmentList + preview | 2 | 🔲 |
| | **Módulo 7: Reportes** | | |
| 7.1 | IReportService interface (Domain) | 1 | 🔲 |
| 7.2 | ExcelReportService (ClosedXML) | 5 | 🔲 |
| 7.3 | WordReportService (OpenXML SDK) | 5 | 🔲 |
| 7.4 | ExportTasksExcelQuery + Handler | 3 | 🔲 |
| 7.5 | GenerateTaskWordSummaryQuery + Handler | 3 | 🔲 |
| 7.6 | ReportsController (export-excel, word-summary) | 2 | 🔲 |
| 7.7 | Frontend: ExportButton (descarga blob) | 3 | 🔲 |
| **Total Sprint 4** | | **50 SP** | |

> Sprint más cargado. Considerar mover 6.9-6.10 a Sprint 5 si se excede velocidad.

---

## Sprint 5 — Background Jobs
**Duración:** 2 semanas | **Período:** 2026-06-23 → 2026-07-04  
**Objetivo:** Módulo 8 completo

| # | Tarea | SP | Estado |
|---|-------|----|--------|
| | **Módulo 8: Hangfire** | | |
| 8.1 | Hangfire NuGet (Hangfire.AspNetCore + Hangfire.PostgreSql) | 1 | 🔲 |
| 8.2 | Configuración Hangfire en Infrastructure DI | 2 | 🔲 |
| 8.3 | IEmailService interface (Domain) | 1 | 🔲 |
| 8.4 | EmailService (SMTP o mock para dev) | 3 | 🔲 |
| 8.5 | MarkOverdueTasksJob (cron diario 00:00 UTC) | 5 | 🔲 |
| 8.6 | SendTaskAssignedEmailJob (fire-and-forget) | 5 | 🔲 |
| 8.7 | SendTaskCompletedEmailJob (fire-and-forget) | 3 | 🔲 |
| 8.8 | HangfireAuthorizationFilter (solo rol Admin) | 3 | 🔲 |
| 8.9 | Dashboard `/hangfire` en Program.cs | 1 | 🔲 |
| 8.10 | Integración fire-and-forget en handlers (AssignTask, ChangeState) | 3 | 🔲 |
| 8.11 | Tests de integración: job vs DB efímero | 5 | 🔲 |
| **Total Sprint 5** | | **32 SP** | |

---

## Sprint 6 — SignalR + Docker
**Duración:** 2 semanas | **Período:** 2026-07-07 → 2026-07-18  
**Objetivo:** Módulo 9 completo + Módulo 10 completo

| # | Tarea | SP | Estado |
|---|-------|----|--------|
| | **Módulo 9: SignalR** | | |
| 9.1 | INotificationService interface (Domain) | 1 | 🔲 |
| 9.2 | TaskHub (OnConnectedAsync, OnDisconnectedAsync, grupos por userId) | 5 | 🔲 |
| 9.3 | SignalRNotificationService (IHubContext\<TaskHub\>) | 3 | 🔲 |
| 9.4 | Integración en AssignTaskCommandHandler | 2 | 🔲 |
| 9.5 | Integración en ChangeTaskStateCommandHandler | 2 | 🔲 |
| 9.6 | Registro SignalR + MapHub en Program.cs | 1 | 🔲 |
| 9.7 | Frontend: lib/signalr.ts (HubConnection factory + JWT) | 3 | 🔲 |
| 9.8 | Frontend: useTaskHub hook | 5 | 🔲 |
| 9.9 | Frontend: NotificationBell + NotificationToast | 5 | 🔲 |
| | **Módulo 10: Docker** | | |
| 10.1 | Dockerfile multi-stage .NET (build + runtime) | 3 | 🔲 |
| 10.2 | next.config.js output: 'standalone' | 1 | 🔲 |
| 10.3 | Dockerfile standalone Next.js | 3 | 🔲 |
| 10.4 | docker-compose.yml completo (API + Client + PG + Redis + Azurite) | 5 | 🔲 |
| 10.5 | Health checks en docker-compose para todos los servicios | 2 | 🔲 |
| 10.6 | /api/health/ready endpoint (verifica PG + Redis + Azurite) | 2 | 🔲 |
| **Total Sprint 6** | | **43 SP** | |

---

## Sprint 7 — Kubernetes y Cierre
**Duración:** 2 semanas | **Período:** 2026-07-21 → 2026-08-01  
**Objetivo:** Módulo 11 completo + observabilidad final + tests E2E

| # | Tarea | SP | Estado |
|---|-------|----|--------|
| | **Módulo 11: Kubernetes** | | |
| 11.1 | namespace.yaml | 1 | 🔲 |
| 11.2 | ConfigMaps (api-config, client-config) | 2 | 🔲 |
| 11.3 | Secrets (db-secret, jwt-secret) | 2 | 🔲 |
| 11.4 | Deployment API (replicas: 2, liveness/readiness probes) | 5 | 🔲 |
| 11.5 | Deployment Next.js (replicas: 2) | 3 | 🔲 |
| 11.6 | StatefulSet PostgreSQL | 3 | 🔲 |
| 11.7 | Services (ClusterIP para API, Client, PG, Redis) | 2 | 🔲 |
| 11.8 | Ingress controller (nginx: / → client, /api → api, /hubs → api) | 3 | 🔲 |
| 11.9 | HPA (api: 2-10 replicas, CPU 70%) | 3 | 🔲 |
| | **Observabilidad y Cierre** | | |
| 11.10 | Serilog (console JSON + enrich CorrelationId) | 3 | 🔲 |
| 11.11 | OpenTelemetry + Prometheus exporter (/metrics) | 5 | 🔲 |
| 11.12 | Tests E2E con Playwright (auth + task lifecycle) | 8 | 🔲 |
| **Total Sprint 7** | | **40 SP** | |

---

## Resumen de Cronograma

```
Sprint 0  ████████████████ ✅ 2026-04-11 → 2026-04-25  (44 SP)
Sprint 1  ░░░░░░░░░░░░░░░░    2026-04-28 → 2026-05-09  (47 SP) Auth
Sprint 2  ░░░░░░░░░░░░░░░░    2026-05-12 → 2026-05-23  (40 SP) Tasks Domain
Sprint 3  ░░░░░░░░░░░░░░░░    2026-05-26 → 2026-06-06  (42 SP) Tasks UI
Sprint 4  ░░░░░░░░░░░░░░░░    2026-06-09 → 2026-06-20  (50 SP) Blobs + Reports
Sprint 5  ░░░░░░░░░░░░░░░░    2026-06-23 → 2026-07-04  (32 SP) Hangfire
Sprint 6  ░░░░░░░░░░░░░░░░    2026-07-07 → 2026-07-18  (43 SP) SignalR + Docker
Sprint 7  ░░░░░░░░░░░░░░░░    2026-07-21 → 2026-08-01  (40 SP) K8s + Cierre

Entrega estimada: 2026-08-01
Duración total: ~16 semanas (4 meses)
Total story points: ~338 SP (incluyendo deuda técnica)
```

---

## Criterios de Aceptación por Sprint

Cada sprint se considera **DONE** cuando:
1. Todos los endpoints nuevos responden correctamente (Swagger/manual)
2. Tests unitarios pasan (`dotnet test`)
3. Tests de integración pasan contra DB real (Testcontainers)
4. No hay regresión en endpoints de sprints anteriores
5. docker-compose up levanta sin errores
6. El frontend consume los nuevos endpoints sin errores en consola
