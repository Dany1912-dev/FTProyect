# FTPCloud — Contexto del proyecto

Gestor de archivos personal tipo Google Drive, self-hosted en un servidor Proxmox.
Acceso exclusivamente por web (HTTP), sin clientes FTP.
Pensado para uso personal, familia y amigos.

---

## Infraestructura

```
Servidor físico (Proxmox)
  └── VM Ubuntu Server + Docker
        ├── ftpcloud-ui   (Vue 3 — frontend)
        ├── ftpcloud-api  (ASP.NET Core — backend)
        ├── PostgreSQL
        └── /storage      (volumen Docker para los archivos)
```

- **Por qué VM y no LXC:** Docker dentro de LXC requiere contenedor privilegiado, lo que
  reduce el aislamiento. La VM es más limpia, los snapshots de Proxmox funcionan bien y
  el overhead es irrelevante para este caso de uso.

- **Acceso externo:** Cloudflare Tunnel + subdominio propio.
  ⚠️ Cloudflare Free limita requests a 100MB. Se resuelve con uploads chunked (TUS protocol)
  donde cada chunk es menor a 100MB.

---

## Stack tecnológico

| Capa       | Tecnología                        | Por qué                                      |
|------------|-----------------------------------|----------------------------------------------|
| Frontend   | Vue 3 + TypeScript + Vite + pnpm  | Curva suave desde React, Vite nativo para Vue |
| Backend    | ASP.NET Core Web API              | Ya conocido, excelente para streaming de archivos |
| Base datos | PostgreSQL                        | Relacional, robusto, ideal para permisos      |
| Auth       | JWT + BCrypt                      | Estándar, seguro                              |
| Cookies    | HttpOnly + SameSite=Lax + Secure  | El token nunca es accesible desde JS          |
| Uploads    | TUS protocol (chunked resumable)  | Archivos grandes, tolerante a desconexiones   |

---

## Modelo de usuarios y permisos

```
owner  (1 solo)
  ├── Ve dashboard con todos los usuarios
  ├── Puede crear admins y usuarios normales
  └── NO tiene acceso al explorador de archivos

admin
  ├── Puede crear usuarios normales
  └── Accede a: carpeta personal, compartidos, grupos

user (normal)
  └── Accede a: carpeta personal, compartidos, grupos
```

### Tipos de carpetas

| Tipo       | Acceso                                              |
|------------|-----------------------------------------------------|
| personal   | Solo el dueño                                       |
| shared     | Archivos que otros te compartieron directamente     |
| group      | Carpeta colaborativa, miembros invitados por nombre (como GitHub collaborators) |

---

## Seguridad — decisiones clave

### Contraseñas
- **BCrypt** para hashear. Nunca texto plano en la base de datos.
- Paquete NuGet: `BCrypt.Net-Next`

### Autenticación JWT con dos tokens

```
Access token  → 30 minutos
Refresh token → 7 días
```

Ambos viajan **únicamente en cookies HttpOnly**, nunca en el body de respuesta
ni en localStorage. El navegador los adjunta automáticamente en cada request.

### Configuración de cookies

```csharp
new CookieOptions
{
    HttpOnly = true,
    Secure   = true,        // solo HTTPS
    SameSite = SameSiteMode.Lax,
    Expires  = DateTimeOffset.UtcNow.AddMinutes(30) // o 7 días para refresh
}
// El refresh token además lleva Path = "/api/auth"
// para que solo se envie en esa ruta específica
```

### Por qué SameSite=Lax y no Strict
Con `Strict`, si el usuario llega desde un link externo (WhatsApp, email), la cookie
no se envía y parece que no tiene sesión. `Lax` protege igual contra CSRF y tiene
mejor UX.

---

## Estado actual del proyecto

### ✅ Frontend completado (`ftpcloud-ui/`)

Estructura de `src/`:
```
assets/         → base.css (variables), main.css (reset)
components/
  common/       → vacío, pendiente: botones, modales reutilizables
  files/        → vacío, pendiente: componentes del explorador
  users/        → vacío, pendiente: tarjetas/tablas de usuario
layouts/
  AuthLayout.vue       → centra el login en pantalla
  DashboardLayout.vue  → sidebar con nav según rol + logout
views/
  auth/LoginView.vue            → formulario de login
  owner/DashboardView.vue       → lista todos los usuarios (solo owner)
  admin/UsersView.vue           → tabla de usuarios con acciones
  files/MyFilesView.vue         → explorador de archivos personales
  files/SharedView.vue          → archivos compartidos
  files/GroupsView.vue          → grupos / carpetas colaborativas
stores/
  auth.ts    → usuario actual, roles (isOwner, isAdmin), restoreSession()
  files.ts   → estado de archivos y carpetas
  users.ts   → lista de usuarios para admin/owner
services/
  api.ts     → wrapper fetch con credentials:include, refresh automático en 401
types/
  index.ts   → interfaces: User, Folder, FileItem, GroupMember, ApiResponse
router/
  index.ts   → rutas con guards por rol (requiresAuth, requiresAdmin, requiresOwner)
```

### Flujo de autenticación (frontend)

1. App arranca → `restoreSession()` llama a `GET /api/auth/me`
   - Si hay cookie válida: restaura el usuario, monta la app
   - Si no: usuario queda null, router redirige a `/login`
