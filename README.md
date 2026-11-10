# MiPresupuesto API 💰📊

API REST desarrollada en **.NET 9 (C#)** diseñada como backend de sincronización en la nube para la aplicación de finanzas personales **MiPresupuesto**. Está orientada a soportar aplicaciones móviles o web bajo una arquitectura **offline-first** con resolución de conflictos por marcas temporales (*Last-Write-Wins*).

---

## 🚀 Tecnologías y Herramientas

* **Framework**: .NET 9.0 (ASP.NET Core Web API)
* **ORM**: Entity Framework Core 9 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
* **Base de Datos**: PostgreSQL 16
* **Autenticación y Seguridad**: 
  * JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
  * Hashing de contraseñas con **BCrypt** (`BCrypt.Net-Next`)
  * Validaciones declarativas con **Data Annotations**
* **Resiliencia**: Middleware global de excepciones y endpoints de **Health Checks**
* **Contenerización**: Docker & Docker Compose
* **Documentación**: Swagger / OpenAPI con soporte interactivo para tokens Bearer (`Swashbuckle.AspNetCore 10`)

---

## 🛠️ Mejoras y Optimizaciones Implementadas

1. **Sincronización Delta / Incremental (`since`)**:
   * `GET /api/sync/pull` ahora acepta el parámetro opcional `since` (`DateTime`). Si se especifica, solo descarga los registros creados o modificados después de esa fecha, ahorrando batería y datos móviles.
2. **Soporte Completo de Categorías Personalizadas en `Push`**:
   * Se incorporó el soporte para sincronizar las categorías creadas o editadas por el usuario, protegiendo las categorías maestras del sistema.
3. **Resolución Estricta de Conflictos en Presupuestos**:
   * Se ajustó el procesamiento de `PresupuestosMensuales` para validar marcas temporales (`UpdatedAt`) y respetar la fecha enviada por el cliente offline.
4. **Eliminación del Problema de Rendimiento N+1**:
   * Consultas por lotes en memoria (*Dictionary Lookup*) en `POST /api/sync/push` para transacciones, presupuestos, pagos recurrentes y categorías.
5. **Cumplimiento con Tiendas de Apps (Google Play Store & Apple App Store)**:
   * Endpoint `DELETE /api/auth/me` para eliminación permanente y en cascada de la cuenta y todos los datos personales del usuario.
   * Endpoint `GET /api/auth/me` para consulta de perfil y `POST /api/auth/change-password` para cambio de contraseña.
6. **Middleware Global de Excepciones**:
   * Captura automática de fallos no controlados con respuesta JSON limpia y formateada (`exito: false, mensaje, status, timestamp`), ocultando detalles sensibles en producción.
7. **Monitoreo y Despliegue con Docker**:
   * Endpoint de salud en `/health`.
   * `Dockerfile` multi-stage y `docker-compose.yml` para levantar la API y PostgreSQL con un único comando.

---

## 📁 Estructura del Proyecto

```text
MiPresupuesto-api/
├── Controllers/
│   ├── AuthController.cs          # Registro, Login, Perfil, Cambio de clave y Borrado de cuenta
│   └── SyncController.cs          # Sincronización offline-first (Pull incremental y Push por lotes)
├── Models/
│   ├── Usuario.cs                 # Datos de usuario y credenciales
│   ├── Transaccion.cs             # Movimientos financieros (soporta soft-delete)
│   ├── Categoria.cs               # Categorías del sistema y personalizadas
│   ├── PresupuestoMensual.cs      # Metas mensuales y límites de gasto
│   └── PagoRecurrente.cs          # Pagos fijos o recurrentes mensuales
├── DTOs/
│   ├── AuthDtos.cs                # Modelos de transferencia con validaciones DataAnnotations
│   └── SyncDtos.cs                # Modelos de transferencia para sincronización (con categorías)
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs # Captura global de errores no controlados
├── Services/
│   └── JwtService.cs              # Generación de tokens JWT
├── Data/
│   └── ApplicationDbContext.cs    # Configuración de EF Core, índices, precisión decimal y seed
├── Migrations/                    # Migraciones de base de datos de EF Core
├── mipresupuesto_init.sql         # Script SQL inicial para PostgreSQL
├── Dockerfile                     # Construcción de imagen de contenedor .NET 9
├── docker-compose.yml             # Orquestación de API + PostgreSQL
├── appsettings.json               # Configuración de conexión y JWT
└── Program.cs                     # Pipeline HTTP, Auth, CORS, Health Checks y Swagger
```

---

## 📡 Endpoints de la API

### Autenticación y Cuenta (`/api/auth`)
* `POST /api/auth/register`: Registra un nuevo usuario con email y contraseña, devolviendo el token de acceso.
* `POST /api/auth/login`: Autentica credenciales y devuelve el token JWT con sus datos de perfil.
* `GET /api/auth/me` *(Requiere Bearer Token)*: Retorna los datos del perfil autenticado.
* `POST /api/auth/change-password` *(Requiere Bearer Token)*: Actualiza la contraseña previa verificación de la actual.
* `DELETE /api/auth/me` *(Requiere Bearer Token)*: Elimina definitivamente la cuenta y todos los registros asociados (transacciones, presupuestos, pagos y categorías).

### Sincronización (`/api/sync`) — *Requiere Header `Authorization: Bearer <token>`*
* `GET /api/sync/pull`: Descarga el histórico del usuario.
  * *Query opcional:* `GET /api/sync/pull?since=2026-11-01T00:00:00Z` (descarga únicamente cambios posteriores a esa fecha).
* `POST /api/sync/push`: Recibe un lote de datos nuevos o actualizados en local (transacciones, presupuestos, pagos y categorías) y aplica un *Upsert* con resolución de conflictos basada en `UpdatedAt`.

### Monitoreo
* `GET /health`: Comprueba que el servicio de la API esté activo y respondiendo.

---

## ⚙️ Cómo Ejecutar el Proyecto

### Opción A: Con Docker Compose (Recomendada)
Para levantar la base de datos PostgreSQL y la API en un entorno aislado con un solo comando:

```bash
docker compose up --build
```
La API quedará disponible en `http://localhost:5242` y la documentación interactiva en `http://localhost:5242/swagger`.

---

### Opción B: Ejecución Local con .NET SDK

#### 1. Requisitos Previos
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) instalado.
* Instancia de [PostgreSQL](https://www.postgresql.org/) en ejecución.

#### 2. Configurar la Cadena de Conexión
Edita el archivo `appsettings.json` con las credenciales de tu base de datos:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=mipresupuesto_db;Username=postgres;Password=tu_password"
}
```

#### 3. Aplicar las Migraciones a la Base de Datos
```bash
dotnet ef database update
```
*(O ejecuta el script `mipresupuesto_init.sql` directamente en pgAdmin o DBeaver).*

#### 4. Iniciar la API
```bash
dotnet run
```

Accede a Swagger en:
```text
http://localhost:5242/swagger
```
