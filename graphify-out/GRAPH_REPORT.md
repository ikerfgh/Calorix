# Graph Report - Calorix  (2026-09-23)

## Corpus Check
- 81 files · ~58,563 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 3 file(s) not represented in the graph (top: (none) 1, .db 1, .css 1)

## Summary
- 686 nodes · 1237 edges · 30 communities (25 shown, 5 thin omitted)
- Extraction: 87% EXTRACTED · 13% INFERRED · 0% AMBIGUOUS · INFERRED: 163 edges (avg confidence: 0.85)
- Token cost: 326,389 input · 0 output

## Community Hubs (Navigation)
- Registro de Comidas
- Casos de Uso CU1-CU8
- Perfil y Macros
- Alimentos y Recetas
- Archivos de Controladores
- Planes Alimenticios
- Admin y Cuentas
- Frontend site.js
- EF Core y Migraciones
- Modelo Nutriólogo
- Estadísticas y Rangos
- Seguimiento del Paciente
- DbContext y Seed
- Gestión de Pacientes
- Mis Planes y Observaciones
- Planes de Demo Escolar
- Login y Sesión
- Análisis PIECES y Riesgos
- Arquitectura MVC y Facade
- Entregas 2 y 3
- Launch Settings
- FODA, Kanban y Ventas
- Strategy Fórmulas Calóricas
- Proyecto csproj
- Diagramas DFD y BD
- Resumen Controladores
- README

## God Nodes (most connected - your core abstractions)
1. `ICalorixFacade` - 63 edges
2. `CalorixFacade` - 52 edges
3. `Usuario` - 31 edges
4. `CalorixDbContext` - 29 edges
5. `Nutriologo` - 25 edges
6. `Receta` - 25 edges
7. `RegistroComida` - 23 edges
8. `ResumenRango` - 22 edges
9. `PlanAlimenticio` - 21 edges
10. `Calorix.Models` - 18 edges

## Surprising Connections (you probably didn't know these)
- `Nutriologo.Validado server-side check in Vincular` --semantically_similar_to--> `Weak server-side validation of biometric data`  [INFERRED] [semantically similar]
  nimbalyst-local/plans/hay-un-problema-con-lively-scroll.md → graphify-out/converted/pieces_2e9538b5.md
- `Use case coverage audit CU1-CU8 + 16 UML diagrams plan` --references--> `Passwords stored without cryptographic hash`  [INFERRED]
  nimbalyst-local/plans/pasted-text-2026-05-07t16-44-41-txt-rev-zazzy-whistle.md → graphify-out/converted/pieces_2e9538b5.md
- `AdminController` --references--> `ICalorixFacade`  [EXTRACTED]
  Controllers/AdminController.cs → Patterns/Facade/CalorixFacade.cs
- `AlimentosController` --references--> `ICalorixFacade`  [EXTRACTED]
  Controllers/AlimentosController.cs → Patterns/Facade/CalorixFacade.cs
- `CuentaController` --references--> `ICalorixFacade`  [EXTRACTED]
  Controllers/CuentaController.cs → Patterns/Facade/CalorixFacade.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Strategy pattern: cálculo nutricional intercambiable** — docs_diagramas_diagrama_clases_general_patron_strategy [EXTRACTED 1.00]
