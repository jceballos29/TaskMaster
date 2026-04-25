# TaskMaster Enterprise — Plan de Implementación

> **Arquitecto:** Juan Ceballos  
> **Versión:** 1.0  
> **Fecha:** 2026-04-25  
> **Stack:** .NET 10 + Next.js 16 (App Router) + PostgreSQL + Redis + Azurite + Hangfire + SignalR + Kubernetes

---

## 1. Principios Rectores

### 1.1 Clean Architecture — Regla de Dependencia

```
[ Domain ] ← [ Application ] ← [ Infrastructure ]
                                        ↑
                                     [ Api ]
```

- **Domain** no conoce ni referencia nada externo. Cero dependencias NuGet de infraestructura.
- **Application** conoce `Domain`. Depende de interfaces (`IUnitOfWork`, `IBlobStorageService`), nunca de implementaciones.
- **Infrastructure** implementa las interfaces. Conoce EF Core, Npgsql, Azure SDK, etc.
- **Api** es el punto de entrada. Registra DI y expone HTTP.

### 1.2 SOLID — Justificación Global

| Principio | Aplicación en este proyecto |
|-----------|----------------------------|
| **S** — Single Responsibility | Cada Handler tiene una sola razón de cambio. `ValidationBehavior` valida, `TransactionBehavior` gestiona transacciones, `TaskItem` encapsula invariantes del dominio. |
| **O** — Open/Closed | `IPipelineBehavior<TRequest,TResponse>` permite agregar nuevos comportamientos (logging, caching) sin modificar handlers existentes. |
| **L** — Liskov Substitution | `Result<T>` e `IResult` garantizan que cualquier resultado (Success/Failure) sea intercambiable sin romper el contrato del caller. |
| **I** — Interface Segregation | `IUnitOfWork` solo expone persistencia. `IBlobStorageService` solo expone almacenamiento. `IEmailService` solo envío. Ninguna interfaz "trampa". |
| **D** — Dependency Inversion | Application depende de `IUnitOfWork`, `IBlobStorageService`, `INotificationService` — nunca de `ApplicationDbContext`, `BlobServiceClient` ni `TaskHub` directamente. |

### 1.3 Spec-Driven Development (SDD)

Cada módulo sigue este orden estricto:

1. **Spec:** Definir el comportamiento esperado (Given/When/Then)
2. **Interface:** Declarar contratos en `Domain` o `Application`
3. **Validator:** Escribir `FluentValidation` antes del handler
4. **Handler:** Implementar lógica que hace pasar la spec
5. **Integration test:** Verificar contra DB real

### 1.4 Testabilidad

| Nivel | Herramienta | Objetivo |
|-------|------------|----------|
| Unitario | xUnit + Moq/NSubstitute | Domain entities, Application handlers (mock IUnitOfWork) |
| Integración | xUnit + Testcontainers (PostgreSQL efímero) | Handlers reales contra DB, validaciones, transacciones |
| E2E | Playwright | Flujos completos desde el navegador |

---

## 2. Estado Actual del Proyecto

> Snapshot al 2026-04-25 — confirmado leyendo código fuente.

### ✅ COMPLETADO

| Módulo | Componente | Archivo(s) |
|--------|-----------|------------|
| M1 | Solution structure (.NET 10 + Next.js 16) | `*.csproj`, `package.json` |
| M1 | Fetch wrapper SSR/CSR | `lib/api.ts` |
| M1 | Variables de entorno | `.env`, `appsettings.json` |
| M1 | docker-compose (PostgreSQL + Redis) | `docker-compose.yml` |
| M1 | App Router base (layout, page, error) | `app/*.tsx` |
| M2 | Clean Architecture layer division | `TaskMaster.{Domain,Application,Infrastructure,Api}` |
| M2 | CQRS con MediatR | `GetSystemInfoQuery + Handler` |
| M2 | ValidationBehavior (FluentValidation pipeline) | `Behaviors/ValidationBehavior.cs` |
| M2 | TransactionBehavior + ICommand markers | `Behaviors/TransactionBehavior.cs`, `ICommand.cs` |
| M2 | Result<T> pattern (Success/Failure) | `Models/Result.cs` |
| M2 | DI modular | `Application/DependencyInjection.cs`, `Infrastructure/DependencyInjection.cs` |
| M3 | EF Core + PostgreSQL + snake_case | `Infrastructure/DependencyInjection.cs` |
| M3 | ApplicationDbContext + config scan | `Persistence/ApplicationDbContext.cs` |
| M3 | BaseEntity + Enums (TaskState, Priority, Activity) | `Domain/Entities/`, `Domain/Enum/` |
| M3 | TaskItem (entidad rica, factory `Create`) | `Domain/Entities/TaskItem.cs` |
| M3 | TaskItemConfiguration (IEntityTypeConfiguration) | `Persistence/Configurations/TaskItemConfiguration.cs` |
| M3 | UnitOfWork con transacciones | `Persistence/UnitOfWork.cs` |
| M3 | InitialCreate migration | `Migrations/20260425155600_InitialCreate.cs` |
| M3 | DomainException + InvalidCredentialsException | `Domain/Exceptions/` |

