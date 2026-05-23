# unir-tfe-app-finanzas

Aplicación de gestión de finanzas personales compuesta por un **cliente móvil multiplataforma** y una **API REST** en la nube, con servicios de **Inteligencia Artificial**.

## Descripción general

**AppFinanzas** permite a los usuarios registrar y consultar sus movimientos financieros (ingresos y gastos), organizarlos por categorías y asociarlos a cuentas bancarias. La identidad de los usuarios se gestiona mediante **Auth0**, y la información se persiste en una base de datos **SQL Server** alojada en Azure.

## Estructura del repositorio

```
unir-tfe-app-finanzas/
├── app/        # Solución de la aplicación multiplataforma (.NET MAUI)
├── backend/    # Solución de la API REST (ASP.NET Core)
└── docs/       # Documentación adicional del proyecto
```

## Soluciones

| Carpeta | Descripción | README |
|---|---|---|
| [`app/`](app/) | Aplicación cliente multiplataforma onstruida usando .NET MAUI 10 y el patrón MVVM. | [Ver README](app/README.md) |
| [`backend/`](backend/) | API REST con ASP.NET Core 10, Arquitectura Limpia y CQRS. Persistencia en SQL Server con Entity Framework Core. | [Ver README](backend/README.md) |

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Cliente móvil | .NET MAUI 10 |
| API REST | ASP.NET Core 10 |
| Base de datos | SQL Server (Azure SQL) |
| ORM | Entity Framework Core 10 |
| Autenticación | Auth0 (OAuth2 / JWT) |
| Patrón | Clean Architecture + Mediator |
| Logging / Monitorización | Serilog + Azure Application Insights |
| Almacenamiento de archivos | Azure Blob Storage |

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) con las características **ASP.NET** y **.NET MAUI** instaladas
- Cuenta de Auth0 configurada
- SQL Server (local o Azure SQL)

## Inicio rápido

El archivo README de cada solución ofrece las instrucciones detalladas de configuración y ejecución:

- [Configurar y ejecutar el backend](backend/README.md)
- [Configurar y ejecutar la aplicación móvil](app/README.md)
