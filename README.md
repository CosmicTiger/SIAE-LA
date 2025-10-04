# SIAE-LA

![.NET Badge](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff&style=for-the-badge)
![MicrosoftSQLServer](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-black?style=for-the-badge&logo=JSON%20web%20tokens)
![Scalar Badge](https://img.shields.io/badge/Scalar-1A1A1A?logo=scalar&logoColor=fff&style=for-the-badge)
![OpenAPI Initiative Badge](https://img.shields.io/badge/OpenAPI%20Initiative-6BA539?logo=openapiinitiative&logoColor=fff&style=for-the-badge)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)


> 📌 **Stack:** ASP.NET Core 8 + EF Core 8 + SQL Server 2022 (Docker) + JWT + OpenAPI (Swagger/Scalar)  
> 🧭 **Estructura actual:** un **único proyecto** `SIAE-LA\\SIAE-LA.csproj`  
> 💻 **Ubicación de trabajo (ejemplos):** `C:\\Users\\<you>\\source\\repos\\SIAE-LA`

---

## 🧰 Preparación de BD, Migraciones y Entorno

### ✅ Requisitos
- .NET 8 SDK
- EF Core CLI (`dotnet-ef`)
  ```powershell
  dotnet tool install --global dotnet-ef
  dotnet ef --version
  dotnet tool update --global dotnet-ef
  ```
- Docker / Docker Desktop (para SQL Server en contenedor)
- Visual Studio 2022 o VS Code (opcional)

---

## 🐳 Base de Datos con Docker (SQL Server 2022)

Archivo `docker-compose.yml` (en la **raíz** del repo):

```yaml
services:
  mssql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: siae-mssql
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "YourStrong!Passw0rd"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      - mssql_data:/var/opt/mssql
    healthcheck:
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost,1433", "-U", "sa", "-P", "YourStrong!Passw0rd", "-C", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 10

volumes:
  mssql_data:
```

Levantar el contenedor:
```powershell
docker-compose up -d
docker ps
docker logs -f siae-mssql
```

> ⏳ Si es la **primera vez**, espera a que el *healthcheck* pase a **healthy**.  
> 🔗 **Cadena de conexión recomendada (DEV):**
```ini
Server=localhost,1433;Database=SIAE_LA_Db;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;
```

---

## ⚙️ Configuración de `appsettings` (DEV / PROD)

### `appsettings.json` (base / PROD)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SIAE_LA_Db;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "SIAE-LA",
    "Audience": "SIAE-LA-Clients",
    "Key": "dev-only-change-this-key"
  },
  "RootAdmin": {
    "Email": "root@siae.local",
    "Password": "Change_this_123!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### `appsettings.Development.json` (override DEV)
> Puedes copiar el mismo contenido y **sobrescribir** lo necesario para DEV.  
> 🔐 Recomendado: usar **User Secrets** para no commitear credenciales.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SIAE_LA_Db;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "SIAE-LA-DEV",
    "Audience": "SIAE-LA-Clients-DEV",
    "Key": "dev-only-change-this-key"
  },
  "RootAdmin": {
    "Email": "root@siae.local",
    "Password": "Change_this_123!"
  }
}
```

#### 🔐 User Secrets (DEV)
```powershell
cd .\SIAE-LA
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=SIAE_LA_Db;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "dev-super-secret-change-me"
dotnet user-secrets set "RootAdmin:Email" "root@siae.local"
dotnet user-secrets set "RootAdmin:Password" "Change_this_123!"
```

#### 🌐 Variables de entorno (PROD)
En producción, **NO** uses User Secrets. Configura estas variables:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `ROOT_ADMIN_EMAIL`
- `ROOT_ADMIN_PASSWORD` *(opcional; si no, se toma de `appsettings`)*

> 🔎 El seeder leerá **primero** `ROOT_ADMIN_EMAIL` y `ROOT_ADMIN_PASSWORD` si existen.

---

## 🗄️ Migraciones EF Core

> Proyecto y startup son el mismo: `SIAE-LA\SIAE-LA.csproj`  
> Si ejecutas desde la **raíz**, especifica -p y -s al mismo `.csproj`.

### 🧩 A) Aplicar **nuevas migraciones** *conservando datos*
1. Crear migración
   ```powershell
   dotnet ef migrations add <NombreMigracion> -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj
   ```
2. Aplicar a la BD
   ```powershell
   dotnet ef database update -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj
   ```
3. Si la última migración fue un error de diseño:
   ```powershell
   dotnet ef migrations remove -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj
   ```

### 🧹 B) **Reset limpio** de DEV *(borra datos)*
> ⚠️ **Solo** para desarrollo. Perderás los datos.

1. Compila primero:
   ```powershell
   dotnet build .\SIAE-LA\SIAE-LA.csproj
   ```
2. Elimina la carpeta de migraciones (si existe):
   ```powershell
   Remove-Item -Recurse -Force .\SIAE-LA\Migrations
   # o: rm -r .\SIAE-LA\Migrations
   ```
3. Dropear BD:
   ```powershell
   dotnet ef database drop -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj -f
   ```
4. Crear migración inicial:
   ```powershell
   dotnet ef migrations add InitialCreate -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj
   ```
5. Aplicar migraciones:
   ```powershell
   dotnet ef database update -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj
   ```

---

## 🌱 Seeding de datos

El seeding se ejecuta al **iniciar** la app si en `Program.cs` está:
```csharp
// Seed en startup
await app.UseDataSeeder();
```

### ▶️ Ejecutar la app (DEV)
```powershell
cd .\SIAE-LA
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```
> o bien en lugar de `dotnet run`, usa:
```powershell
dotnet run --project .\SIAE-LA\SIAE-LA.csproj
```

Lo que crea el seeder:
- ✅ **Roles**: `Admin, Direccion, Subdireccion, JefeArea, Docente, Estudiante, Tutor`
- 👑 **Root Admin** (aprobado)
- 📚 **Catálogos del ER**
- 👩‍🏫👨‍🎓 **Personas/Usuarios** demo (docentes, dirección, alumno + apoderado)
- 🧩 **Reglas aplicadas**:
  - `Persona.DocumentoIdentidad` obligatorio y válido (cédula NI).
  - Alumno menor → `DocumentoIdentidad = "TUTOR-<cedTutor>"`.
  - `Sexo` y `FechaNacimiento` obligatorios.
  - Teléfonos normalizados a formato **E.164 NI** (`+505########`).