### ❌ PENDIENTE EN M1-M3

| Componente | Justificación |
|-----------|--------------|
| Global exception middleware | Sin `UseExceptionHandler`, errores no manejados devuelven 500 sin formato consistente |
| CORS explícito en `Program.cs` | Necesario para producción (SpaProxy solo funciona en dev) |

---

## 3. Plan Detallado por Fase

---

### FASE 1 — Cimientos y Arquitectura Base (Módulos 1-3)

> Estado: **95% completo**. Solo falta exception middleware.

#### Deuda Técnica Pendiente: Global Exception Middleware

**Spec:**
```
DADO cualquier excepción no manejada en el pipeline
CUANDO el middleware la intercepta
ENTONCES responde con JSON { "errorCode": "...", "message": "..." }
y el HTTP status code correcto (400, 401, 404, 422, 500)
```

**Implementación:**
```
TaskMaster.Api/Middleware/ExceptionHandlingMiddleware.cs
```

Mapa de excepciones → HTTP status:
- `ValidationException` (FluentValidation) → 422 Unprocessable Entity
- `DomainException` → usa `HttpStatuCode` del objeto
- `KeyNotFoundException` → 404
- `UnauthorizedAccessException` → 401
- `Exception` (fallback) → 500

**Registro en `Program.cs`:**
```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**CORS (producción):**
```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("NextJsPolicy", policy =>
        policy.WithOrigins(config["Frontend:Url"]!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));
app.UseCors("NextJsPolicy");
```

---

### FASE 2 — Core del Negocio y Seguridad (Módulos 4-5)

---

#### Módulo 4: Autenticación y Autorización (JWT)

**Objetivo:** Registro, login, JWT (access + refresh), roles (Admin/User), rutas protegidas.

**SDD — Specs clave:**
```
DADO un email no registrado y password válido
CUANDO POST /api/auth/register
ENTONCES se crea usuario, se devuelve 201 con { userId, email }

DADO credenciales válidas
CUANDO POST /api/auth/login
ENTONCES se devuelven AccessToken (15min) + RefreshToken (7d) + userId

DADO un AccessToken vencido y RefreshToken válido
CUANDO POST /api/auth/refresh
ENTONCES se emite nuevo AccessToken y rota el RefreshToken (Refresh Token Rotation)

DADO un AccessToken inválido
CUANDO GET /api/tasks (endpoint protegido)
ENTONCES 401 Unauthorized con { errorCode: "UNAUTHORIZED" }
```

**Domain — Nuevas entidades:**

`TaskMaster.Domain/Entities/ApplicationUser.cs`
```
Hereda de IdentityUser<Guid>
Propiedades adicionales:
  - string DisplayName
  - DateTime CreatedAt
  - ICollection<RefreshToken> RefreshTokens