2. Login → `POST /api/auth/login` → backend setea cookies, body devuelve solo `User`
3. Cada request → el browser adjunta cookies automáticamente (`credentials: 'include'`)
4. Si llega 401 → `api.ts` intenta `POST /api/auth/refresh` automáticamente
   - Éxito: reintenta la request original
   - Fallo: dispara evento `auth:logout` → limpia estado → redirige a `/login`
5. Logout → `POST /api/auth/logout` → backend borra las cookies → frontend limpia estado

### Variable de entorno frontend

```
# ftpcloud-ui/.env
VITE_API_URL=http://localhost:5000/api
```

En producción cambiar a la URL real del backend.

---

## ❌ Backend pendiente (`ftpcloud-api/`)

### Setup del proyecto

```bash
dotnet new webapi -n ftpcloud-api
cd ftpcloud-api
dotnet add package BCrypt.Net-Next
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Endpoints que el frontend espera

#### Auth
| Método | Ruta               | Descripción                                         |
|--------|--------------------|-----------------------------------------------------|
| POST   | /api/auth/login    | Recibe {username, password}. Setea cookies. Devuelve User |
| POST   | /api/auth/logout   | Borra las cookies. No requiere body                 |
| POST   | /api/auth/refresh  | Usa refresh token cookie. Rota y setea nueva access cookie |
| GET    | /api/auth/me       | Devuelve el usuario autenticado por la cookie       |

#### Usuarios
| Método | Ruta            | Roles          | Descripción                  |
|--------|-----------------|----------------|------------------------------|
| GET    | /api/users      | owner, admin   | Lista todos los usuarios     |
| POST   | /api/users      | owner, admin   | Crea un nuevo usuario        |
| DELETE | /api/users/{id} | owner          | Elimina usuario              |

#### Archivos
| Método | Ruta                   | Descripción                          |
|--------|------------------------|--------------------------------------|
| GET    | /api/files/personal    | Carpetas y archivos personales       |
| GET    | /api/files/shared      | Archivos compartidos conmigo         |
| GET    | /api/files/groups      | Grupos a los que pertenezco          |
| POST   | /api/files/upload      | Subir archivo (chunked/TUS)          |
| GET    | /api/files/{id}/download | Descargar archivo                  |
| DELETE | /api/files/{id}        | Eliminar archivo                     |

### Formato de respuesta estándar

```json
{
  "data": { ... },
  "message": "opcional"
}
```

Los errores devuelven:
```json
{
  "message": "Descripción del error"
}
```

### Schema de base de datos (borrador)

```sql
users
  id          UUID PRIMARY KEY
  username    VARCHAR(50) UNIQUE NOT NULL
  email       VARCHAR(100) UNIQUE NOT NULL
  password    VARCHAR(255) NOT NULL  -- hash BCrypt
  role        VARCHAR(10) NOT NULL   -- 'owner' | 'admin' | 'user'
  created_at  TIMESTAMP DEFAULT NOW()

refresh_tokens
  id          UUID PRIMARY KEY
  user_id     UUID REFERENCES users(id) ON DELETE CASCADE
  token       VARCHAR(500) NOT NULL
  expires_at  TIMESTAMP NOT NULL
  created_at  TIMESTAMP DEFAULT NOW()

folders
  id          UUID PRIMARY KEY
  name        VARCHAR(100) NOT NULL
  type        VARCHAR(10) NOT NULL  -- 'personal' | 'group'
  owner_id    UUID REFERENCES users(id)
  created_at  TIMESTAMP DEFAULT NOW()

folder_members
  folder_id   UUID REFERENCES folders(id) ON DELETE CASCADE
  user_id     UUID REFERENCES users(id) ON DELETE CASCADE
  role        VARCHAR(10) NOT NULL  -- 'editor' | 'viewer'
  PRIMARY KEY (folder_id, user_id)

files
  id          UUID PRIMARY KEY
  name        VARCHAR(255) NOT NULL
  size        BIGINT NOT NULL
  mime_type   VARCHAR(100)
  folder_id   UUID REFERENCES folders(id)
  uploaded_by UUID REFERENCES users(id)
  storage_path VARCHAR(500) NOT NULL  -- ruta en el filesystem
  created_at  TIMESTAMP DEFAULT NOW()
```

---

## Notas para cuando retomes en Linux

- El `.env` de Vue **no** debe subirse a GitHub (ya está en `.gitignore`)
- El `node_modules/` tampoco sube; al clonar hacer `pnpm install`
- El backend irá en una carpeta hermana: `FTProyect/ftpcloud-api/`
- Para el backend en Linux: instalar .NET SDK 8+ (`dotnet-sdk-8.0` en apt)
- PostgreSQL puede correr en Docker localmente para desarrollo:
  ```bash
  docker run -d \
    --name ftpcloud-db \
    -e POSTGRES_USER=ftpcloud \
    -e POSTGRES_PASSWORD=changeme \
    -e POSTGRES_DB=ftpcloud \
    -p 5432:5432 \
    postgres:16
  ```
- La estructura final del repo debería ser:
  ```
  FTProyect/
    CLAUDE.md
    ftpcloud-ui/    ← frontend Vue
    ftpcloud-api/   ← backend ASP.NET (pendiente)
  ```
