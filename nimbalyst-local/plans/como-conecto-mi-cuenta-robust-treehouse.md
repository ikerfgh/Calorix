---
title: Crear CLAUDE.md para Calorix
date: 2026-05-04T00:00:00.000Z
status: pending
---
## Context

El repositorio Calorix ya está conectado a GitHub (`https://github.com/claudecalorix-coder/Calorix.git`). El usuario ejecutó `/init` para que se cree un archivo `CLAUDE.md` en la raíz del proyecto. Este archivo guía a futuras instancias de Claude Code sobre cómo trabajar con este repositorio.

---

## Plan

Crear `C:\Users\ikere\Documents\Calorix\Calorix\CLAUDE.md` con el siguiente contenido:

```markdown
# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the app (development)
dotnet run --project Calorix.csproj

# Build only
dotnet build

# Restore NuGet packages
dotnet restore
```

No test project exists yet. The app runs on http://localhost:5000 by default.

## Architecture

ASP.NET Core MVC (.NET 9), no database — all data lives in memory via a Singleton.

### Design Patterns

| Pattern | File | Purpose |
| --- | --- | --- |
| Singleton | `Patterns/Singleton/DatabaseConnection.cs` | Single shared in-memory store; seed data for demo accounts |
| Strategy | `Patterns/Strategy/CalculoNutricionalStrategy.cs` | Swappable calorie-calculation formulas (Mifflin-St Jeor, Harris-Benedict, Deportiva) |
| Facade | `Patterns/Facade/CalorixFacade.cs` | Single entry point for all business operations; controllers only touch `ICalorixFacade` |

### Data flow

`DatabaseConnection` (Singleton) → `CalorixFacade` (Scoped, registered in `Program.cs`) → Controllers → Razor Views

### Authentication

Session-based, no ASP.NET Identity. On login, `UserId`, `UserRole` (`Usuario` | `Nutriologo`), and `UserName` are written to `HttpContext.Session`. `_Layout.cshtml` reads these to render role-conditional sidebar nav.

### Route convention

`{controller=Home}/{action=Index}/{id?}` — default action is `Index`, not `Login`. `HomeController.Index()` checks session and redirects to either `/Seguimiento` (Usuario) or `/Pacientes` (Nutriólogo).

### Controllers

- `HomeController` — login/logout only
- `SeguimientoController` — daily dashboard (calorie totals, macro rings, bar chart)
- `RegistroController` — food/meal logging via AJAX
- `AlimentosController` — food & recipe browser; recipe creation
- `PerfilController` — user profile view/edit + nutritional target calculation
- `PacientesController` — nutritionist: patient list + daily summaries
- `PlanesController` — nutritionist: meal plan builder per patient
- `NutriologoController` — nutritionist profile/registration

### Views & Layout

`Views/Shared/_Layout.cshtml` wraps all authenticated views in an app-shell with sidebar. The login page (`Views/Home/Login.cshtml`) uses `Layout = null` and renders standalone.


- `wwwroot/css/site.css` — full design-token system (CSS custom properties: primitives → semantic → component layers); Inter font; dark sidebar (`#0F172A`)
- `wwwroot/js/site.js` — vanilla JS: unified food/recipe search, SVG macro rings, bar charts, AJAX food registration, recipe builder modal, meal plan builder

### Razor gotcha

Use `selected="@(boolExpression)"` for `<option selected>` — Razor tag helpers render `selected="selected"` when the value is `true`. Do **not** use `selected="@(bool ? "selected" : "")"` (raises RZ1031).

### Seed demo accounts

| Email | Password | Role |
| --- | --- | --- |
| maria@demo.com | demo123 | Usuario |
| juan@demo.com | demo123 | Usuario |
| ana@demo.com | demo123 | Usuario |
| carlos@demo.com | demo123 | Nutriólogo |
| laura@demo.com | demo123 | Nutriólogo |
```
---

## Critical file

- `C:\Users\ikere\Documents\Calorix\Calorix\CLAUDE.md` — to be created

---

## Verification

After creation, run `dotnet build` to confirm nothing broke, then open the app and verify login still works.
```
### Frontend