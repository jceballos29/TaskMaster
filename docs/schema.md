# TaskMaster Enterprise

## FASE 1: Cimientos y Arquitectura Base
*El objetivo aquí es tener ambos servidores comunicándose perfectamente y la estructura del código lista para escalar, sin tocar lógica de negocio todavía.*

**Módulo 1: El Setup Perfecto (Next.js 16 + .NET 10)**
* Creación de la solución manual usando CLI.
* Configuración del `SpaProxy` y resolución de CORS.
* Manejo de variables de entorno y el Wrapper de `fetch` para Server Components y Client Components.

**Módulo 2: Clean Architecture y Patrones Estructurales**
* División en `Domain`, `Application`, `Infrastructure` y `Api`.
* Implementación de **CQRS** con MediatR.
* Inyección de dependencias modular y validación de entrada con FluentValidation (Spec-Driven Development inicial).

**Módulo 3: Persistencia Políglota (EF Core)**
* Configuración de Entity Framework Core.
* Diseño del contexto para soportar múltiples proveedores: PostgreSQL (para desarrollo/Linux) y SQL Server (alternativa Enterprise).
* Patrón Repository (solo si es estrictamente necesario) o uso directo de `DbSet` encapsulado por casos de uso.
* Primera migración inicial usando contenedores efímeros.

---

## FASE 2: El Core del Negocio y Seguridad
*Aquí la aplicación empieza a hacer cosas útiles. Implementaremos la gestión de usuarios, roles y el CRUD avanzado de tareas.*

**Módulo 4: Autenticación y Autorización (JWT)**
* Implementación de ASP.NET Core Identity personalizado en el backend.
* Generación y validación de JWT (Access Tokens y Refresh Tokens).
* Integración en el frontend con **Auth.js (NextAuth)**.
* Políticas basadas en roles (Admin vs. Usuario) y protección de rutas en Next.js (Middleware).

**Módulo 5: Gestión de Tareas (Domain Driven)**
* Creación de entidades ricas: `Task`, `Category`, `Tag`.
* Implementación de lógica de negocio pura: asignaciones, cambios de estado, fechas de vencimiento.
* Paginación eficiente, filtros dinámicos y ordenamiento matemático en la base de datos.
* Mutaciones en Next.js usando **Server Actions** y revalidación de caché.

---

## FASE 3: Ecosistema y Procesamiento Pesado
*Elevamos la aplicación de "un simple CRUD" a una herramienta de nivel empresarial con manejo de archivos y reportes.*

**Módulo 6: Almacenamiento en la Nube (Azurite)**
* Levantamiento de Azurite (emulador de Azure Blob Storage) vía Docker.
* Desarrollo del servicio de infraestructura para subir, descargar y eliminar archivos.
* Asignación de adjuntos (PDFs, imágenes) a las tareas desde Next.js con previsualizaciones.

**Módulo 7: Procesamiento de Documentos (Excel y Word)**
* Generación de reportes dinámicos.
* Exportación masiva de tareas filtradas a Excel usando `ClosedXML`.
* Generación de contratos o resúmenes de proyectos en Word usando `OpenXML SDK`.
* Descarga de archivos como `Blob` desde Client Components en React.

---

## FASE 4: Asincronía, Tiempo Real y Background
*La aplicación ahora necesita reaccionar en tiempo real y hacer trabajos pesados sin bloquear las respuestas HTTP al usuario.*

**Módulo 8: Tareas Programadas y Background (Hangfire)**
* Integración de Hangfire con almacenamiento en base de datos.
* Creación de un *Cron Job* nocturno para marcar tareas como "Vencidas".
* Trabajos en segundo plano (Fire-and-forget): envío de correos electrónicos de resumen sin bloquear el hilo principal de la API.
* Acceso al Dashboard de Hangfire protegido por autenticación.

**Módulo 9: Comunicación Bidireccional (SignalR)**
* Configuración de Hubs en .NET 10.
* Gestión de conexiones de usuarios autenticados.
* Notificaciones push en vivo en Next.js (Ej: "Juan acaba de asignarte una nueva tarea") sin recargar la página.

---

## FASE 5: Infraestructura de Producción (DevOps)
*El código está listo. Ahora lo empaquetamos y lo preparamos para sobrevivir en un entorno de alta disponibilidad.*

**Módulo 10: Dockerización Total**
* Creación de `Dockerfile` multi-stage altamente optimizados para la API de .NET.
* Creación de `Dockerfile` standalone para Next.js.
* Orquestación local total con `docker-compose.yml` (API, Next.js, Postgres, Redis, Azurite).

**Módulo 11: Orquestación con Kubernetes (K8s)**
* Creación de manifiestos declarativos: Deployments, Services, ConfigMaps, y Secrets.
* Configuración de Ingress Controllers.
* Pruebas de escalado horizontal (HPA) y auto-recuperación de pods.

---

### La Regla del Juego

Para que este curso sea efectivo, no te daré el código completo de golpe para hacer "copiar y pegar". En cada paso te explicaré el **por qué** arquitectónico, escribiremos el código clave del backend, luego lo consumiremos en el frontend, y probaremos que la capa completa funciona antes de avanzar a la siguiente.
