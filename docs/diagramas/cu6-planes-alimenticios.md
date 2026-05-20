# CU6 — Planes Alimenticios

| Campo | Valor |
| --- | --- |
| **Actor principal** | Nutriólogo |
| **Actores secundarios** | Paciente (recibe notificación) |
| **Alcance** | Aplicación de nutrición (Calorix) |
| **Nivel** | Usuario (Nivel 1) |
| **Precondición** | Nutriólogo con uno o más pacientes vinculados |
| **Postcondición** | Plan asignado a uno o varios pacientes; observación de notificación creada automáticamente |

## Flujo principal

1. El nutriólogo crea o edita recetas clínicas según necesidades del paciente.
2. El nutriólogo diseña un plan de alimentación estructurado.
3. El nutriólogo asigna el plan a uno o varios pacientes.
4. El sistema notifica al paciente con una observación automática.

## Diagrama de secuencia

```mermaid
sequenceDiagram
    actor Nutriologo
    actor Paciente
    participant PlanesController
    participant MisPlanesController
    participant CalorixFacade
    participant Receta
    participant PlanAlimenticio
    participant ComidaPlan
    participant Observacion

    Nutriologo->>PlanesController: CrearRecetaClinica(nombre, descripcion, ingredientesJson)
    PlanesController->>CalorixFacade: CrearRecetaClinica(nutriologoId, nombre, descripcion, ingredientes)
    CalorixFacade->>CalorixFacade: CalcularMacrosTotales()
    CalorixFacade->>Receta: Add(creadaPorNutriologoId)
    Receta-->>CalorixFacade: receta clínica
    CalorixFacade-->>PlanesController: Receta
    PlanesController-->>Nutriologo: Redirect a Index()

    Nutriologo->>PlanesController: Crear(nombre, comidasJson, pacienteIdsJson)
    PlanesController->>CalorixFacade: CrearPlanesAlimenticios(nutriologoId, pacienteIds, nombre, comidas)
    loop Por cada pacienteId
        CalorixFacade->>PlanAlimenticio: Add(plan)
        CalorixFacade->>ComidaPlan: Add(comidas con RecetaId)
        PlanAlimenticio-->>CalorixFacade: plan persistido
        CalorixFacade->>Observacion: Add(texto auto, planId, leida=false)
        Observacion-->>CalorixFacade: notificación creada
    end
    CalorixFacade-->>PlanesController: Lista<PlanAlimenticio>
    PlanesController-->>Nutriologo: Redirect a Index() (N pacientes)

    Paciente->>MisPlanesController: Index()
    MisPlanesController->>CalorixFacade: ObtenerPlanesPaciente(usuarioId)
    CalorixFacade-->>MisPlanesController: Lista<PlanAlimenticio> + Observaciones
    MisPlanesController-->>Paciente: View(Planes + Notificacion)
```

## Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> Borrador : Abrir modal "Nuevo Plan"
    Borrador --> AgregandoComidas : Editar nombre
    AgregandoComidas --> AgregandoComidas : + comida (con receta opcional)
    AgregandoComidas --> SeleccionandoPacientes : Confirmar comidas
    SeleccionandoPacientes --> if_validacion
    state if_validacion <<choice>>
    if_validacion --> Persistiendo : N pacientes seleccionados
    if_validacion --> Fallido : Sin pacientes / sin comidas
    Fallido --> AgregandoComidas : Corregir
    Persistiendo --> NotificandoPaciente : Plan guardado
    NotificandoPaciente --> Asignado : Observación creada
    Asignado --> [*]
```

## Diagrama de colaboración

```mermaid
flowchart LR
    Nutri(["👨‍⚕️ Nutriólogo"])
    Paciente(["👤 Paciente"])
    PCtrl[":PlanesController"]
    MisP[":MisPlanesController"]
    Facade[":CalorixFacade"]
    RecetaObj[":Receta"]
    Plan[":PlanAlimenticio"]
    Comida[":ComidaPlan"]
    Obs[":Observacion"]
    DB[":CalorixDbContext"]

    Nutri  -- "1: crearRecetaClinica(nombre, descripcion, ingredientesJson)" --> PCtrl
    PCtrl  -- "2: CrearRecetaClinica(nutriologoId, ...)"             --> Facade
    Facade -- "3: CalcularMacrosTotales()"                            --> Facade
    Facade -- "4: Add(receta, CreadaPorNutriologoId)"                 --> RecetaObj
    RecetaObj -- "5: SaveChanges()"                                   --> DB
    Facade -- "6: Receta clínica"                                     --> PCtrl
    PCtrl  -- "7: Redirect(Index)"                                    --> Nutri

    Nutri  -- "8: crearPlan(nombre, comidasJson, pacienteIdsJson)"    --> PCtrl
    PCtrl  -- "9a [sin pacientes o sin comidas]: error"                --> Nutri
    PCtrl  -- "9b [válido]: CrearPlanesAlimenticios(nutriologoId, pacienteIds, nombre, comidas)" --> Facade

    Facade -- "10: por cada pacienteId → Add(plan)"                   --> Plan
    Facade -- "11: Add(comidas con RecetaId/AlimentoIds)"             --> Comida
    Plan   -- "12: SaveChanges()"                                     --> DB
    Facade -- "13: Add(observación auto, planId, Leida=false)"        --> Obs
    Obs    -- "14: SaveChanges()"                                     --> DB
    Facade -- "15: Lista&lt;PlanAlimenticio&gt;"                      --> PCtrl
    PCtrl  -- "16: Redirect(Index, mensaje N pacientes)"              --> Nutri

    Paciente -- "17: misPlanes()"                                     --> MisP
    MisP   -- "18: ObtenerPlanesPaciente(usuarioId)"                  --> Facade
    Facade -- "19: Lista&lt;PlanAlimenticio&gt; + Observaciones"      --> MisP
    MisP   -- "20: View(Planes + Notificación)"                       --> Paciente
```