```

`TaskMaster.Domain/Entities/RefreshToken.cs`
```
- Guid Id
- string Token (hash SHA-256)
- DateTime ExpiresAt
- bool IsRevoked
- Guid UserId (FK)
```

`TaskMaster.Domain/Enums/UserRole.cs`
```
Admin = "Admin"
User = "User"
```

**Application — Commands y Queries:**

```
Application/Auth/
  Commands/
    Register/
      RegisterCommand.cs          (record: Email, Password, DisplayName)
      RegisterCommandValidator.cs (email válido, password ≥ 8 chars)
      RegisterCommandHandler.cs
    Login/
      LoginCommand.cs             (record: Email, Password)
      LoginCommandValidator.cs
      LoginCommandHandler.cs      → devuelve LoginResult (AccessToken, RefreshToken, UserId)
    Refresh/
      RefreshTokenCommand.cs      (record: RefreshToken)
      RefreshTokenCommandHandler.cs
    Logout/
      LogoutCommand.cs            (record: UserId, RefreshToken)
      LogoutCommandHandler.cs
  Common/
    Models/
      AuthResult.cs               (AccessToken, RefreshToken, ExpiresAt, UserId)
    Interfaces/
      ITokenService.cs            (GenerateAccessToken, GenerateRefreshToken, ValidateRefreshToken)
      IPasswordHasher.cs          (solo si se necesita abstracción extra)
```

**Infrastructure — Implementaciones:**

```
Infrastructure/
  Identity/
    ApplicationUserConfiguration.cs    (IEntityTypeConfiguration)
    RefreshTokenConfiguration.cs
  Services/
    JwtTokenService.cs                  (implementa ITokenService)
      - Usa Microsoft.IdentityModel.Tokens
      - Access token: 15 min, claims: sub, email, role, jti
      - Refresh token: GUID seguro + hash SHA-256 almacenado en DB
```

**Api:**

```
Api/
  Controllers/
    AuthController.cs
      POST /api/auth/register
      POST /api/auth/login
      POST /api/auth/refresh
      POST /api/auth/logout     [Authorize]
      GET  /api/auth/me         [Authorize]
```

**EF Core — Configuración:**

En `ApplicationDbContext`:
- Hereda de `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- `DbSet<RefreshToken> RefreshTokens`

Nueva migración: `AddIdentityAndRefreshTokens`

**Frontend — Auth.js v5:**

```
TaskMaster.Client/
  auth.ts                    (NextAuth config: JWT provider + /api/auth/login)
  middleware.ts              (protege rutas /dashboard/*, /tasks/*)
  app/
    (auth)/
      login/page.tsx
      register/page.tsx
  lib/
    actions/
      auth.ts                (Server Actions: signIn, signOut wrappers)
  types/
    auth.ts                  (Session, UserRole)
```

**SOLID aplicado:**
- **S:** `JwtTokenService` solo genera/valida tokens. `LoginCommandHandler` solo orquesta login.
- **D:** Application usa `ITokenService`, nunca `JwtTokenService` directamente.
- **I:** `ITokenService` no expone nada de EF Core ni HTTP context.

**Observabilidad:**
- Log `Information` en login exitoso (userId, timestamp)
- Log `Warning` en refresh token inválido (token hash, IP)
- Log `Warning` en login fallido (email intentado — sin exponer password)
- Métrica: contador `auth.login.success`, `auth.login.failure`, `auth.token.refresh`

---

#### Módulo 5: Gestión de Tareas (Domain Driven)

**Objetivo:** CRUD completo de tareas con estados, asignaciones, estimaciones, categorías, tags, paginación y filtros.

**SDD — Specs clave:**
```
DADO un usuario autenticado y datos válidos
CUANDO POST /api/tasks
ENTONCES se crea TaskItem con State=ToDo, se devuelve 201 con taskId

DADO un TaskItem en estado ToDo
CUANDO cambiar a InProgress
ENTONCES se registra la transición en dominio, UpdatedAt se actualiza

DADO un TaskItem en estado Done
CUANDO intentar cambiar a InProgress
ENTONCES DomainException "INVALID_STATE_TRANSITION" (state machine)

DADO LogWork con hours > RemainingWork
CUANDO LogWork command
ENTONCES DomainException "WORK_EXCEEDS_REMAINING"

DADO filtros (state, priority, assignedTo, categoryId, search)
CUANDO GET /api/tasks?page=1&pageSize=20&state=InProgress
ENTONCES retorna IEnumerable<TaskSummaryDto> paginado con totalCount
```

**Domain — Nuevas entidades:**

`TaskMaster.Domain/Entities/Category.cs`
```
- Guid Id
- string Name (unique)
- string? Color (hex)
- ICollection<TaskItem> Tasks
```

`TaskMaster.Domain/Entities/Tag.cs`
```
- Guid Id
- string Name
- string? Color
- ICollection<TaskItemTag> TaskItemTags (tabla de unión)
```