- **Casos de uso CU1–CU8 que fluyen Controlador → CalorixFacade → Entidad** — docs_diagramas_cu1_gestion_alimentos_cu1_gestion_de_alimentos, docs_diagramas_cu2_registro_comidas_cu2_registro_de_comidas, docs_diagramas_cu3_seguimiento_nutricional_cu3_seguimiento_nutricional, docs_diagramas_cu4_gestion_pacientes_cu4_gestion_de_pacientes, docs_diagramas_cu5_seguimiento_profesional_cu5_seguimiento_profesional, docs_diagramas_cu6_planes_alimenticios_cu6_planes_alimenticios, docs_diagramas_cu7_perfil_personal_cu7_perfil_personal, docs_diagramas_cu8_registro_profesional_cu8_registro_profesional, docs_diagramas_diagrama_clases_general_patron_facade [INFERRED 0.95]
- **Brecha del subsistema de notificaciones** — docs_kanban_subsistema_notificaciones, docs_foda_sin_notificaciones_push_email, docs_diagramas_cu6_planes_alimenticios_observacion_automatica_notificacion, docs_kanban_hu5_planes_alimenticios, docs_kanban_hu7_validacion_profesional [INFERRED 0.85]
- **Design pattern explainers (Singleton, Facade, Strategy, MVC)** — nimbalyst_local_plans_explicame_como_funciona_el_happy_dongarra_singleton_explainer, nimbalyst_local_plans_explicame_como_funciona_el_zazzy_tiger_facade_explainer, nimbalyst_local_plans_explicame_como_funciona_el_groovy_rain_strategy_explainer, nimbalyst_local_plans_necesito_que_me_expliques_enchanted_pebble_mvc_pattern, nimbalyst_local_plans_continua_con_el_diagrama_proud_naur_gof_mapping_table [INFERRED 0.85]
- **School demo fallback strategy (laptop, Codespaces, video, USB)** — nimbalyst_local_plans_en_la_otra_compu_curious_parnas_own_laptop_demo, nimbalyst_local_plans_en_la_otra_compu_curious_parnas_github_codespaces, nimbalyst_local_plans_en_la_otra_compu_curious_parnas_video_failsafe, nimbalyst_local_plans_este_proyecto_esta_en_peaceful_pebble_self_contained_publish [EXTRACTED 1.00]
- **PIECES detected problems (weighted total 0.216)** — graphify_out_converted_pieces_2e9538b5_plaintext_passwords_issue, graphify_out_converted_pieces_2e9538b5_unencrypted_data_at_rest_issue, graphify_out_converted_pieces_2e9538b5_missing_async_issue, graphify_out_converted_pieces_2e9538b5_weak_server_validation_issue, graphify_out_converted_pieces_2e9538b5_missing_tests_issue [EXTRACTED 1.00]

## Communities (30 total, 5 thin omitted)

### Community 0 - "Registro de Comidas"
Cohesion: 0.06
Nodes (29): RegistroController, HttpPost, IActionResult, RegistroComida, AlimentoId, Calorias, Cantidad, Carbohidratos (+21 more)

### Community 1 - "Casos de Uso CU1-CU8"
Cohesion: 0.06
Nodes (58): CalcularTotalReceta (cálculo de macros de receta), CU1 — Gestión de Alimentos, CalcularMacrosConsumidos (alimento vs receta), CU2 — Registro de Comidas, CU3 — Seguimiento Nutricional, PorcentajeCumplimiento (consumo vs meta calórica), CU4 — Gestión de Pacientes, Flujo de Solicitud de Vinculación (pendiente → aprobada/rechazada) (+50 more)

### Community 2 - "Perfil y Macros"
Cohesion: 0.06
Nodes (25): PerfilController, HttpPost, IActionResult, MacrosRecomendados, CaloriasMeta, CarbosGramos, Formula, GrasasGramos (+17 more)

### Community 3 - "Alimentos y Recetas"
Cohesion: 0.06
Nodes (32): AlimentosController, Dictionary, HttpPost, IActionResult, JsonElement, List, Alimento, Calorias (+24 more)

### Community 4 - "Archivos de Controladores"
Cohesion: 0.06
Nodes (27): Calorix.Models, Calorix.Controllers, Calorix.Patterns.Strategy, Calorix.Patterns.Facade, microsoft_aspnetcore_mvc, system_globalization, System.Text.Json, DocumentoNutriologo (+19 more)

### Community 5 - "Planes Alimenticios"
Cohesion: 0.07
Nodes (27): PlanesController, Dictionary, HttpGet, HttpPost, IActionResult, JsonElement, List, List (+19 more)

### Community 6 - "Admin y Cuentas"
Cohesion: 0.07
Nodes (24): Controller, AdminController, HttpGet, HttpPost, IActionResult, IWebHostEnvironment, CuentaController, HttpPost (+16 more)

### Community 7 - "Frontend site.js"
Cohesion: 0.08
Nodes (32): addComidaPlan(), addIngrediente(), buscarParaRegistro(), eliminarRegistro(), esc(), fetchWeekAndRender(), modalSelectDay(), modalWeekNext() (+24 more)

