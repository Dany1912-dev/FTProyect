# FTPCloud — Gestor de archivos self-hosted

> **Proyecto personal / hobby** — un gestor de archivos estilo Google Drive para uso propio, familia y amigos cercanos. Corre en un servidor Proxmox propio y se accede desde el navegador. El nombre dice "FTP" pero en realidad todo funciona sobre HTTP con una API REST — el protocolo FTP quedó descartado desde el inicio.

![Vue 3](https://img.shields.io/badge/Vue-3-42B883?logo=vuedotjs&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-6.0-3178C6?logo=typescript&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

---

## Estado del proyecto

Funcional de extremo a extremo — frontend y backend implementados y corriendo en Docker. No está pensado para producción pública ni para muchos usuarios: es un proyecto de hobby para uso personal, familia o amigos en un entorno self-hosted. Cosas como rate limiting, auditoría de seguridad exhaustiva o alta disponibilidad están fuera del alcance intencionalmente.

---

## ¿Qué hace?

Un explorador de archivos web alojado en casa. Sin depender de Google Drive, Dropbox ni ningún servicio de terceros. Los archivos viven en el servidor propio.

**Funcionalidades:**
- Explorar, subir, descargar, renombrar, mover y eliminar archivos y carpetas
- Subcarpetas anidadas con navegación por breadcrumb
- Subida de archivos por chunks con progreso en tiempo real (reanudable si se corta la conexión)
- Vista previa de archivos directamente en el navegador
- Papelera con restauración y eliminación permanente
- Buscador global de archivos y carpetas
- Compartir archivos directamente con otro usuario (con rol editor o viewer)
- Carpetas grupales colaborativas con miembros invitados
- Gestión de usuarios con cuotas de almacenamiento por usuario
- Sistema de roles: owner, admin, usuario normal
- Sesiones persistentes con refresh automático de token

---

## Roles y permisos

```
owner  (exactamente 1, creado automáticamente al arrancar)
  ├── Panel con todos los usuarios del sistema
  ├── Crea admins y usuarios normales
  ├── Edita cuotas de almacenamiento por usuario
  ├── Cambia roles y contraseñas de cualquier usuario
  ├── Elimina usuarios
  └── Sin acceso al explorador de archivos (es el gestor del sistema)

admin
  ├── Crea usuarios normales
  └── Accede a: archivos personales, compartidos y grupos

user
  └── Accede a: archivos personales, compartidos y grupos
```

### Tipos de carpeta

| Tipo | Descripción |
|------|-------------|
| **personal** | Solo el dueño. Soporta subcarpetas anidadas |
| **shared** | Archivos que otros usuarios te compartieron directamente |
| **group** | Carpeta colaborativa — el dueño invita miembros con rol `editor` o `viewer` |

---

## Arquitectura

El backend sigue **Clean Architecture** separado en cuatro proyectos:

```
FTProyect/
│
├── ftpcloud-api/                           # Backend — ASP.NET Core 9
│   └── src/
│       ├── FtpCloud.Domain/                # Entidades y enums (sin dependencias externas)
│       │   ├── Entities/
│       │   │   ├── User.cs
│       │   │   ├── Folder.cs               # Soporta anidamiento y soft delete
│       │   │   ├── FileEntity.cs           # Soft delete, ruta de almacenamiento
│       │   │   ├── FolderMember.cs         # Editor / viewer en grupos
│       │   │   ├── FileShare.cs            # Compartir archivo con usuario específico
│       │   │   └── RefreshToken.cs
│       │   └── Enums/ (UserRole, FolderType, FolderMemberRole)
│       │
│       ├── FtpCloud.Application/           # Servicios, interfaces, DTOs
│       │   ├── Services/
│       │   │   ├── AuthService.cs          # Login, refresh, logout, cambio de contraseña
│       │   │   ├── UserService.cs          # CRUD de usuarios + gestión de cuotas
│       │   │   └── FileService.cs          # Toda la lógica de archivos y carpetas
│       │   ├── Interfaces/                 # Contratos de repositorios y servicios
│       │   └── Dtos/                       # Objetos de transferencia para todos los endpoints
│       │
│       ├── FtpCloud.Infrastructure/        # Implementaciones concretas
│       │   ├── Persistence/
│       │   │   ├── FtpCloudDbContext.cs
│       │   │   ├── Configurations/         # Fluent API por entidad
│       │   │   ├── Migrations/             # 5 migraciones EF Core
│       │   │   └── Repositories/           # User, Folder, File, RefreshToken
│       │   ├── Security/
│       │   │   ├── JwtService.cs           # Generación y validación de tokens
│       │   │   └── BCryptPasswordHasher.cs
│       │   └── Storage/
│       │       └── LocalFileStorage.cs     # Lectura/escritura de archivos en disco
│       │
│       └── FtpCloud.Api/                   # Controllers, middleware, configuración
│           ├── Controllers/
│           │   ├── AuthController.cs       # /api/auth — login, logout, refresh, me, change-password
│           │   ├── UsersController.cs      # /api/users — CRUD + cuotas + roles
│           │   └── FilesController.cs      # /api/files — explorador, trash, search, shares
│           ├── Common/                     # Base controller, ApiResponse<T>, CookieHelper
│           ├── Middleware/
│           │   └── ExceptionHandlingMiddleware.cs
│           ├── Seed/
│           │   └── OwnerSeeder.cs          # Crea el owner en el primer arranque
│           └── Services/
│               └── TusCleanupService.cs    # Limpia uploads incompletos > 24h
│
├── ftpcloud-ui/                            # Frontend — Vue 3 + TypeScript + Vite
│   └── src/
│       ├── views/
│       │   ├── auth/LoginView.vue
│       │   ├── owner/DashboardView.vue     # Panel de gestión de usuarios
│       │   ├── admin/UsersView.vue         # Tabla de usuarios para admin
│       │   └── files/
│       │       ├── MyFilesView.vue         # Explorador personal con subcarpetas
│       │       ├── SharedView.vue          # Archivos compartidos conmigo
│       │       ├── GroupsView.vue          # Carpetas grupales
│       │       ├── TrashView.vue           # Papelera
│       │       └── SearchResultsView.vue  # Resultados de búsqueda
│       ├── components/
│       │   ├── files/                      # UploadQueue, FilePreview, FileShare, Move, Rename...
│       │   ├── users/                      # CreateUser, EditQuota, ChangeRole, ChangePassword
│       │   └── common/                     # ConfirmDialog, ChangePasswordModal
│       ├── composables/
│       │   ├── useFileUpload.ts            # Lógica de subida TUS con cola de progreso
│       │   └── useUserSearch.ts            # Búsqueda de usuarios para compartir
│       ├── stores/ (auth, files, users, dialog)
│       ├── services/
│       │   ├── api.ts                      # Wrapper fetch: cookies, retry en 401, refresh automático
│       │   └── tus.ts                      # Cliente TUS (chunks de 8MB, reintentos, progreso)
│       └── router/index.ts                 # Guards por rol (requiresAuth, requiresAdmin, requiresOwner)
│
├── docker-compose.yml                      # 4 servicios: db, api, ui, cloudflared
└── .env.example                            # Variables de entorno requeridas
```

**Infraestructura:**

```
Servidor físico (Proxmox)
  └── VM Ubuntu Server
        └── Docker Compose
              ├── PostgreSQL 16    → datos persistidos en volumen Docker
              ├── ftpcloud-api     → ASP.NET Core 9 en puerto 5000
              ├── ftpcloud-ui      → Vue 3 servido por Nginx en puerto 5173
              └── cloudflared      → Cloudflare Tunnel (acceso externo por subdominio propio)
```

El storage de archivos vive en un volumen Docker montado en `/app/storage`. Cloudflare Tunnel hace el TLS y la exposición pública sin abrir puertos en el router. Los uploads usan **TUS** (chunks de 8 MB) para sortear el límite de 100 MB de la capa gratuita de Cloudflare y poder reanudar subidas si se interrumpen.

---

## Autenticación y seguridad

### JWT en cookies HttpOnly

El token nunca toca JavaScript. Viaja solo en cookies que el navegador adjunta automáticamente:

```
Access token   → 30 minutos  → cookie HttpOnly, Secure, SameSite=Lax
Refresh token  → 7 días      → cookie HttpOnly, Path=/api/auth
```

Cuando el access token expira, `api.ts` intercepta el `401`, llama a `/api/auth/refresh` transparentemente y reintenta la request original. Si el refresh también falla, limpia la sesión y redirige al login.

`SameSite=Lax` en lugar de `Strict` para que la sesión no se rompa cuando el usuario llega desde un link externo (WhatsApp, email). Protege igual contra CSRF en operaciones de escritura.

### Contraseñas

BCrypt para almacenar hashes. El backend nunca almacena ni devuelve contraseñas en texto plano.

---

## API REST (endpoints principales)

#### `/api/auth`
| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/login` | Credenciales → setea cookies → devuelve `User` |
| `POST` | `/logout` | Invalida refresh token + borra cookies |
| `POST` | `/refresh` | Rota el refresh token → nuevas cookies |
| `GET`  | `/me` | Usuario autenticado por la cookie |
| `POST` | `/change-password` | Cambia la propia contraseña |

#### `/api/users` — requiere rol admin u owner
| Método | Ruta | Roles | Descripción |
|--------|------|-------|-------------|
| `GET`    | `/`              | admin, owner | Lista todos los usuarios |
| `POST`   | `/`              | admin, owner | Crea usuario |
| `DELETE` | `/{id}`          | owner        | Elimina usuario |
| `PUT`    | `/{id}/quota`    | owner        | Cambia cuota de almacenamiento |
| `PUT`    | `/{id}/role`     | owner        | Cambia rol |
| `PUT`    | `/{id}/password` | owner        | Resetea contraseña |

#### `/api/files` — requiere autenticación
| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET`    | `/personal`, `/shared`, `/groups` | Contenido del explorador (con `?folderId=` para navegar) |
| `POST`   | `/files/tus` | Upload TUS (chunked resumable) |
| `GET`    | `/{id}/download` | Descarga el archivo |
| `GET`    | `/{id}/preview` | Preview inline (sin forzar descarga) |
| `PUT`    | `/{id}` | Renombrar archivo |
| `PUT`    | `/{id}/move` | Mover archivo a otra carpeta |
| `DELETE` | `/{id}` | Mover a papelera |
| `POST`   | `/folders` | Crear carpeta |
| `PUT`    | `/folders/{id}` | Renombrar carpeta |
| `PUT`    | `/folders/{id}/move` | Mover carpeta |
| `DELETE` | `/folders/{id}` | Mover a papelera |
| `GET`    | `/folders/{id}/tree` | Árbol de carpetas (para selector de destino) |
| `GET/POST/DELETE` | `/folders/{id}/members` | Gestión de miembros en grupos |
| `GET`    | `/trash` | Contenido de la papelera |
| `POST`   | `/trash/files/{id}/restore`, `/trash/folders/{id}/restore` | Restaurar |
| `DELETE` | `/trash/files/{id}`, `/trash/folders/{id}` | Eliminar permanentemente |
| `DELETE` | `/trash` | Vaciar papelera |
| `GET`    | `/search?q=` | Búsqueda global |
| `GET/POST/DELETE` | `/{id}/shares` | Compartir archivo con usuarios específicos |

---

## Cómo levantar

### Con Docker (despliegue completo)

```bash
cp .env.example .env
# Editar .env con tus valores

docker compose up --build -d
```

El backend aplica las migraciones y crea el owner automáticamente al arrancar. La UI queda en `http://localhost:5173` y la API en `http://localhost:5000`.

### Variables de entorno requeridas

```env
POSTGRES_USER=ftpcloud
POSTGRES_PASSWORD=tu-contraseña-segura
POSTGRES_DB=ftpcloud

JWT_SIGNING_KEY=clave-larga-aleatoria-minimo-32-chars
FRONTEND_ORIGIN=http://localhost:5173

SEED_OWNER_USERNAME=admin
SEED_OWNER_EMAIL=admin@example.com
SEED_OWNER_PASSWORD=tu-contraseña

VITE_API_URL=http://localhost:5000/api

CLOUDFLARE_TUNNEL_TOKEN=   # opcional, para acceso externo
```

### Solo el frontend (desarrollo)

```bash
cd ftpcloud-ui
pnpm install
pnpm dev
```

---

## Tecnologías

| Capa | Tecnología |
|------|------------|
| Frontend | Vue 3, TypeScript, Vite, Pinia, Vue Router, pnpm |
| Backend | ASP.NET Core 9 Web API, Clean Architecture |
| ORM | Entity Framework Core 9, Npgsql |
| Base de datos | PostgreSQL 16 |
| Autenticación | JWT (cookies HttpOnly), BCrypt |
| Uploads | TUS protocol — tusdotnet (backend), tus-js-client (frontend) |
| Infraestructura | Docker Compose, Proxmox VM |
| Acceso externo | Cloudflare Tunnel |
