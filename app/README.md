# AppFinanzas — Aplicación móvil (Frontend)

Solución de la aplicación cliente, desarrollada con **.NET MAUI 10** para ejecutarse en Android, iOS, macOS y Windows desde una única base de código.

## Estructura de la solución

```
app/
├── HA.TFG.AppFinanzas.App.slnx   # Archivo de solución
├── source/
│   ├── HA.TFG.AppFinanzas.App/   # Proyecto principal MAUI
│   └── HA.TFG.AppFinanzas.Core/  # Biblioteca de lógica compartida
└── test/
    └── HA.TFG.AppFinanzas.App.UnitTests/  # Tests unitarios
```

### Proyectos

| Proyecto | Descripción |
|---|---|
| `HA.TFG.AppFinanzas.App` | Proyecto MAUI ejecutable. Contiene vistas (XAML), navegación, configuración de la aplicación e integración con la capa de núcleo. |
| `HA.TFG.AppFinanzas.Core` | Biblioteca de clases compartida. Contiene ViewModels, modelos de vista, servicios HTTP, lógica de autenticación y utilidades. |
| `HA.TFG.AppFinanzas.App.UnitTests` | Tests unitarios sobre los ViewModels y la lógica de la capa `Core`. |

## Tecnologías y dependencias clave

- **.NET MAUI 10** — Framework multiplataforma (Android, iOS, macOS, Windows)
- **MVVM** — Patrón de presentación con `ViewModels` desacoplados de las vistas
- **Auth0** — Autenticación de usuarios mediante flujo OAuth2/OIDC
- **HttpClient** — Comunicación con la API REST del backend

## Plataformas soportadas

| Plataforma | Versión mínima |
|---|---|
| Android | API 21 (Android 5.0) |
| iOS | 15.0 |
| macOS (Mac Catalyst) | 15.0 |
| Windows | 10.0.17763.0 (1809) |

## Configuración

La configuración de la aplicación se gestiona mediante `appsettings.json`. Para entornos locales, los secretos (URL de la API, credenciales de Auth0, etc.) se definen en `appsettings.secrets.json`, que **no** está incluido en el control de versiones.

## Compilar y ejecutar

Abrir la solución `HA.TFG.AppFinanzas.App.slnx` en Visual Studio 2022 o posterior y seleccionar la plataforma destino en la barra de herramientas.

```bash
# Compilar desde línea de comandos (ejemplo Android)
dotnet build source/HA.TFG.AppFinanzas.App -f net10.0-android

# Ejecutar los tests unitarios
dotnet test test/HA.TFG.AppFinanzas.App.UnitTests
```

## Funcionalidades principales

- **Cuentas** — Consulta y gestión de cuentas financieras del usuario
- **Movimientos** — Registro y visualización de ingresos y gastos
- **Autenticación** — Inicio de sesión y cierre de sesión mediante Auth0