### Community 8 - "EF Core y Migraciones"
Cohesion: 0.09
Nodes (23): Calorix.Migrations, Calorix.Data, Calorix.Patterns.Singleton, microsoft_entityframeworkcore, microsoft_entityframeworkcore_infrastructure, microsoft_entityframeworkcore_migrations, microsoft_entityframeworkcore_storage_valueconversion, Migration (+15 more)

### Community 9 - "Modelo Nutriólogo"
Cohesion: 0.08
Nodes (22): error, Nutriologo, AniosExp, Cedula, Email, EsAdmin, Especialidad, Id (+14 more)

### Community 10 - "Estadísticas y Rangos"
Cohesion: 0.09
Nodes (22): EstadisticasSemana, DatosPorDia, PromedioCaloriasDia, TotalSemana, ResumenRango, CaloriasPorDia, Desde, Etiqueta (+14 more)

### Community 11 - "Seguimiento del Paciente"
Cohesion: 0.13
Nodes (12): SeguimientoController, Dictionary, HttpGet, HttpPost, IActionResult, List, RegistroPeso, Fecha (+4 more)

### Community 12 - "DbContext y Seed"
Cohesion: 0.11
Nodes (17): CalorixDbContext, Alimentos, DocumentosNutriologo, Nutriologos, Observaciones, Planes, Recetas, RegistrosComida (+9 more)

### Community 13 - "Gestión de Pacientes"
Cohesion: 0.17
Nodes (10): PacientesController, DateTime, HttpPost, IActionResult, desde, etiqueta, fechaActual, hasta (+2 more)

### Community 14 - "Mis Planes y Observaciones"
Cohesion: 0.13
Nodes (11): MisPlanesController, HttpPost, IActionResult, Observacion, Fecha, Id, Leida, NutriologoId (+3 more)

### Community 15 - "Planes de Demo Escolar"
Cohesion: 0.15
Nodes (16): Plan: Create CLAUDE.md for Calorix, GitHub repo claudecalorix-coder/Calorix, In-memory Singleton data store (early architecture), Razor selected attribute gotcha (RZ1031), Seed demo accounts (maria, juan, ana, carlos, laura), Session-based authentication (UserId/UserRole/UserName), Plan: School demo without running binaries (A/B/C), GitHub Codespaces live backup (port 5080 public) (+8 more)

### Community 16 - "Login y Sesión"
Cohesion: 0.31
Nodes (5): HomeController, HttpPost, IActionResult, id, role

### Community 17 - "Análisis PIECES y Riesgos"
Cohesion: 0.18
Nodes (11): DB queries without async/await (throughput risk), No automated tests (regression risk), PIECES Analysis Workbook - Calorix, PIECES Initial Weighting (6 categories), ResultadoPIECES (total 0.216, score 7.84/10), Personal/biometric data unencrypted at rest (LFPDPPP/GDPR), Weak server-side validation of biometric data, Plan: Block Vincular Paciente for unvalidated nutritionist (+3 more)

### Community 18 - "Arquitectura MVC y Facade"
Cohesion: 0.20
Nodes (11): docs/diagramas/diagrama-clases-general.md, GoF role to Calorix class mapping table, Plan: Pattern-focused class diagram zoom views, Patterns/ folder as didactic choice, Calorix code anatomy guide, Request flow Browser to Controller to Facade to DbContext to SQLite, Facade pattern and DB connection explainer, RegistrarComida flow (macros scaled per 100g) (+3 more)

### Community 19 - "Entregas 2 y 3"
Cohesion: 0.22
Nodes (10): Plan: Entregas 2 y 3 (weight history, observations, range view), Nutritionist observations to patients, Patient day/week/month range view with offset navigation, renderBarChart JS helper (reused for weight), ResumenRango view-model, Weight history auto-recorded on profile update, Plan Alimenticio (patient-assigned daily guide), Receta Clinica (nutritionist reusable recipe) (+2 more)

### Community 20 - "Launch Settings"
Cohesion: 0.22
Nodes (8): applicationUrl, commandName, environmentVariables, launchBrowser, ASPNETCORE_ENVIRONMENT, profiles, Calorix, $schema

