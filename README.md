# MiPresupuesto API 💰📊

API REST desarrollada en **.NET 9 (C#)** diseñada como backend de sincronización en la nube para la aplicación de finanzas personales **MiPresupuesto**. Está orientada a soportar aplicaciones móviles o web bajo una arquitectura **offline-first**.

---

## 🚀 Tecnologías y Herramientas

* **Framework**: .NET 9.0 (ASP.NET Core Web API)
* **ORM**: Entity Framework Core 9 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
* **Base de Datos**: PostgreSQL
* **Autenticación y Seguridad**: 
  * JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
  * Hashing de contraseñas con **BCrypt** (`BCrypt.Net-Next`)
* **Documentación**: Swagger / OpenAPI con soporte interactivo para tokens Bearer (`Swashbuckle.AspNetCore 10`)

---

## 🛠️ Mejoras y Optimizaciones Realizadas

1. **Soporte Completo de Sincronización en `Push`**:
   * Se incorporó el procesamiento y almacenamiento por lotes (*Upsert*) de **`PagosRecurrentes`**, el cual estaba declarado en los DTOs pero no se procesaba en el controlador.
2. **Eliminación del Problema de Rendimiento N+1**:
   * Se refactorizó el método `POST /api/sync/push` para precargar en memoria mediante diccionarios en una sola consulta las transacciones, presupuestos y pagos recurrentes existentes, reduciendo drásticamente la latencia y la carga en la base de datos al sincronizar lotes grandes.
3. **Manejo Seguro de Autenticación en Controladores**:
   * Se ajustó la extracción del `UsuarioId` desde el token JWT para responder con un código HTTP `401 Unauthorized` limpio y estructurado en lugar de fallar con un error 500 no controlado.
4. **Unificación de Seguridad JWT**:
   * Se sincronizaron las claves y parámetros por defecto de firma y validación entre `Program.cs` y `JwtService.cs`.
5. **Configuración Interactiva de Swagger**:
   * Se configuró el esquema de seguridad Bearer en Swagger UI, permitiendo autenticarse con el token JWT directamente desde la interfaz web (botón *Authorize*).
6. **Limpieza del Proyecto**:
   * Se removieron los archivos de plantilla generados por defecto (`WeatherForecast.cs` y `WeatherForecastController.cs`).
   * Se añadió un archivo `.gitignore` optimizado para proyectos .NET / Visual Studio / VS Code.

---

## 📁 Estructura del Proyecto

```text
MiPresupuesto-api/
├── Controllers/
│   ├── AuthController.cs          # Registro y Login de usuarios
│   └── SyncController.cs          # Sincronización offline-first (Pull y Push)
├── Models/
│   ├── Usuario.cs                 # Datos de usuario y credenciales
│   ├── Transaccion.cs             # Movimientos financieros (soporta soft-delete)
│   ├── Categoria.cs               # Categorías del sistema y personalizadas
│   ├── PresupuestoMensual.cs      # Metas mensuales y límites de gasto
│   └── PagoRecurrente.cs          # Pagos fijos o recurrentes mensuales
├── DTOs/
│   ├── AuthDtos.cs                # Modelos de transferencia para autenticación
│   └── SyncDtos.cs                # Modelos de transferencia para sincronización
├── Services/
│   └── JwtService.cs              # Generación de tokens JWT
├── Data/
│   └── ApplicationDbContext.cs    # Configuración de EF Core, índices y seed de categorías
├── Migrations/                    # Migraciones de base de datos de EF Core
├── mipresupuesto_init.sql         # Script SQL inicial para PostgreSQL
├── appsettings.json               # Configuración de conexión y JWT
└── Program.cs                     # Inicialización de la aplicación y servicios
```

---

## 📡 Endpoints de la API

### Autenticación (`/api/auth`)
* `POST /api/auth/register`: Registra un nuevo usuario con email y contraseña, devolviendo el token de acceso.
* `POST /api/auth/login`: Autentica credenciales y devuelve el token JWT con sus datos de perfil.

### Sincronización (`/api/sync`) — *Requiere Header `Authorization: Bearer <token>`*
* `GET /api/sync/pull`: Descarga todo el histórico del usuario (transacciones, categorías del sistema/usuario, presupuestos y pagos recurrentes).
* `POST /api/sync/push`: Recibe un lote de datos nuevos o actualizados en local y aplica un *Upsert* con resolución de conflictos basada en marcas de tiempo (`UpdatedAt`).

---

## ⚙️ Cómo Ejecutar el Proyecto

### 1. Requisitos Previos
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) instalado.
* Instancia de [PostgreSQL](https://www.postgresql.org/) en ejecución (local o en Docker).

### 2. Configurar la Cadena de Conexión
Edita el archivo `appsettings.json` o `appsettings.Development.json` con las credenciales de tu base de datos:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=mipresupuesto_db;Username=postgres;Password=tu_password"
}
```

### 3. Aplicar las Migraciones a la Base de Datos
Puedes inicializar la base de datos ejecutando el comando de Entity Framework:

```bash
dotnet ef database update
```

*(Alternativamente, puedes ejecutar directamente el archivo script `mipresupuesto_init.sql` en tu cliente de PostgreSQL como pgAdmin o DBeaver).*

### 4. Compilar y Levantar la API

Para compilar la solución:
```bash
dotnet build
```

Para iniciar el servidor de desarrollo:
```bash
dotnet run
```

### 5. Probar con Swagger UI
Abre tu navegador web en:
```text
https://localhost:7087/swagger
```
*(o en el puerto HTTP/HTTPS que la consola indique, por ejemplo `http://localhost:5242/swagger`)*.

1. Usa `/api/auth/register` o `/api/auth/login` para obtener un token JWT.
2. Haz clic en el botón superior **Authorize** e introduce el token.
3. Prueba los endpoints de `/api/sync/pull` y `/api/sync/push`.
