# CU4 — Gestión de Pacientes

| Campo | Valor |
| --- | --- |
| **Actor principal** | Nutriólogo |
| **Alcance** | Aplicación de nutrición (Calorix) |
| **Nivel** | Usuario (Nivel 1) |
| **Precondición** | Nutriólogo autenticado y validado |
| **Postcondición** | Paciente vinculado al nutriólogo (tras aprobación del usuario) |

## Flujo principal

1. El nutriólogo busca un usuario por nombre o correo.
2. El nutriólogo envía una solicitud de vinculación.
3. El sistema valida la existencia del usuario y registra la solicitud.
4. El usuario aprueba la solicitud y queda vinculado al nutriólogo.
5. El nutriólogo organiza su lista de pacientes activos.

## Diagrama de secuencia

```mermaid
sequenceDiagram
    actor Nutriologo
    actor Usuario
    participant PacientesController
    participant SeguimientoController
    participant CalorixFacade
    participant Usuario_BD as Usuario(BD)
    participant SolicitudVinculacion
    participant Nutriologo_BD as Nutriologo(BD)

    Nutriologo->>PacientesController: Index(q)
    PacientesController->>CalorixFacade: BuscarUsuarios(termino)
    CalorixFacade->>Usuario_BD: Where(nombre/email LIKE)
    Usuario_BD-->>CalorixFacade: Lista<Usuario>
    CalorixFacade-->>PacientesController: Lista<Usuario>
    PacientesController-->>Nutriologo: View(Resultados)

    Nutriologo->>PacientesController: Vincular(usuarioId)
    PacientesController->>CalorixFacade: EnviarSolicitudVinculacion(nutriologoId, usuarioId)
    CalorixFacade->>Usuario_BD: VerificarExistencia()
    Usuario_BD-->>CalorixFacade: Usuario válido
    CalorixFacade->>SolicitudVinculacion: Add(estado=pendiente)
    SolicitudVinculacion-->>CalorixFacade: solicitud creada
    CalorixFacade-->>PacientesController: (ok, mensaje)
    PacientesController-->>Nutriologo: Redirect a Index()

    Usuario->>SeguimientoController: ResponderSolicitud(solicitudId, aprobar)
    SeguimientoController->>CalorixFacade: ResponderSolicitud(usuarioId, solicitudId, aprobar)
    CalorixFacade->>Nutriologo_BD: AgregarPacienteId(usuarioId)
    CalorixFacade->>SolicitudVinculacion: Update(estado=aprobada)
    CalorixFacade-->>SeguimientoController: ok
    SeguimientoController-->>Usuario: Redirect a Index()

    Nutriologo->>PacientesController: Index()
    PacientesController->>CalorixFacade: ObtenerPacientes(nutriologoId)
    CalorixFacade-->>PacientesController: Lista<Usuario>
    PacientesController-->>Nutriologo: View(PacientesActivos)
```

## Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> Buscando : Abrir buscador
    Buscando --> EnviandoSolicitud : Seleccionar usuario
    EnviandoSolicitud --> if_validacion
    state if_validacion <<choice>>
    if_validacion --> Pendiente : Usuario válido y libre
    if_validacion --> Fallido : Usuario ya tiene nutriólogo
    Fallido --> Buscando : Corregir
    Pendiente --> EsperandoRespuesta : Solicitud creada
    EsperandoRespuesta --> Aprobada : Usuario acepta
    EsperandoRespuesta --> Rechazada : Usuario rechaza
    Aprobada --> Vinculado : Agregar a PacienteIds
    Vinculado --> [*]
    Rechazada --> [*]
```

## Diagrama de colaboración

```mermaid
flowchart LR
    Nutri(["👨‍⚕️ Nutriólogo"])
    User(["👤 Usuario"])
    PCtrl[":PacientesController"]
    SCtrl[":SeguimientoController"]
    Facade[":CalorixFacade"]
    UsuarioObj[":Usuario"]
    Solicitud[":SolicitudVinculacion"]
    NutriObj[":Nutriologo"]
    DB[":CalorixDbContext"]

    Nutri  -- "1: buscarUsuario(q)"                                --> PCtrl
    PCtrl  -- "2: BuscarUsuarios(termino)"                          --> Facade
    Facade -- "3: Where(Nombre/Email LIKE)"                         --> UsuarioObj
    UsuarioObj -- "3.1: Lista&lt;Usuario&gt;"                       --> DB
    Facade -- "4: Lista&lt;Usuario&gt;"                             --> PCtrl
    PCtrl  -- "5: View(Resultados)"                                 --> Nutri

    Nutri  -- "6: vincular(usuarioId)"                              --> PCtrl
    PCtrl  -- "7: EnviarSolicitudVinculacion(nutriologoId, usuarioId)" --> Facade
    Facade -- "8: VerificarExistencia(usuarioId)"                   --> UsuarioObj
    Facade -- "8a [usuario no existe]: error"                        --> PCtrl
    Facade -- "8b [ya vinculado]: rechazar"                          --> PCtrl
    Facade -- "9 [válido]: Add(estado='pendiente')"                  --> Solicitud
    Solicitud -- "10: SaveChanges()"                                --> DB
    Facade -- "11: (ok, mensaje)"                                   --> PCtrl
    PCtrl  -- "12: Redirect(Index)"                                 --> Nutri

    User   -- "13: responderSolicitud(solicitudId, aprobar)"        --> SCtrl
    SCtrl  -- "14: ResponderSolicitud(usuarioId, solicitudId, aprobar)" --> Facade
    Facade -- "15a [aprobar=true]: AgregarPacienteId(usuarioId)"    --> NutriObj
    Facade -- "16a [aprobar=true]: Update(estado='aprobada')"       --> Solicitud
    Facade -- "15b [aprobar=false]: Update(estado='rechazada')"     --> Solicitud
    Facade -- "17: ok"                                              --> SCtrl
    SCtrl  -- "18: Redirect(Index)"                                 --> User
```