`TaskMaster.Domain/Entities/TaskItem.cs` — **Extensión con métodos de dominio:**
```
Propiedades adicionales:
  - Guid? AssignedToId (FK ApplicationUser)
  - Guid? CategoryId (FK Category)
  - DateTime? DueDate
  - ICollection<Tag> Tags
  - ICollection<TaskAttachment> Attachments (Módulo 6)

Métodos de dominio:
  + TransitionTo(TaskState newState): void
    → State machine: valida transiciones permitidas
    → UpdateTimestamp()
  + LogWork(int hours): void
    → CompletedWork += hours
    → RemainingWork = max(0, RemainingWork - hours)
  + AssignTo(Guid userId): void
  + Unassign(): void
  + SetDueDate(DateTime dueDate): void
  + AddTag(Tag tag): void
  + RemoveTag(Guid tagId): void
```

**State Machine (TaskState):**
```
ToDo       → InProgress, Removed
InProgress → Done, ToDo, Removed
Done       → (inmutable — no transiciones hacia atrás)
Removed    → (inmutable)
```

**Application — Commands:**
```
Application/Tasks/
  Commands/
    CreateTask/
      CreateTaskCommand.cs         (Title, Description?, OriginalEstimate, Priority, CategoryId?, DueDate?)
      CreateTaskCommandValidator.cs
      CreateTaskCommandHandler.cs  → devuelve Result<Guid>
    UpdateTask/
      UpdateTaskCommand.cs
      UpdateTaskCommandValidator.cs
      UpdateTaskCommandHandler.cs
    DeleteTask/
      DeleteTaskCommand.cs         (TaskId) — soft delete (State = Removed)
      DeleteTaskCommandHandler.cs
    ChangeTaskState/
      ChangeTaskStateCommand.cs    (TaskId, NewState)
      ChangeTaskStateCommandHandler.cs
    AssignTask/
      AssignTaskCommand.cs         (TaskId, UserId)
      AssignTaskCommandHandler.cs
    LogWork/
      LogWorkCommand.cs            (TaskId, Hours)
      LogWorkCommandValidator.cs
      LogWorkCommandHandler.cs
  Queries/
    GetTasks/
      GetTasksQuery.cs             (Page, PageSize, State?, Priority?, AssignedToId?, CategoryId?, Search?)
      GetTasksQueryHandler.cs      → devuelve Result<PagedResult<TaskSummaryDto>>
    GetTaskById/
      GetTaskByIdQuery.cs          (TaskId)
      GetTaskByIdQueryHandler.cs   → devuelve Result<TaskDetailDto>
  Common/
    DTOs/
      TaskSummaryDto.cs
      TaskDetailDto.cs
    Models/
      PagedResult.cs               (Items, TotalCount, Page, PageSize, TotalPages)
```

**Infrastructure:**
```
Persistence/
  Configurations/
    CategoryConfiguration.cs
    TagConfiguration.cs
    TaskItemTagConfiguration.cs    (tabla de unión)
```

Nueva migración: `AddCategoriesTagsAndUserRelations`

**Api:**
```
Controllers/
  TasksController.cs
    POST   /api/tasks              [Authorize]
    GET    /api/tasks              [Authorize]
    GET    /api/tasks/{id}         [Authorize]
    PUT    /api/tasks/{id}         [Authorize]
    DELETE /api/tasks/{id}         [Authorize]
    PATCH  /api/tasks/{id}/state   [Authorize]
    PATCH  /api/tasks/{id}/assign  [Authorize(Roles=Admin)]
    POST   /api/tasks/{id}/log-work[Authorize]
  CategoriesController.cs
    CRUD básico de categorías     [Authorize(Roles=Admin)]
```

**Frontend — Server Actions + Components:**
```
app/
  (dashboard)/
    tasks/
      page.tsx                    (Server Component: lista con filtros)
      [id]/page.tsx               (detail)
      new/page.tsx
  components/
    tasks/
      TaskList.tsx                (Client Component: filtros interactivos)
      TaskCard.tsx
      TaskFilters.tsx
      TaskStateSelector.tsx
lib/
  actions/
    tasks.ts                      (Server Actions: createTask, updateTask, deleteTask, logWork)
```