Cambiar root admin en DEV:
```powershell
dotnet user-secrets set "RootAdmin:Email" "nuevo_root@siae.local"
dotnet user-secrets set "RootAdmin:Password" "NuevoPass_123!"
```
o por variables de entorno:
```powershell
$env:ROOT_ADMIN_EMAIL="nuevo_root@siae.local"
$env:ROOT_ADMIN_PASSWORD="NuevoPass_123!"
```

---

## 📚 API Docs

- **Scalar (UI)** → `https://localhost:<httpsPort>/docs`  
- **OpenAPI JSON** → `https://localhost:<httpsPort>/openapi/v1.json`

> ℹ️ Puerto HTTPS se define en `launchSettings.json` (perfil `https`).

---

## 🚀 Checklist de Producción

- [ ] Variables de entorno: `ConnectionStrings__DefaultConnection`, `Jwt__*`, `ROOT_ADMIN_*`  
- [ ] `Jwt:Key` **fuerte y secreta** (rotación periódica)  
- [ ] SQL Server gestionado o contenedor con volúmenes persistentes y backups  
- [ ] `ASPNETCORE_ENVIRONMENT=Production`  
- [ ] HTTPS obligatorio y reverse proxy (NGINX/IIS) configurado  
- [ ] Monitoreo y logs (Application Insights / Serilog, etc.)  
- [ ] Política de CORS restringida a dominios de tu SPA (Angular/React)

---

## 🧪 Troubleshooting

- ❌ `Build failed` al usar `dotnet ef` → primero corre `dotnet build` y corrige errores.  
- 🐳 SQL no responde → verifica `docker ps`, puertos `1433`, y la cadena `Encrypt=False;TrustServerCertificate=True;` en DEV.  
- 🧩 Seeding no corre → confirma `await app.UseDataSeeder();` en `Program.cs` y revisa credenciales/variables del Root Admin.

---

## ⌨️ Cheatsheet EF

```powershell
# Nueva migración
dotnet ef migrations add NuevaFeature -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj

# Aplicar migraciones
dotnet ef database update -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj

# Quitar última migración (si no aplicada a BD)
dotnet ef migrations remove -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj

# Reset limpio (DEV)
dotnet ef database drop -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj -f
Remove-Item -Recurse -Force .\SIAE-LA\Migrations
dotnet ef migrations add InitialCreate -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj
dotnet ef database update -p .\SIAE-LA\SIAE-LA.csproj -s .\SIAE-LA\SIAE-LA.csproj
```

---

Hecho con ❤️ para **SIAE-LA**.
