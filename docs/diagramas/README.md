# Diagramas UML — Calorix

Cada caso de uso definido en el documento de requerimientos cuenta con tres diagramas:

- **Diagrama de secuencia** — flujo Actor → Controlador → Facade → Entidad/BD.
- **Diagrama de estados** — ciclo de vida del recurso principal con caminos de éxito y fallo.
- **Diagrama de colaboración** — interacción espacial entre objetos con mensajes numerados (estilo UML).

Adicionalmente, el sistema cuenta con tres diagramas globales:

- **Diagrama de clases general** — arquitectura completa con patrones (MVC, Facade, Singleton, Strategy).
- **Diagrama de Flujo de Datos (DFD)** — contexto del sistema con entidades externas y almacenes.
- **Modelo lógico de la base de datos** — esquema relacional de las 12 tablas.

Los diagramas usan sintaxis Mermaid y se renderizan automáticamente en Nimbalyst y GitHub.

## Diagramas globales

| Diagrama | Archivo |
|---|---|
| Diagrama de clases general | [diagrama-clases-general.md](diagrama-clases-general.md) |
| Diagrama de Flujo de Datos (Nivel 0) | [diagrama-flujo-datos.md](diagrama-flujo-datos.md) |
| Modelo lógico de la base de datos | [modelo-logico-bd.md](modelo-logico-bd.md) |

## Documentación ágil

| Documento | Archivo |
|---|---|
| Historias de Usuario (34 historias agrupadas por rol) | [../historias-usuario.md](../historias-usuario.md) |

## Casos de uso

| # | Caso de uso | Actor | Archivo |
|---|---|---|---|
| 1 | Gestión de Alimentos | Usuario | [cu1-gestion-alimentos.md](cu1-gestion-alimentos.md) |
| 2 | Registro de Comidas | Usuario | [cu2-registro-comidas.md](cu2-registro-comidas.md) |
| 3 | Seguimiento Nutricional | Usuario | [cu3-seguimiento-nutricional.md](cu3-seguimiento-nutricional.md) |
| 4 | Gestión de Pacientes | Nutriólogo | [cu4-gestion-pacientes.md](cu4-gestion-pacientes.md) |
| 5 | Sistema de Seguimiento Profesional | Nutriólogo | [cu5-seguimiento-profesional.md](cu5-seguimiento-profesional.md) |
| 6 | Planes Alimenticios | Nutriólogo | [cu6-planes-alimenticios.md](cu6-planes-alimenticios.md) |
| 7 | Perfil / Información Personal | Usuario | [cu7-perfil-personal.md](cu7-perfil-personal.md) |
| 8 | Registro Profesional (Nutriólogo) | Nutriólogo + Admin | [cu8-registro-profesional.md](cu8-registro-profesional.md) |

## Convenciones

**Diagramas de secuencia**
- Participantes alineados con la arquitectura real:
  - `Actor` (Usuario / Nutriólogo / Administrador)
  - `Controlador` (`AlimentosController`, `PlanesController`, etc.)
  - `Facade` (`CalorixFacade`)
  - `Entidad/BD` (tablas SQLite vía EF Core)
- Llamadas síncronas con flecha sólida `->>`.
- Respuesta final hacia el actor con flecha punteada `-->>`.

**Diagramas de estados**
- Estado inicial `[*]`, estados intermedios y estado final `[*]`.
- Decisiones modeladas con `state if_state <<choice>>`.
- Camino de error con retorno (`Corregir`) al estado anterior.

**Diagramas de colaboración**
- Implementados como `flowchart LR` en Mermaid (no existe `collaborationDiagram` nativo).
- Mensajes numerados al estilo UML: `1: mensaje()`, `2a [condición]: rama`, `2b [otra condición]: rama`.
- Objetos identificados como `:NombreClase` (instancia anónima).
- Las ramas con condición usan letras (`2a`, `2b`) para indicar caminos alternativos.

**DFD (Diagrama de Flujo de Datos)**
- Notación DeMarco/Yourdon: rectángulos (entidades externas), círculos (procesos), cilindros (almacenes).
- Nivel 0 (contexto): un único proceso central, sin descomposición.

**Modelo lógico**
- Doble representación: diagrama entidad-relación (`erDiagram` de Mermaid) y esquema relacional en tablas markdown.
- PK, FK, UK, NN y CASCADE indicados explícitamente.