**SOLID aplicado:**
- **S:** `GetTasksQueryHandler` solo consulta. `ChangeTaskStateCommandHandler` solo transiciona.
- **O:** La state machine en `TaskItem.TransitionTo()` es extensible sin modificar handlers.
- **L:** `TaskDetailDto` y `TaskSummaryDto` son contratos independientes — no se mezclan.
- **D:** Handlers usan `IUnitOfWork`, no `ApplicationDbContext`.

**Observabilidad:**
- Log `Information` en creación, cambio de estado, asignación
- Log `Warning` en transición inválida (estado actual, estado solicitado, taskId)
- Métrica: contador `tasks.created`, `tasks.state_change`, `tasks.completed`
- Métrica gauge: `tasks.overdue_count` (actualizada por job de Módulo 8)

---

### FASE 3 — Ecosistema y Procesamiento Pesado (Módulos 6-7)

---

#### Módulo 6: Almacenamiento en la Nube (Azurite)

**Objetivo:** Upload/download de adjuntos en tareas usando Azure Blob Storage emulado.

**SDD — Specs:**
```
DADO un archivo PDF < 10MB y una tarea existente
CUANDO POST /api/tasks/{id}/attachments (multipart/form-data)
ENTONCES se sube a Azurite, se crea TaskAttachment en DB, devuelve attachmentId + URL firmada

DADO un attachmentId válido
CUANDO GET /api/attachments/{id}/download
ENTONCES se devuelve el archivo con Content-Disposition: attachment

DADO un attachmentId válido y el usuario propietario
CUANDO DELETE /api/attachments/{id}
ENTONCES se elimina del blob y de la DB
```

**docker-compose — Azurite:**
```yaml
azurite:
  image: mcr.microsoft.com/azure-storage/azurite
  container_name: azurite
  ports:
    - "10000:10000"  # Blob service
  command: azurite-blob --blobHost 0.0.0.0
  volumes:
    - azurite_data:/data
```

**Domain:**
```
Entities/TaskAttachment.cs
  - Guid Id
  - Guid TaskId
  - string FileName
  - string ContentType
  - long FileSizeBytes
  - string BlobUri (referencia al blob — no URL pública)
  - DateTime UploadedAt
  - Guid UploadedByUserId

Interfaces/IBlobStorageService.cs
  - Task<BlobUploadResult> UploadAsync(string containerName, string blobName, Stream content, string contentType)
  - Task<Stream> DownloadAsync(string containerName, string blobName)
  - Task DeleteAsync(string containerName, string blobName)
  - Task<bool> ExistsAsync(string containerName, string blobName)
```

**Application:**
```
Tasks/Commands/
  UploadAttachment/
    UploadAttachmentCommand.cs   (TaskId, FileName, ContentType, FileStream)
    UploadAttachmentCommandValidator.cs
    UploadAttachmentCommandHandler.cs

Tasks/Commands/
  DeleteAttachment/
    DeleteAttachmentCommand.cs
    DeleteAttachmentCommandHandler.cs

Tasks/Queries/
  GetAttachmentDownload/
    GetAttachmentDownloadQuery.cs
    GetAttachmentDownloadQueryHandler.cs → Result<AttachmentDownloadDto> (Stream + ContentType + FileName)
```

**Infrastructure:**
```
Services/BlobStorageService.cs
  - Azure.Storage.Blobs SDK
  - Reads: AzureBlobStorage:ConnectionString, AzureBlobStorage:ContainerName
```

**Api:**
```
Controllers/AttachmentsController.cs
  POST   /api/tasks/{taskId}/attachments  [Authorize]
  GET    /api/attachments/{id}/download   [Authorize]
  DELETE /api/attachments/{id}            [Authorize]
```

**Frontend:**
```
components/tasks/AttachmentUploader.tsx   (Client Component: drag-and-drop)
components/tasks/AttachmentList.tsx       (Client Component: lista + preview PDF/imagen)
lib/actions/attachments.ts               (Server Actions: upload, delete)
```

**SOLID:** `IBlobStorageService` en Domain — Application no conoce Azure SDK.

**Observabilidad:**
- Log `Information` en upload exitoso (taskId, fileName, sizeBytes, blobUri)
- Log `Error` en fallo de upload con mensaje de Azure SDK
- Métrica: `storage.upload.bytes`, `storage.download.count`

---

#### Módulo 7: Procesamiento de Documentos (Excel y Word)

