# TaskMaster Enterprise — Lista de Tareas

> **Última actualización:** 2026-04-25  
> **Estado verificado:** leyendo código fuente del repositorio  
> Formato: `[x]` = Completado | `[ ]` = Pendiente | `[~]` = Parcial/Deuda

---

## Fase 1: Cimientos y Arquitectura Base

### Módulo 1: Setup (Next.js 16 + .NET 10)

- [x] Crear solución .NET 10 con CLI (`dotnet new sln`)
- [x] Proyectos: `TaskMaster.Domain`, `TaskMaster.Application`, `TaskMaster.Infrastructure`, `TaskMaster.Api`
- [x] Referencias entre proyectos (regla de dependencia respetada)
- [x] Configurar SpaProxy para desarrollo
- [x] Variables de entorno: `.env` para Docker, `appsettings.json` / `appsettings.Development.json`
- [x] Wrapper `fetchFromApi<T>` con manejo SSR/CSR (`lib/api.ts`)
  - [x] `getApiUrl` detecta `typeof window === "undefined"` y usa `API_INTERNAL_URL`
  - [x] Maneja `response.ok`, JSON, texto plano y errores de red
  - [x] Devuelve `ApiResult<T>` (nunca lanza excepción)
- [x] `docker-compose.yml` con PostgreSQL 18-alpine + Redis 8.4-alpine
  - [x] Healthchecks para ambos servicios
  - [x] Volúmenes nombrados
  - [x] Network `taskmaster-network`
- [x] Next.js App Router base
  - [x] `app/layout.tsx` (ThemeProvider)
  - [x] `app/page.tsx` (Server Component: health + system info)
  - [x] `app/error.tsx` (error boundary)
- [x] shadcn/ui configurado (`components.json`)
  - [x] `Card`, `Badge`, `Button`, `Alert`
- [x] TypeScript types (`types/index.ts`, `types/system-info.ts`)

---

### Módulo 2: Clean Architecture y Patrones Estructurales

- [x] División en capas (Domain, Application, Infrastructure, Api)
- [x] CQRS con MediatR
  - [x] `GetSystemInfoQuery` + `GetSystemInfoQueryHandler`
  - [x] `SystemInfoResult` DTO
  - [x] `SystemController` consume via `ISender`
- [x] `ICommand<TResponse>` y `ICommand` (marker interfaces para MediatR)
- [x] `Result<T>` y `Result` (Success/Failure pattern)
  - [x] `IResult` interface
  - [x] `ToFailure<U>()` para propagación de errores
- [x] `ValidationBehavior<TRequest, TResponse>` (FluentValidation pipeline)
  - [x] Ejecuta validadores en paralelo
  - [x] Lanza `ValidationException` con todos los fallos
- [x] `TransactionBehavior<TRequest, TResponse>`
  - [x] Solo abre transacción si `request is ICommand`
  - [x] Log de inicio/fin/error
  - [x] Rollback en catch
- [x] DI modular
  - [x] `Application/DependencyInjection.cs` (MediatR + FluentValidation + Behaviors)
  - [x] `Infrastructure/DependencyInjection.cs` (DbContext + UnitOfWork + SystemInfoService)
- [~] Global exception middleware ← **DEUDA TÉCNICA**
  - [ ] `Middleware/ExceptionHandlingMiddleware.cs`
  - [ ] Mapa: `ValidationException` → 422, `DomainException` → `HttpStatuCode`, `KeyNotFoundException` → 404, fallback → 500
  - [ ] Registro en `Program.cs`: `app.UseMiddleware<ExceptionHandlingMiddleware>()`
- [ ] CORS explícito para producción en `Program.cs`

---

### Módulo 3: Persistencia (EF Core + PostgreSQL)

- [x] EF Core + Npgsql configurado en `Infrastructure/DependencyInjection.cs`
  - [x] `UseSnakeCaseNamingConvention()`
  - [x] `EnableRetryOnFailure(maxRetryCount: 5)`
  - [x] `MigrationsHistoryTable("__ef_migrations_history")`
- [x] `ApplicationDbContext` con `ApplyConfigurationsFromAssembly`
  - [x] `DbSet<TaskItem> Tasks`