### Community 21 - "FODA, Kanban y Ventas"
Cohesion: 0.25
Nodes (8): Calorix brand palette (#3ddc84 on #0a0f0d), Competitors: MyFitnessPal, FatSecret, Cronometer, Calorix sales presentation (13-slide pptx), Plan: FODA, PIECES and Kanban deliverables, FODA (SWOT) analysis docs/foda.md, Kanban board with 7 macro user stories, User story to use case traceability table, 34 user stories (docs/historias-usuario.md)

### Community 22 - "Strategy Fórmulas Calóricas"
Cohesion: 0.29
Nodes (7): Passwords stored without cryptographic hash, Calorie formulas: Mifflin-St Jeor, Harris-Benedict, Katch-McArdle, Formula selected via ?formula= query in Seguimiento, Strategy pattern explainer (calorie formulas), CU8 Registro Profesional gap (~50%), Formula inconsistency: dashboard selection not persisted (always Mifflin), Use case coverage audit CU1-CU8 + 16 UML diagrams plan

### Community 23 - "Proyecto csproj"
Cohesion: 0.33
Nodes (5): net9.0, Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation (9.*), Microsoft.EntityFrameworkCore.Design (9.*), Microsoft.EntityFrameworkCore.Sqlite (9.*), Microsoft.NET.Sdk.Web

### Community 24 - "Diagramas DFD y BD"
Cohesion: 0.40
Nodes (5): UML collaboration diagrams per use case, DFD Level 0 - Sistema Calorix, Plan: DFD, logical model and collaboration diagrams, Logical DB model (12 tables), 16 Mermaid sequence + state diagrams (docs/diagramas)

## Ambiguous Edges - Review These
- `In-memory Singleton data store (early architecture)` → `Singleton pattern explainer (DatabaseConnection)`  [AMBIGUOUS]
  nimbalyst-local/plans/como-conecto-mi-cuenta-robust-treehouse.md · relation: conceptually_related_to

## Knowledge Gaps
- **205 isolated node(s):** `net9.0`, `Microsoft.EntityFrameworkCore.Design (9.*)`, `Microsoft.EntityFrameworkCore.Sqlite (9.*)`, `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation (9.*)`, `Microsoft.NET.Sdk.Web` (+200 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 267 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **5 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **What is the exact relationship between `In-memory Singleton data store (early architecture)` and `Singleton pattern explainer (DatabaseConnection)`?**
  _Edge tagged AMBIGUOUS (relation: conceptually_related_to) - confidence is low._
- **Why does `ICalorixFacade` connect `Registro de Comidas` to `Perfil y Macros`, `Alimentos y Recetas`, `Archivos de Controladores`, `Planes Alimenticios`, `Admin y Cuentas`, `Modelo Nutriólogo`, `Estadísticas y Rangos`, `Seguimiento del Paciente`, `Gestión de Pacientes`, `Mis Planes y Observaciones`, `Login y Sesión`?**
  _High betweenness centrality (0.177) - this node is a cross-community bridge._
- **Why does `CalorixDbContext` connect `DbContext y Seed` to `Registro de Comidas`, `Perfil y Macros`, `Alimentos y Recetas`, `Planes Alimenticios`, `Admin y Cuentas`, `EF Core y Migraciones`, `Modelo Nutriólogo`, `Seguimiento del Paciente`, `Mis Planes y Observaciones`?**
  _High betweenness centrality (0.078) - this node is a cross-community bridge._
- **Why does `CalorixFacade` connect `Registro de Comidas` to `Perfil y Macros`, `Alimentos y Recetas`, `Archivos de Controladores`, `Planes Alimenticios`, `Admin y Cuentas`, `Modelo Nutriólogo`, `Estadísticas y Rangos`, `Seguimiento del Paciente`, `DbContext y Seed`, `Gestión de Pacientes`, `Mis Planes y Observaciones`, `Login y Sesión`?**
  _High betweenness centrality (0.078) - this node is a cross-community bridge._
- **What connects `net9.0`, `Microsoft.EntityFrameworkCore.Design (9.*)`, `Microsoft.EntityFrameworkCore.Sqlite (9.*)` to the rest of the system?**
  _205 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Registro de Comidas` be split into smaller, more focused modules?**
  _Cohesion score 0.05706214689265537 - nodes in this community are weakly interconnected._
- **Should `Casos de Uso CU1-CU8` be split into smaller, more focused modules?**
  _Cohesion score 0.055051421657592255 - nodes in this community are weakly interconnected._