**Objetivo:** Exportar tareas filtradas a Excel y generar resúmenes en Word.

**SDD — Specs:**
```
DADO un conjunto de filtros de tareas
CUANDO GET /api/reports/export-excel?state=InProgress&categoryId=xxx
ENTONCES se devuelve archivo .xlsx con columnas: Title, State, Priority, AssignedTo, DueDate, EstimatedHours

DADO una tarea con ID válido
CUANDO GET /api/reports/tasks/{id}/word-summary
ENTONCES se devuelve documento .docx con título, descripción, historial de trabajo y adjuntos
```

**Domain:**
```
Interfaces/IReportService.cs
  - Task<byte[]> GenerateTasksExcelAsync(IEnumerable<TaskDetailDto> tasks)
  - Task<byte[]> GenerateTaskWordSummaryAsync(TaskDetailDto task)
```

**Infrastructure:**
```
Services/ExcelReportService.cs   (ClosedXML)
Services/WordReportService.cs    (DocumentFormat.OpenXml)
```

**Application:**
```
Reports/Queries/
  ExportTasksExcel/
    ExportTasksExcelQuery.cs     (mismos filtros que GetTasksQuery pero sin paginación)
    ExportTasksExcelQueryHandler.cs
  GenerateTaskWordSummary/
    GenerateTaskWordSummaryQuery.cs
    GenerateTaskWordSummaryQueryHandler.cs
```

**Api:**
```
Controllers/ReportsController.cs
  GET /api/reports/export-excel      [Authorize]
  GET /api/reports/tasks/{id}/word   [Authorize]
  → Devuelve File(bytes, contentType, fileName)
```

**Frontend:**
```
components/tasks/ExportButton.tsx    (Client Component: descarga blob)
lib/actions/reports.ts
```

**Observabilidad:**
- Log `Information`: `{UserEmail} exported {TaskCount} tasks to Excel`
- Métrica: `reports.excel.generated`, `reports.word.generated`

---

### FASE 4 — Asincronía, Tiempo Real y Background (Módulos 8-9)

---

#### Módulo 8: Tareas Programadas y Background (Hangfire)

**Objetivo:** Jobs nocturnos, fire-and-forget, dashboard protegido.

**SDD — Specs:**
```
DADO tareas con DueDate < DateTime.UtcNow y State != Done y State != Removed
CUANDO el job MarkOverdueTasksJob ejecuta (00:00 UTC diario)
ENTONCES State cambia a Overdue (nuevo estado) y se registra log de transición

DADO un evento de tarea completada
CUANDO se dispara un job de notificación email
ENTONCES IEmailService.SendAsync se llama con el template correcto
  sin bloquear la respuesta HTTP (fire-and-forget)

DADO un request al dashboard /hangfire
CUANDO el usuario NO es Admin
ENTONCES 403 Forbidden
```

**Domain:**
```
Enum/TaskState.cs → agregar: Overdue = 5

Interfaces/IEmailService.cs
  - Task SendAsync(string to, string subject, string htmlBody)
```

**Application:**
```
Jobs/
  MarkOverdueTasksJob.cs
  Notifications/
    SendTaskAssignedEmailJob.cs
    SendTaskCompletedEmailJob.cs
```

**Infrastructure:**
```
Services/EmailService.cs            (SMTP o mock en dev — IEmailService)
Hangfire/
  HangfireAuthorizationFilter.cs    (IDashboardAuthorizationFilter)
```

**DI en Infrastructure:**
```csharp
services.AddHangfire(config => config.UsePostgreSqlStorage(connString));
services.AddHangfireServer();
services.AddScoped<IEmailService, EmailService>();
services.AddTransient<MarkOverdueTasksJob>();

// Cron job nocturno (registrar en IHostedService o startup):
RecurringJob.AddOrUpdate<MarkOverdueTasksJob>(
    "mark-overdue-tasks",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.Daily(0, 0));
```

**Api:**
```csharp
app.UseHangfireDashboard("/hangfire", new DashboardOptions {
    Authorization = [new HangfireAuthorizationFilter()]
});
```

**Observabilidad:**
- Log `Information` al inicio/fin de cada job con cantidad de registros afectados
- Log `Error` si falla con excepción detallada (Hangfire ya reintenta)
- Métrica: `jobs.overdue_tasks.count`, `jobs.email.sent`, `jobs.email.failed`