- [x] `BaseEntity` abstracta (Id, CreatedAt, UpdatedAt, `UpdateTimestamp()`)
- [x] `TaskItem` entidad rica
  - [x] Factory `Create(Guid id, string title, string? description, int originalEstimate)`
  - [x] Validaciones inline (id no vacío, title no nulo, estimate ≥ 0)
  - [x] Private setters (encapsulamiento)
  - [x] Constructor privado (EF navigation)
- [x] Enums del dominio
  - [x] `TaskState` (ToDo=1, InProgress=2, Done=3, Removed=4)
  - [x] `Priority` (Low=1, Medium=2, High=3, Urgent=4)
  - [x] `Activity` (Requirement, Design, Development, Testing, Deployment, Documentation)
- [x] `TaskItemConfiguration` (`IEntityTypeConfiguration<TaskItem>`)
  - [x] Tabla `tasks`, clave `Id`, `ValueGeneratedNever`
  - [x] `Title` max 100, `Description` max 500
  - [x] `State` y `Priority` como `string` en DB
  - [x] Índices en `State` y `Priority`
  - [x] `timestamptz` para `CreatedAt` y `UpdatedAt`
- [x] `IUnitOfWork` (SaveChanges, BeginTransaction, Commit, Rollback)
- [x] `UnitOfWork` implementación
  - [x] Wraps `IDbContextTransaction`
  - [x] `Dispose` pattern correcto (GC.SuppressFinalize)
- [x] Migración `20260425155600_InitialCreate`
- [x] `DomainException` abstracta (message, errorCode, httpStatusCode)
- [x] `InvalidCredentialsException` (errorCode: INVALID_CREDENTIALS, 401)
- [x] `ISystemInfoService` + `SystemInfoService` (versión del assembly)

---

## Fase 2: Core del Negocio y Seguridad

### Módulo 4: Autenticación y Autorización (JWT)

