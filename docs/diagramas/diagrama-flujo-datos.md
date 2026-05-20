# Diagrama de Flujo de Datos (DFD) — Calorix

| Campo | Valor |
| --- | --- |
| **Tipo** | Diagrama de Flujo de Datos — Nivel 0 (contexto) |
| **Notación** | DeMarco / Yourdon |
| **Sistema** | Calorix (aplicación de nutrición ASP.NET Core MVC) |
| **Renderizado** | Mermaid (flowchart) |

## Resumen

Este DFD muestra el sistema **Calorix** como un único proceso central rodeado por sus **entidades externas** (los actores que interactúan con él) y sus **almacenes de datos** (las tablas persistentes de SQLite). Cada flecha está etiquetada con el flujo de datos concreto que viaja entre el actor/almacén y el sistema.

- **Entidades externas (rectángulos):** Usuario / Paciente, Nutriólogo, Administrador.
- **Proceso (círculo):** `1 — Sistema Calorix`.
- **Almacenes de datos (cilindros):** Usuarios, Alimentos, Registros de comida, Planes, Observaciones, Solicitudes, Documentos profesionales, Registros de peso.

## DFD Nivel 0

```mermaid
flowchart LR
    %% === Entidades externas (rectángulos) ===
    Usuario["👤 Usuario / Paciente"]
    Nutriologo["👨‍⚕️ Nutriólogo"]
    Admin["🛡️ Administrador"]

    %% === Proceso central (círculo) ===
    Sistema((("1<br/>Sistema<br/>Calorix")))

    %% === Almacenes de datos (cilindros) ===
    DSUsuarios[("D1 — Usuarios / Nutriólogos")]
    DSAlimentos[("D2 — Alimentos")]
    DSRegistros[("D3 — Registros de comida")]
    DSPlanes[("D4 — Planes alimenticios")]
    DSObs[("D5 — Observaciones")]
    DSSolic[("D6 — Solicitudes de vinculación")]
    DSDocs[("D7 — Documentos profesionales")]
    DSPeso[("D8 — Registros de peso")]

    %% === Flujos Usuario <-> Sistema ===
    Usuario -- "Credenciales / Datos de registro" --> Sistema
    Usuario -- "Comida consumida (alimento, cantidad, momento)" --> Sistema
    Usuario -- "Actualización de peso/perfil" --> Sistema
    Usuario -- "Respuesta a solicitud de vinculación" --> Sistema
    Sistema -- "Resumen diario / semanal" --> Usuario
    Sistema -- "Plan asignado + Observaciones" --> Usuario
    Sistema -- "Macros recomendados (Mifflin/Harris/Deportiva)" --> Usuario

    %% === Flujos Nutriólogo <-> Sistema ===
    Nutriologo -- "Datos profesionales + Documentos" --> Sistema
    Nutriologo -- "Solicitud de vinculación a paciente" --> Sistema
    Nutriologo -- "Plan alimenticio / Receta clínica" --> Sistema
    Nutriologo -- "Observación clínica" --> Sistema
    Sistema -- "Lista de pacientes" --> Nutriologo
    Sistema -- "Seguimiento del paciente (rango fechas)" --> Nutriologo

    %% === Flujos Admin <-> Sistema ===
    Admin -- "Aprobar / Rechazar cuenta profesional" --> Sistema
    Sistema -- "Nutriólogos pendientes + Documentos" --> Admin

    %% === Flujos Sistema <-> Almacenes ===
    Sistema -- "Lee / Escribe usuario" --> DSUsuarios
    Sistema -- "Consulta tabla nutricional" --> DSAlimentos
    Sistema -- "Inserta registro" --> DSRegistros
    DSRegistros -- "Historial consumido" --> Sistema
    Sistema -- "Crea plan y comidas" --> DSPlanes
    DSPlanes -- "Plan asignado" --> Sistema
    Sistema -- "Crea observación (leida=false)" --> DSObs
    DSObs -- "Notificaciones pendientes" --> Sistema
    Sistema -- "Crea / Actualiza solicitud" --> DSSolic
    DSSolic -- "Estado de vinculación" --> Sistema
    Sistema -- "Guarda archivo + metadatos" --> DSDocs
    DSDocs -- "Documentos para revisión" --> Sistema
    Sistema -- "Registra peso del día" --> DSPeso
    DSPeso -- "Serie de peso (gráficas)" --> Sistema

    %% === Estilos ===
    classDef ext fill:#fff,stroke:#333,stroke-width:1.5px,color:#000
    classDef proc fill:#FFF6D6,stroke:#a07f00,stroke-width:2px,color:#000
    classDef store fill:#E8F0FE,stroke:#1a73e8,stroke-width:1.5px,color:#000

    class Usuario,Nutriologo,Admin ext
    class Sistema proc
    class DSUsuarios,DSAlimentos,DSRegistros,DSPlanes,DSObs,DSSolic,DSDocs,DSPeso store
```

## Descripción de los flujos de datos

| Flujo | Origen | Destino | Datos transportados |
| --- | --- | --- | --- |
| Credenciales / Registro | Usuario | Sistema | Email, contraseña, datos antropométricos |
| Comida consumida | Usuario | Sistema | `alimentoId`, `cantidad`, `unidad`, `momento` |
| Resumen diario | Sistema | Usuario | Total kcal, macros, % cumplimiento vs meta |
| Plan asignado | Sistema | Usuario | `PlanAlimenticio` + `ComidaPlan[]` |
| Datos profesionales | Nutriólogo | Sistema | Cédula, especialidad, años de experiencia |
| Documento profesional | Nutriólogo | Sistema | Archivo PDF/JPG/PNG (max 5 MB) |
| Solicitud de vinculación | Nutriólogo | Sistema | `nutriologoId`, `usuarioId`, fecha |
| Seguimiento del paciente | Sistema | Nutriólogo | `ResumenRango`: kcal/día, peso/día, observaciones |
| Aprobación / Rechazo | Administrador | Sistema | `nutriologoId`, decisión |
| Nutriólogos pendientes | Sistema | Administrador | Lista con documentos para descarga |

## Convenciones de notación

- **Rectángulo** → Entidad externa (actor fuera del sistema).
- **Círculo / Burbuja** → Proceso (transformación que el sistema realiza).
- **Cilindro / Líneas paralelas** → Almacén de datos persistente.
- **Flecha etiquetada** → Flujo de datos con la información concreta que viaja.

> **Nota:** En este nivel 0 (contexto) el sistema completo se modela como un único proceso. Para descomponer en subprocesos numerados (registro de comidas, gestión de planes, validación profesional, etc.) se requeriría un DFD Nivel 1, fuera del alcance actual.