---

#### Módulo 9: Comunicación Bidireccional (SignalR)

**Objetivo:** Notificaciones push en vivo a usuarios conectados.

**SDD — Specs:**
```
DADO usuario A asigna una tarea a usuario B
CUANDO AssignTaskCommandHandler completa la asignación
ENTONCES B recibe notificación en tiempo real: { type: "TASK_ASSIGNED", taskId, taskTitle, assignedBy }

DADO usuario con conexiones SignalR activas
CUANDO se desconecta
ENTONCES su connectionId se limpia del registro de grupos
```

**Domain:**
```
Interfaces/INotificationService.cs
  - Task NotifyUserAsync(Guid userId, string eventType, object payload)
  - Task NotifyGroupAsync(string groupName, string eventType, object payload)
```

**Infrastructure:**
```
Hubs/TaskHub.cs
  - Hereda Hub
  - OnConnectedAsync: agrega connectionId a grupo por userId
  - OnDisconnectedAsync: limpia grupo
  - Autentica via JWT (ConnectionId mapping)

Services/SignalRNotificationService.cs  (implementa INotificationService)
  - Inyecta IHubContext<TaskHub>
```

**Application — Integración con handlers:**

En `AssignTaskCommandHandler`, después de éxito:
```csharp
await _notificationService.NotifyUserAsync(command.UserId, "TASK_ASSIGNED", new { taskId, taskTitle, assignedBy });
```

En `ChangeTaskStateCommandHandler` (a Done):
```csharp
await _notificationService.NotifyGroupAsync("admins", "TASK_COMPLETED", new { taskId, completedBy });
```

**Api:**
```csharp
app.MapHub<TaskHub>("/hubs/tasks");
builder.Services.AddSignalR();
```

**Frontend:**
```
hooks/useTaskHub.ts              (Client hook: conecta, escucha eventos, disconnects on unmount)
components/notifications/
  NotificationBell.tsx           (Client Component: badge con contador)
  NotificationToast.tsx          (toast al recibir evento)
lib/signalr.ts                   (factory: crea HubConnection con JWT token)
```

**SOLID:** `INotificationService` en Domain — handlers no conocen SignalR ni IHubContext.

**Observabilidad:**
- Log `Information`: `{UserId} connected to TaskHub (connectionId: {ConnectionId})`
- Log `Information`: `Notification sent to {UserId} for event {EventType}`
- Métrica: `signalr.connections.active`, `signalr.messages.sent`

---

### FASE 5 — Infraestructura de Producción (Módulos 10-11)

---

#### Módulo 10: Dockerización Total

**Objetivo:** Imágenes optimizadas, orquestación local completa.

**Dockerfile .NET (multi-stage):**
```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.sln .
COPY TaskMaster.Api/*.csproj TaskMaster.Api/
COPY TaskMaster.Application/*.csproj TaskMaster.Application/
COPY TaskMaster.Domain/*.csproj TaskMaster.Domain/
COPY TaskMaster.Infrastructure/*.csproj TaskMaster.Infrastructure/
RUN dotnet restore
COPY . .
RUN dotnet publish TaskMaster.Api -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskMaster.Api.dll"]
```

**Dockerfile Next.js (standalone):**
```dockerfile
FROM node:22-alpine AS deps
WORKDIR /app
COPY package.json pnpm-lock.yaml .
RUN corepack enable && pnpm install --frozen-lockfile

FROM node:22-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules .
COPY . .
RUN pnpm build

FROM node:22-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
COPY --from=builder /app/.next/standalone .
COPY --from=builder /app/.next/static .next/static
COPY --from=builder /app/public public
EXPOSE 3000
CMD ["node", "server.js"]
```

**docker-compose.yml completo:**
Servicios: `api`, `client`, `postgresql`, `redis`, `azurite`, `hangfire-dashboard`

Requiere `next.config.js` con `output: 'standalone'`.

---

#### Módulo 11: Orquestación con Kubernetes

**Objetivo:** Manifiestos declarativos para despliegue en clúster K8s.

