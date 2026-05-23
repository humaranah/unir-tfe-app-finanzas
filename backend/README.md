# AppFinanzas — API Backend

Solución _backend_, desarrollada con **ASP.NET Core 10** siguiendo los principios de **Arquitectura Limpia (Clean Architecture)** y el patrón **CQRS** mediante Mediator.

## Estructura de la solución

```
backend/
├── HA.TFG.AppFinanzas.BackEnd.slnx       # Archivo de solución
├── source/
│   ├── HA.TFG.AppFinanzas.BackEnd/               # Capa de presentación (Web API)
│   ├── HA.TFG.AppFinanzas.BackEnd.Application/   # Capa de aplicación (casos de uso)
│   ├── HA.TFG.AppFinanzas.BackEnd.Domain/        # Capa de dominio (entidades y contratos)
│   └── HA.TFG.AppFinanzas.BackEnd.Infrastructure/ # Capa de infraestructura (persistencia y servicios externos)
└── test/
    ├── HA.TFG.AppFinanzas.BackEnd.Tests/
    ├── HA.TFG.AppFinanzas.BackEnd.Application.Tests/
    └── HA.TFG.AppFinanzas.BackEnd.Infrastructure.Tests/
```

### Proyectos

| Proyecto | Capa | Descripción |
|---|---|---|
| `HA.TFG.AppFinanzas.BackEnd` | Presentación | Controladores REST, middleware, extensiones de startup y configuración de Auth0 y OpenAPI. |
| `HA.TFG.AppFinanzas.BackEnd.Application` | Aplicación | Comandos, consultas y handlers CQRS (Mediator). Contratos de repositorios e interfaces de servicios externos. |
| `HA.TFG.AppFinanzas.BackEnd.Domain` | Dominio | Entidades, value objects, enumeraciones y definición de repositorios. Sin dependencias externas. |
| `HA.TFG.AppFinanzas.BackEnd.Infrastructure` | Infraestructura | Implementación de repositorios con EF Core, migraciones de base de datos, Auth0 y almacenamiento de comprobantes. |

## Tecnologías y dependencias clave

| Tecnología | Uso |
|---|---|
| **ASP.NET Core 10** | Framework web |
| **Entity Framework Core 10** | ORM y migraciones (SQL Server) |
| **Mediator** (Source Generator) | Implementación CQRS |
| **Auth0 / JWT Bearer** | Autenticación y autorización |
| **Serilog** | Logging estructurado (consola + Application Insights) |
| **Application Insights** | Monitorización y telemetría en Azure |
| **Mapperly** | Mapeo de objetos mediante generación de código |
| **Scalar** | Interfaz de documentación OpenAPI |
| **Azure Blob Storage** | Almacenamiento de comprobantes |

## Recursos expuestos

| Recurso | Controlador | Descripción |
|---|---|---|
| `/api/cuentas` | `CuentasController` | Gestión de cuentas financieras |
| `/api/usuarios` | `UsuariosController` | Datos del usuario autenticado |

## Configuración

La aplicación se configura mediante `appsettings.json` y `appsettings.Development.json`. Los secretos se gestionan con **User Secrets** en desarrollo y con **Azure Key Vault** en producción.

Variables de configuración necesarias:

```json
{
  "ConnectionStrings": {
    "SqlServer": "<cadena de conexión SQL Server>"
  },
  "Auth0": {
    "Domain": "<dominio Auth0>",
    "Audience": "<audiencia Auth0>"
  },
  "ComprobanteStorage": {
    "Provider": "Azure | Local",
    "AzureConnectionString": "<cadena de conexión Azure Storage>",
    "LocalBasePath": "<ruta local para desarrollo>"
  },
  "ApplicationInsights": {
    "ConnectionString": "<cadena de conexión Application Insights>"
  }
}
```

## Compilar y ejecutar

```bash
# Restaurar dependencias y compilar
dotnet build source/HA.TFG.AppFinanzas.BackEnd

# Ejecutar en modo desarrollo
dotnet run --project source/HA.TFG.AppFinanzas.BackEnd

# Ejecutar todos los tests
dotnet test

# Aplicar migraciones de base de datos
dotnet ef database update --project source/HA.TFG.AppFinanzas.BackEnd.Infrastructure --startup-project source/HA.TFG.AppFinanzas.BackEnd
```

Una vez en ejecución, la documentación interactiva de la API está disponible en:
`https://localhost:<puerto>/scalar`

## Funcionalidades principales

- **Cuentas** — CRUD de cuentas financieras por usuario
- **Movimientos** — Registro y consulta de ingresos y gastos con soporte de comprobantes
- **Categorías** — Clasificación de movimientos
- **Usuarios** — Gestión de perfil asociado a la identidad Auth0
