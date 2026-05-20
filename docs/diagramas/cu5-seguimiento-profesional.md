# CU5 — Sistema de Seguimiento Profesional

| Campo | Valor |
| --- | --- |
| **Actor principal** | Nutriólogo |
| **Actores secundarios** | Paciente (indirecto, como dueño de los datos) |
| **Alcance** | Aplicación de nutrición (Calorix) |
| **Nivel** | Usuario (Nivel 1) |
| **Precondición** | Nutriólogo con al menos un paciente vinculado |
| **Postcondición** | Nutriólogo registra observación visible para el paciente |

## Flujo principal

1. El nutriólogo selecciona un paciente de su lista asignada.
2. El sistema despliega la información personal y el historial de comidas.
3. El nutriólogo revisa el consumo nutricional diario y semanal.
4. El nutriólogo monitorea el cumplimiento de objetivos.
5. El nutriólogo escribe una observación sobre el progreso.

## Diagrama de secuencia

```mermaid
sequenceDiagram
    actor Nutriologo
    participant PacientesController
    participant CalorixFacade
    participant Usuario_BD as Usuario(BD)
    participant RegistroComida
    participant Observacion

    Nutriologo->>PacientesController: Index(q, pacienteId, vista, fecha, offset)
    PacientesController->>CalorixFacade: ObtenerPacientes(nutriologoId)
    CalorixFacade-->>PacientesController: Lista<Usuario>
    PacientesController->>CalorixFacade: ObtenerResumenPacienteRango(nutriologoId, pacienteId, desde, hasta)
    CalorixFacade->>Usuario_BD: VerificarVinculacion()
    Usuario_BD-->>CalorixFacade: ok
    CalorixFacade->>RegistroComida: Where(usuarioId, fecha BETWEEN)
    RegistroComida-->>CalorixFacade: Lista<RegistroComida>
    CalorixFacade->>CalorixFacade: SumarMacrosYCalcularPromedios()
    CalorixFacade-->>PacientesController: ResumenRango
    PacientesController->>CalorixFacade: ObtenerObservacionesNutriologo(nutriologoId, pacienteId)
    CalorixFacade-->>PacientesController: Lista<Observacion>
    PacientesController-->>Nutriologo: View(Historial + Observaciones)

    Nutriologo->>PacientesController: EscribirObservacion(pacienteId, planId, texto)
    PacientesController->>CalorixFacade: EscribirObservacion(nutriologoId, pacienteId, planId, texto)
    CalorixFacade->>Observacion: Add(observacion, leida=false)
    Observacion-->>CalorixFacade: observacion guardada
    CalorixFacade-->>PacientesController: Observacion
    PacientesController-->>Nutriologo: Redirect a Index(pacienteId)
```

## Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> SeleccionandoPaciente : Abrir /Pacientes
    SeleccionandoPaciente --> CargandoDatos : Click paciente
    CargandoDatos --> RevisandoHistorial : Datos cargados
    RevisandoHistorial --> EvaluandoObjetivos : Cambiar período
    EvaluandoObjetivos --> RedactandoObservacion : Escribir mensaje
    RedactandoObservacion --> if_texto
    state if_texto <<choice>>
    if_texto --> Enviada : Texto válido
    if_texto --> Fallido : Texto vacío
    Fallido --> RedactandoObservacion : Corregir
    Enviada --> [*]
    RevisandoHistorial --> [*] : Salir sin observar
```

## Diagrama de colaboración

```mermaid
flowchart LR
    Nutri(["👨‍⚕️ Nutriólogo"])
    Ctrl[":PacientesController"]
    Facade[":CalorixFacade"]
    UsuarioObj[":Usuario"]
    Registros[":RegistroComida"]
    Pesos[":RegistroPeso"]
    Obs[":Observacion"]
    DB[":CalorixDbContext"]

    Nutri  -- "1: index(pacienteId, vista, fecha, offset)"          --> Ctrl
    Ctrl   -- "2: ObtenerPacientes(nutriologoId)"                    --> Facade
    Facade -- "3: Lista&lt;Usuario&gt; vinculados"                   --> Ctrl

    Ctrl   -- "4: ObtenerResumenPacienteRango(nutriologoId, pacienteId, desde, hasta)" --> Facade
    Facade -- "5: VerificarVinculacion()"                             --> UsuarioObj
    Facade -- "5a [paciente no vinculado]: error"                     --> Ctrl
    Facade -- "6 [válido]: Where(UsuarioId, Fecha BETWEEN)"           --> Registros
    Registros -- "6.1: query"                                         --> DB
    Facade -- "7: Where(UsuarioId, Fecha BETWEEN)"                   --> Pesos
    Pesos  -- "7.1: query"                                            --> DB
    Facade -- "8: SumarMacros() + Promedios + Compliance"             --> Facade
    Facade -- "9: ResumenRango"                                       --> Ctrl

    Ctrl   -- "10: ObtenerObservacionesNutriologo(nutriologoId, pacienteId)" --> Facade
    Facade -- "11: Lista&lt;Observacion&gt;"                          --> Ctrl
    Ctrl   -- "12: View(Historial + Observaciones)"                   --> Nutri

    Nutri  -- "13: escribirObservacion(pacienteId, planId?, texto)"   --> Ctrl
    Ctrl   -- "14a [texto vacío]: error"                              --> Nutri
    Ctrl   -- "14b [válido]: EscribirObservacion(...)"                 --> Facade
    Facade -- "15: Add(observacion, Leida=false)"                     --> Obs
    Obs    -- "16: SaveChanges()"                                     --> DB
    Facade -- "17: Observacion"                                       --> Ctrl
    Ctrl   -- "18: Redirect(Index, pacienteId)"                       --> Nutri
```