**Estructura:**
```
k8s/
  namespace.yaml
  configmaps/
    api-config.yaml
    client-config.yaml
  secrets/
    db-secret.yaml       (base64-encoded)
    jwt-secret.yaml
  deployments/
    api-deployment.yaml       (replicas: 2, livenessProbe: /api/health)
    client-deployment.yaml    (replicas: 2)
    postgres-deployment.yaml  (StatefulSet)
    redis-deployment.yaml
  services/
    api-service.yaml          (ClusterIP)
    client-service.yaml       (ClusterIP)
    postgres-service.yaml
    redis-service.yaml
  ingress/
    ingress.yaml              (nginx ingress: / → client, /api → api)
  hpa/
    api-hpa.yaml              (minReplicas: 2, maxReplicas: 10, CPU: 70%)
    client-hpa.yaml
```

**Observabilidad en K8s:**
- Liveness probe: `GET /api/health`
- Readiness probe: `GET /api/health/ready` (nuevo endpoint que verifica DB)
- Resource limits: `requests.cpu: 250m`, `limits.cpu: 500m`

---

## 4. Estrategia de Testing

### Por Módulo

| Módulo | Unitario | Integración | E2E |
|--------|----------|------------|-----|
| M2 (CQRS) | ValidationBehavior, TransactionBehavior | GetSystemInfoHandler vs DB real | - |
| M4 (Auth) | JwtTokenService (genera/valida), validators | Register/Login handlers vs DB+Identity | Login form → dashboard |
| M5 (Tasks) | TaskItem state machine, LogWork invariants | CreateTask, GetTasks paginado | Create task → assign → complete |
| M6 (Blobs) | - | Upload/download vs Azurite real | Upload from UI → download |
| M8 (Hangfire) | MarkOverdueTasksJob logic | Job execution vs test DB | - |
| M9 (SignalR) | NotificationService mock | Hub connection + message receipt | Assignment → toast appears |

### Infraestructura de Tests

```
TaskMaster.UnitTests/
  Domain/
    TaskItemTests.cs                (state machine, LogWork invariants, factory validation)
  Application/
    Behaviors/
      ValidationBehaviorTests.cs
      TransactionBehaviorTests.cs
    Auth/
      LoginCommandHandlerTests.cs
    Tasks/
      CreateTaskCommandHandlerTests.cs
      ChangeTaskStateCommandHandlerTests.cs

TaskMaster.IntegrationTests/
  Common/
    TestDbFactory.cs               (Testcontainers: PostgreSQL efímero)
    IntegrationTestBase.cs
  Auth/
    AuthEndpointTests.cs
  Tasks/
    TaskCrudTests.cs
    TaskPaginationTests.cs

TaskMaster.E2eTests/
  Auth/
    LoginFlowTests.cs              (Playwright)
  Tasks/
    TaskLifecycleTests.cs
```

---

## 5. Observabilidad — Estrategia Transversal

### Logging (Serilog)

```csharp
// En Program.cs
builder.Host.UseSerilog((ctx, config) =>
    config
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.WithCorrelationId()
        .Enrich.WithProperty("Application", "TaskMaster.Api")
        .WriteTo.Console(new RenderedCompactJsonFormatter())
        .WriteTo.PostgreSQL(...));  // para correlación con datos
```

Niveles por namespace:
- `Microsoft.EntityFrameworkCore`: `Warning` (silenciar SQL en prod)
- `Hangfire`: `Information`
- `TaskMaster.*`: `Information` en prod, `Debug` en dev

### Métricas (OpenTelemetry)

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("TaskMaster.Application")
        .AddPrometheusExporter());
```

Endpoint: `GET /metrics` (Prometheus scraping)

### Health Checks

```
/api/health           → liveness (siempre ok si el proceso corre)
/api/health/ready     → readiness (PostgreSQL, Redis, Azurite alcanzables)
```

---

## 6. Compatibilidad Retroactiva

- **Contratos HTTP:** Se agrega versioning `api/v1/` antes de producción. Módulos existentes no se renombran.
- **Migraciones EF:** Solo migraciones aditivas (nuevas tablas, nuevas columnas nullable). Nunca `DropTable` sin script de datos.
- **Result<T>:** El patrón no cambia. Nuevos handlers simplemente lo adoptan.
- **ICommand / IQuery:** Los marker interfaces no se modifican — solo se extienden si hay necesidad futura justificada.
- **Enums:** Solo agregar valores al final. Cambiar valores existentes rompe la DB (registros como string).
