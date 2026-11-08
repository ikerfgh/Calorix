# Pruebas automatizadas — Calorix

| Proyecto | Tipo | Qué cubre |
|---|---|---|
| `Calorix.Tests` | xUnit + SQLite en memoria | Lógica de negocio vía `CalorixFacade` (CU1–CU8) y reportes PDF. Casos TC-01 … TC-20 |
| `Calorix.UITests` | Robot Playwright (Chromium) | Recorre la interfaz: login, menú, descargas PDF, logout. Casos UI-01 … UI-08 |

## Cómo correrlas

```bash
# Pruebas unitarias / integración (no necesitan la app corriendo)
dotnet test tests/Calorix.Tests

# Robot de interfaz
dotnet run --urls http://localhost:5080          # terminal 1, desde la raíz
pwsh tests/Calorix.UITests/bin/Debug/net9.0/playwright.ps1 install chromium   # solo la primera vez
dotnet test tests/Calorix.UITests                # terminal 2
# Para ver el navegador moviéndose:  $env:HEADED=1; dotnet test tests/Calorix.UITests
```

## Trazabilidad caso de prueba ↔ caso de uso

| Caso de uso | Pruebas |
|---|---|
| CU1 Gestión de alimentos | TC-07 |
| CU2 Registro de comidas | TC-08, TC-09, TC-10 |
| CU3 Seguimiento nutricional | TC-11, TC-12, TC-13, TC-19, UI-05 |
| CU4/CU5 Pacientes y seguimiento profesional | TC-14, TC-15, UI-07 |
| CU6 Planes alimenticios | TC-16, TC-17, TC-20, UI-06 |
| CU7 Perfil / autenticación | TC-01 … TC-06, UI-01, UI-02, UI-03, UI-08 |
| CU8 Registro profesional | TC-18 |
| Navegación general | UI-04 |
