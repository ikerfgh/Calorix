# Calorix

![Pruebas automatizadas](https://github.com/ikerfgh/Calorix/actions/workflows/pruebas.yml/badge.svg)

Aplicación web de seguimiento nutricional que conecta **pacientes** con **nutriólogos** validados.
Proyecto final de *Arquitectura e Ingeniería de Software* — Universidad La Salle México, Equipo 5.

| Integrante | GitHub |
|---|---|
| Iker Escandón Perea | [@ikerfgh](https://github.com/ikerfgh) |
| André Alejandro Lerios Imai | [@AndreLerios](https://github.com/AndreLerios) |
| Héctor Tadeo Olivo Castro | [@HectorTade](https://github.com/HectorTade) |
| Carlos Germán Rivera Arriaga | [@german457](https://github.com/german457) |

## Funcionalidad

- **Paciente:** registro de comidas y recetas, dashboard diario/semanal, historial de peso, macros recomendados (3 fórmulas), planes alimenticios y **reportes PDF** (progreso y planes).
- **Nutriólogo:** solicitudes de vinculación, seguimiento de pacientes por rango de fechas, planes alimenticios, recetas clínicas y observaciones.
- **Administrador:** validación de la cédula y documentos de los nutriólogos.
- **APIs:** **REST** (`/api`, documentada con Swagger) y **GraphQL** (`/graphql`). Ver [docs/API.md](docs/API.md).

## Arquitectura

ASP.NET Core 9 **MVC** + Entity Framework Core (SQLite).

| Capa | Ubicación |
|---|---|
| Vistas (Razor) | `Views/` |
| Controladores MVC | `Controllers/` |
| API REST | `Controllers/Api/` |
| API GraphQL (HotChocolate) | `GraphQL/` |
| Lógica de negocio | `Patterns/Facade/CalorixFacade.cs` |
| Modelo y datos | `Models/`, `Data/` |
| Servicios (PDF, contraseñas) | `Services/` |

Patrones de diseño (GoF): **Facade** (`CalorixFacade` como punto único de la lógica), **Strategy** (fórmulas Mifflin-St Jeor, Harris-Benedict y Katch-McArdle) y **Singleton** (`DatabaseConnection`).

## Cómo correrlo

Requisitos: [.NET 9 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --urls http://localhost:5080
```

| URL | Qué es |
|---|---|
| http://localhost:5080 | Aplicación |
| http://localhost:5080/swagger | Documentación y prueba de la API REST |
| http://localhost:5080/graphql | Explorador GraphQL (Nitro) |

Cuentas demo (contraseña `demo123`): `maria@demo.com` (paciente), `carlos@demo.com` (nutriólogo). Administrador: `admin@calorix.com` / `admin123`.

## Pruebas

64 pruebas automatizadas, ejecutadas en cada push por **GitHub Actions** ([workflow](.github/workflows/pruebas.yml)):

| Suite | Pruebas |
|---|---|
| `tests/Calorix.Tests` — xUnit (fachada, seguridad, reportes) | 30 |
| `tests/Calorix.UITests` — robot Playwright (UI, REST, GraphQL) | 34 |

Detalle y trazabilidad con los casos de uso en [tests/README.md](tests/README.md).