**Backend:**
- [ ] `ApplicationUser : IdentityUser<Guid>` (DisplayName, CreatedAt, RefreshTokens)
- [ ] `RefreshToken` entity (Id, Token, ExpiresAt, IsRevoked, UserId)
- [ ] `UserRole` enum (Admin, User)
- [ ] `ApplicationDbContext` hereda de `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- [ ] `ApplicationUserConfiguration` (IEntityTypeConfiguration)
- [ ] `RefreshTokenConfiguration` (índice único en Token)
- [ ] ASP.NET Core Identity en DI (password policy: min 8 chars, require digit)
- [ ] `ITokenService` interface (GenerateAccessToken, GenerateRefreshToken, ValidateRefreshToken)
- [ ] `JwtTokenService` implementación
  - [ ] Access token: 15 min, claims (sub, email, role, jti)
  - [ ] Refresh token: GUID seguro + hash SHA-256
  - [ ] Validación y rotación de refresh token
- [ ] `RegisterCommand` + Validator (email único, password ≥ 8) + Handler
- [ ] `LoginCommand` + Validator + Handler → `AuthResult`
- [ ] `RefreshTokenCommand` + Handler (refresh token rotation)
- [ ] `LogoutCommand` + Handler (marcar refresh token como revocado)
- [ ] `AuthResult` DTO (AccessToken, RefreshToken, ExpiresAt, UserId, Role)
- [ ] `AuthController` (POST /register, POST /login, POST /refresh, POST /logout, GET /me)
- [ ] Role-based authorization policies (Admin, User) en Program.cs
- [ ] Migración `AddIdentityAndRefreshTokens`

**Frontend:**
- [ ] `auth.ts` (NextAuth v5 config: credentials provider → llama /api/auth/login)
- [ ] `middleware.ts` (protege /dashboard/*, /tasks/*)
- [ ] `app/(auth)/login/page.tsx`
- [ ] `app/(auth)/register/page.tsx`
- [ ] Server Actions: signIn, signOut, getSession wrappers
- [ ] `types/auth.ts` (Session, UserRole)
- [ ] Mostrar info de usuario en navbar (displayName, role)

---

### Módulo 5: Gestión de Tareas

**Domain:**
- [ ] `Category` entity (Id, Name unique, Color?, ICollection\<TaskItem\>)
- [ ] `Tag` entity (Id, Name, Color?)
- [ ] `TaskItemTag` (tabla de unión TaskItem ↔ Tag)
- [ ] `TaskItem` — propiedades adicionales:
  - [ ] `Guid? AssignedToId`
  - [ ] `Guid? CategoryId`
  - [ ] `DateTime? DueDate`
  - [ ] `ICollection<Tag> Tags`
- [ ] `TaskState.Overdue = 5` (nuevo valor — solo Módulo 8 lo asigna)
- [ ] `TaskItem.TransitionTo(TaskState newState)` — state machine con validación
  - [ ] ToDo → InProgress, Removed
  - [ ] InProgress → Done, ToDo, Removed
  - [ ] Done → inmutable (lanza DomainException)
  - [ ] Removed → inmutable
- [ ] `TaskItem.LogWork(int hours)` — valida hours ≤ RemainingWork
- [ ] `TaskItem.AssignTo(Guid userId)` / `Unassign()`
- [ ] `TaskItem.SetDueDate(DateTime dueDate)` — valida dueDate > utcNow
- [ ] `TaskItem.AddTag(Tag tag)` / `RemoveTag(Guid tagId)`

**Configurations:**
- [ ] `CategoryConfiguration` (tabla categories, unique index en Name)
- [ ] `TagConfiguration` (tabla tags)
- [ ] `TaskItemTagConfiguration` (tabla task_item_tags, PK compuesta)
- [ ] Migración `AddCategoriesTagsAndUserRelations`

**Application — Commands:**
- [ ] `CreateTaskCommand` (Title, Description?, OriginalEstimate, Priority, CategoryId?, DueDate?) + Validator + Handler → `Result<Guid>`
- [ ] `UpdateTaskCommand` (TaskId, Title?, Description?, Priority?, CategoryId?, DueDate?) + Validator + Handler
- [ ] `DeleteTaskCommand` (TaskId) + Handler → soft delete (State = Removed)
- [ ] `ChangeTaskStateCommand` (TaskId, NewState) + Handler
- [ ] `AssignTaskCommand` (TaskId, UserId) + Handler
- [ ] `LogWorkCommand` (TaskId, Hours) + Validator (hours > 0) + Handler

**Application — Queries:**
- [ ] `PagedResult<T>` (Items, TotalCount, Page, PageSize, TotalPages)
- [ ] `TaskSummaryDto` (Id, Title, State, Priority, AssignedTo, DueDate, Tags)
- [ ] `TaskDetailDto` (todos los campos + Category + historial work)
- [ ] `GetTasksQuery` (Page, PageSize, State?, Priority?, AssignedToId?, CategoryId?, Search?) + Handler → `Result<PagedResult<TaskSummaryDto>>`
- [ ] `GetTaskByIdQuery` (TaskId) + Handler → `Result<TaskDetailDto>`

**Api:**
- [ ] `TasksController`
  - [ ] `POST /api/tasks` [Authorize]
  - [ ] `GET /api/tasks` [Authorize] (query params: page, pageSize, state, priority, assignedTo, categoryId, search)
  - [ ] `GET /api/tasks/{id}` [Authorize]
  - [ ] `PUT /api/tasks/{id}` [Authorize]
  - [ ] `DELETE /api/tasks/{id}` [Authorize]
  - [ ] `PATCH /api/tasks/{id}/state` [Authorize]
  - [ ] `PATCH /api/tasks/{id}/assign` [Authorize(Roles=Admin)]
  - [ ] `POST /api/tasks/{id}/log-work` [Authorize]
- [ ] `CategoriesController` CRUD [Authorize(Roles=Admin)]

**Frontend:**
- [ ] `app/(dashboard)/tasks/page.tsx` (Server Component: lista inicial con filtros desde URL)
- [ ] `app/(dashboard)/tasks/[id]/page.tsx` (detail)
- [ ] `app/(dashboard)/tasks/new/page.tsx`
- [ ] `components/tasks/TaskList.tsx` (Client Component)
- [ ] `components/tasks/TaskCard.tsx`
- [ ] `components/tasks/TaskFilters.tsx` (state, priority, category, search)
- [ ] `components/tasks/TaskStateSelector.tsx` (dropdown con colores por estado)
- [ ] `lib/actions/tasks.ts` (Server Actions: createTask, updateTask, deleteTask, logWork, changeState)

---

## Fase 3: Ecosistema y Procesamiento Pesado

### Módulo 6: Almacenamiento en la Nube (Azurite)

**Infraestructura:**
- [ ] Azurite en `docker-compose.yml` (puerto 10000, comando `azurite-blob --blobHost 0.0.0.0`)
- [ ] Volumen `azurite_data` nombrado

**Domain:**
- [ ] `IBlobStorageService` interface (UploadAsync, DownloadAsync, DeleteAsync, ExistsAsync)
- [ ] `TaskAttachment` entity (Id, TaskId, FileName, ContentType, FileSizeBytes, BlobUri, UploadedAt, UploadedByUserId)

**Application:**
- [ ] `AttachmentUploadResult` DTO (AttachmentId, FileName, BlobUri)
- [ ] `AttachmentDownloadDto` (Stream, ContentType, FileName)
- [ ] `UploadAttachmentCommand` + Validator (file ≤ 10MB, ContentType permitido) + Handler
- [ ] `DeleteAttachmentCommand` + Handler (verifica propiedad o Admin)
- [ ] `GetAttachmentDownloadQuery` + Handler

**Infrastructure:**
- [ ] `BlobStorageService` implementación (Azure.Storage.Blobs SDK)
  - [ ] Lee `AzureBlobStorage:ConnectionString` y `AzureBlobStorage:ContainerName`
  - [ ] Crea container si no existe
- [ ] `TaskAttachmentConfiguration` (EF)
- [ ] Migración `AddTaskAttachments`
- [ ] Registro `IBlobStorageService` en DI

**Api:**
- [ ] `AttachmentsController`
  - [ ] `POST /api/tasks/{taskId}/attachments` [Authorize] (multipart/form-data)
  - [ ] `GET /api/attachments/{id}/download` [Authorize]
  - [ ] `DELETE /api/attachments/{id}` [Authorize]

**Frontend:**
- [ ] `components/tasks/AttachmentUploader.tsx` (Client: drag-and-drop, progress)
- [ ] `components/tasks/AttachmentList.tsx` (Client: lista con preview PDF/imagen)
- [ ] `lib/actions/attachments.ts` (Server Actions: upload, delete)

---

### Módulo 7: Procesamiento de Documentos

**Domain:**
- [ ] `IReportService` interface (GenerateTasksExcelAsync, GenerateTaskWordSummaryAsync)

**Infrastructure:**
- [ ] `ExcelReportService` (ClosedXML NuGet)
  - [ ] Columnas: Title, State, Priority, AssignedTo, Category, DueDate, OriginalEstimate, CompletedWork
  - [ ] Estilos: header bold, colores por prioridad
- [ ] `WordReportService` (DocumentFormat.OpenXml NuGet)
  - [ ] Template con: título, descripción, tabla de work log, lista de adjuntos
- [ ] Registro `IReportService` en DI

**Application:**
- [ ] `ExportTasksExcelQuery` (mismos filtros GetTasksQuery, sin paginación) + Handler → `Result<byte[]>`
- [ ] `GenerateTaskWordSummaryQuery` (TaskId) + Handler → `Result<byte[]>`

**Api:**
- [ ] `ReportsController`
  - [ ] `GET /api/reports/export-excel` [Authorize] → `File(bytes, "application/vnd.openxmlformats...", "tasks.xlsx")`
  - [ ] `GET /api/reports/tasks/{id}/word` [Authorize] → `File(bytes, "application/vnd.openxmlformats...", "task-{id}.docx")`

**Frontend:**
- [ ] `components/tasks/ExportButton.tsx` (Client: fetch → crear blob URL → click automático → revocar URL)

---

## Fase 4: Asincronía, Tiempo Real y Background

### Módulo 8: Tareas Programadas y Background (Hangfire)

**Infrastructure:**
- [ ] NuGets: `Hangfire.AspNetCore`, `Hangfire.PostgreSql`
- [ ] Configuración Hangfire en Infrastructure DI
  - [ ] `UsePostgreSqlStorage(connectionString)`
  - [ ] `AddHangfireServer()`
- [ ] `IEmailService` interface (Domain): `SendAsync(to, subject, htmlBody)`
- [ ] `EmailService` implementación
  - [ ] Dev: mock que solo hace log
  - [ ] Prod: SMTP real (configurable via appsettings)
- [ ] Registro `IEmailService` en DI

**Application — Jobs:**
- [ ] `MarkOverdueTasksJob`
  - [ ] Busca tasks: DueDate < UtcNow, State not in (Done, Removed, Overdue)
  - [ ] Llama `task.TransitionTo(TaskState.Overdue)` via dominio
  - [ ] Log cantidad de tareas marcadas
- [ ] `SendTaskAssignedEmailJob` (fire-and-forget)
  - [ ] Template: "Juan te asignó la tarea {Title}"
- [ ] `SendTaskCompletedEmailJob` (fire-and-forget)
  - [ ] Template: "{Title} fue marcada como completada por {User}"

**Registro de recurring job:**
- [ ] `MarkOverdueTasksJob` → `Cron.Daily(0, 0)` (00:00 UTC)
- [ ] Integración: disparar `SendTaskAssignedEmailJob` en `AssignTaskCommandHandler`
- [ ] Integración: disparar `SendTaskCompletedEmailJob` en `ChangeTaskStateCommandHandler` (cuando Done)

**Api:**
- [ ] `HangfireAuthorizationFilter` (solo rol Admin)
- [ ] `app.UseHangfireDashboard("/hangfire", ...)` en Program.cs

---

### Módulo 9: Comunicación Bidireccional (SignalR)

**Domain:**
- [ ] `INotificationService` interface (NotifyUserAsync, NotifyGroupAsync)

**Infrastructure:**
- [ ] `TaskHub : Hub`
  - [ ] `OnConnectedAsync`: agrega connection a grupo `user-{UserId}` (extrae userId del JWT claim)
  - [ ] `OnDisconnectedAsync`: limpia grupo
  - [ ] Requiere autenticación JWT
- [ ] `SignalRNotificationService` (implementa INotificationService via IHubContext\<TaskHub\>)
- [ ] Registro SignalR + INotificationService en DI

**Api:**
- [ ] `builder.Services.AddSignalR()` en Program.cs
- [ ] `app.MapHub<TaskHub>("/hubs/tasks")` en Program.cs
- [ ] CORS: permitir credentials para SignalR

**Application — Integración:**
- [ ] `AssignTaskCommandHandler`: llama `INotificationService.NotifyUserAsync` tras éxito
- [ ] `ChangeTaskStateCommandHandler` (Done): llama `NotifyGroupAsync("admins", ...)`

**Frontend:**
- [ ] `lib/signalr.ts` (factory: `HubConnectionBuilder` + JWT token desde session)
- [ ] `hooks/useTaskHub.ts` (connect on mount, disconnect on unmount, escucha eventos)
- [ ] `components/notifications/NotificationBell.tsx` (badge con contador no leídos)
- [ ] `components/notifications/NotificationToast.tsx` (toast al recibir push)
- [ ] `types/notifications.ts` (NotificationPayload, NotificationEvent)

---

## Fase 5: Infraestructura de Producción

### Módulo 10: Dockerización Total

- [ ] `TaskMaster.Api/Dockerfile` multi-stage
  - [ ] Stage `build`: `sdk:10.0`, restore, publish Release
  - [ ] Stage `runtime`: `aspnet:10.0`, copy publish, EXPOSE 8080
- [ ] `next.config.js`: `output: 'standalone'`
- [ ] `TaskMaster.Client/Dockerfile` standalone
  - [ ] Stage `deps`: pnpm install --frozen-lockfile
  - [ ] Stage `builder`: pnpm build
  - [ ] Stage `runner`: copy standalone + static + public
- [ ] `docker-compose.yml` completo actualizado
  - [ ] Servicio `api` (build: ./TaskMaster.Api, healthcheck)
  - [ ] Servicio `client` (build: ./TaskMaster.Client, healthcheck)
  - [ ] Servicio `azurite` (puerto 10000)
  - [ ] Todos los servicios en `taskmaster-network`
  - [ ] Variables de entorno vía `.env`
- [ ] `GET /api/health/ready` endpoint
  - [ ] Verifica conexión a PostgreSQL
  - [ ] Verifica conexión a Redis
  - [ ] Verifica acceso a Azurite

---

### Módulo 11: Orquestación con Kubernetes

- [ ] `k8s/namespace.yaml` (namespace: taskmaster)
- [ ] `k8s/configmaps/api-config.yaml` (ASPNETCORE_ENVIRONMENT, connection strings sin secrets)
- [ ] `k8s/configmaps/client-config.yaml` (NEXT_PUBLIC_API_URL, NODE_ENV)
- [ ] `k8s/secrets/db-secret.yaml` (base64: POSTGRES_PASSWORD, connection string)
- [ ] `k8s/secrets/jwt-secret.yaml` (base64: JWT_SECRET_KEY)
- [ ] `k8s/deployments/api-deployment.yaml`
  - [ ] replicas: 2
  - [ ] livenessProbe: GET /api/health (delay: 15s, period: 10s)
  - [ ] readinessProbe: GET /api/health/ready (delay: 10s, period: 5s)
  - [ ] resources: requests.cpu=250m, limits.cpu=500m, requests.memory=256Mi, limits.memory=512Mi
- [ ] `k8s/deployments/client-deployment.yaml` (replicas: 2)
- [ ] `k8s/deployments/postgres-statefulset.yaml` (StatefulSet + PVC)
- [ ] `k8s/services/api-service.yaml` (ClusterIP, port 8080)
- [ ] `k8s/services/client-service.yaml` (ClusterIP, port 3000)
- [ ] `k8s/services/postgres-service.yaml` (ClusterIP, port 5432)
- [ ] `k8s/services/redis-service.yaml` (ClusterIP, port 6379)
- [ ] `k8s/ingress/ingress.yaml` (nginx: / → client, /api → api:8080, /hubs → api:8080, /hangfire → api:8080)
- [ ] `k8s/hpa/api-hpa.yaml` (minReplicas: 2, maxReplicas: 10, targetCPU: 70%)
- [ ] `k8s/hpa/client-hpa.yaml` (minReplicas: 2, maxReplicas: 5)

---

## Observabilidad (Transversal)

- [ ] Serilog configurado (JSON formatter, CorrelationId enricher)
  - [ ] Console sink (desarrollo)
  - [ ] Nivel `Microsoft.EntityFrameworkCore`: Warning en producción
- [ ] OpenTelemetry + Prometheus exporter
  - [ ] `GET /metrics` endpoint
  - [ ] Instrumentación ASP.NET Core + Runtime
  - [ ] Meter personalizado `TaskMaster.Application`
- [ ] Health checks granulares (`/api/health/ready`)

---

## Testing

### Proyecto: TaskMaster.UnitTests

**Domain:**
- [ ] `TaskItemTests.cs`
  - [ ] `Create_WithValidData_ReturnsTaskItem`
  - [ ] `Create_WithEmptyId_ThrowsArgumentException`
  - [ ] `Create_WithEmptyTitle_ThrowsArgumentException`
  - [ ] `Create_WithNegativeEstimate_ThrowsArgumentException`
  - [ ] `TransitionTo_ToDo_ToInProgress_Succeeds`
  - [ ] `TransitionTo_Done_ToInProgress_ThrowsDomainException`
  - [ ] `TransitionTo_Removed_ToAny_ThrowsDomainException`
  - [ ] `LogWork_ValidHours_UpdatesCompletedAndRemaining`
  - [ ] `LogWork_ExceedsRemaining_ThrowsDomainException`

**Application:**
- [ ] `ValidationBehaviorTests.cs` (validators se ejecutan, errores se agregan)
- [ ] `TransactionBehaviorTests.cs` (query salta transacción, command abre/commit/rollback)
- [ ] `LoginCommandHandlerTests.cs` (mock ITokenService, mock UserManager)
- [ ] `CreateTaskCommandHandlerTests.cs` (mock IUnitOfWork, mock DbContext)
- [ ] `ChangeTaskStateCommandHandlerTests.cs`

### Proyecto: TaskMaster.IntegrationTests

- [ ] `TestDbFactory.cs` (Testcontainers, PostgreSQL efímero, ApplyMigrations)
- [ ] `IntegrationTestBase.cs` (setup/teardown, WebApplicationFactory)
- [ ] `AuthEndpointTests.cs` (register → login → refresh → logout)
- [ ] `TaskCrudTests.cs` (create → get → update → delete)
- [ ] `TaskPaginationTests.cs` (filtros, paginación, búsqueda)
- [ ] `MarkOverdueTasksJobTests.cs` (insertar tasks vencidas → ejecutar job → verificar estado)

### Proyecto: TaskMaster.E2eTests (Playwright)

- [ ] `LoginFlowTests.cs` (formulario → dashboard)
- [ ] `TaskLifecycleTests.cs` (crear → asignar → log work → completar)
- [ ] `AttachmentUploadTests.cs` (subir archivo → descargar)
- [ ] `NotificationTests.cs` (asignar tarea → verificar toast en browser del receptor)
