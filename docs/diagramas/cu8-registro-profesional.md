# CU8 — Gestión de Nutriólogos (Registro Profesional)

| Campo | Valor |
| --- | --- |
| **Actor principal** | Nutriólogo |
| **Actores secundarios** | Administrador (revisa y valida) |
| **Alcance** | Aplicación de nutrición (Calorix) |
| **Nivel** | Usuario (Nivel 1) |
| **Precondición** | Ninguna (registro público) |
| **Postcondición** | Cuenta de nutriólogo validada y operativa, o rechazada |

## Flujo principal

1. El profesional accede al formulario de registro para nutriólogos.
2. El nutriólogo ingresa su información profesional: cédula, especialidad y años de experiencia.
3. El nutriólogo adjunta la documentación requerida para validar su identidad.
4. El sistema envía la solicitud al administrador para revisión.
5. El administrador aprueba o rechaza la cuenta.

## Diagrama de secuencia

```mermaid
sequenceDiagram
    actor Nutriologo
    actor Admin
    participant NutriologoController
    participant AdminController
    participant CalorixFacade
    participant Nutriologo_BD as Nutriologo(BD)
    participant DocumentoNutriologo
    participant FileSystem

    Nutriologo->>NutriologoController: Registrar(nombre, email, password, cedula, especialidad, aniosExp)
    NutriologoController->>CalorixFacade: RegistrarNutriologo(...)
    CalorixFacade->>Nutriologo_BD: Add(validado=false)
    Nutriologo_BD-->>CalorixFacade: nutriologo creado
    CalorixFacade-->>NutriologoController: Nutriologo
    NutriologoController-->>Nutriologo: Redirect a Index() (pendiente)

    Nutriologo->>NutriologoController: SubirDocumento(documento)
    NutriologoController->>NutriologoController: ValidarTipoYTamano()
    NutriologoController->>FileSystem: Guardar(uploads/nutriologos/{id}/...)
    FileSystem-->>NutriologoController: rutaRelativa
    NutriologoController->>CalorixFacade: RegistrarDocumento(nutriologoId, nombre, ruta, tipo, tamano)
    CalorixFacade->>DocumentoNutriologo: Add(estado=pendiente)
    DocumentoNutriologo-->>CalorixFacade: documento guardado
    CalorixFacade-->>NutriologoController: DocumentoNutriologo
    NutriologoController-->>Nutriologo: Redirect a Index()

    Admin->>AdminController: Index()
    AdminController->>CalorixFacade: ObtenerNutriologosPendientes()
    CalorixFacade-->>AdminController: Lista<Nutriologo>
    AdminController-->>Admin: View(PendientesConDocumentos)

    Admin->>AdminController: Aprobar(nutriologoId)
    AdminController->>CalorixFacade: ValidarNutriologo(nutriologoId, true)
    CalorixFacade->>Nutriologo_BD: SetValidado(true)
    CalorixFacade->>DocumentoNutriologo: SetEstado(aprobado)
    CalorixFacade-->>AdminController: ok
    AdminController-->>Admin: Redirect a Index()
```

## Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> Capturando : Abrir formulario
    Capturando --> Registrando : Submit datos profesionales
    Registrando --> EsperandoDocs : Cuenta creada (Validado=false)
    EsperandoDocs --> SubiendoDoc : Adjuntar archivo
    SubiendoDoc --> if_archivo
    state if_archivo <<choice>>
    if_archivo --> EnRevision : Tipo y tamaño OK
    if_archivo --> Fallido : Tipo no permitido o > 5 MB
    Fallido --> SubiendoDoc : Corregir archivo
    EnRevision --> if_admin
    state if_admin <<choice>>
    if_admin --> Validada : Admin aprueba
    if_admin --> Rechazada : Admin rechaza
    Validada --> [*]
    Rechazada --> SubiendoDoc : Reintentar con nueva doc
```

## Diagrama de colaboración

```mermaid
flowchart LR
    Nutri(["👨‍⚕️ Nutriólogo"])
    Admin(["🛡️ Administrador"])
    NCtrl[":NutriologoController"]
    ACtrl[":AdminController"]
    Facade[":CalorixFacade"]
    NutriObj[":Nutriologo"]
    Doc[":DocumentoNutriologo"]
    FS[":FileSystem"]
    DB[":CalorixDbContext"]

    Nutri  -- "1: registrar(nombre, email, password, cedula, especialidad, aniosExp)" --> NCtrl
    NCtrl  -- "2a [email duplicado]: error"                          --> Nutri
    NCtrl  -- "2b [válido]: RegistrarNutriologo(...)"                --> Facade
    Facade -- "3: Add(Validado=false)"                                --> NutriObj
    NutriObj -- "4: SaveChanges()"                                   --> DB
    Facade -- "5: Nutriologo"                                        --> NCtrl
    NCtrl  -- "6: Redirect(Index pendiente)"                         --> Nutri

    Nutri  -- "7: subirDocumento(archivo)"                           --> NCtrl
    NCtrl  -- "8: ValidarTipoYTamano()"                              --> NCtrl
    NCtrl  -- "8a [tipo no permitido o &gt; 5 MB]: error"            --> Nutri
    NCtrl  -- "8b [válido]: Guardar(uploads/nutriologos/{id}/...)"   --> FS
    FS     -- "9: rutaRelativa"                                      --> NCtrl
    NCtrl  -- "10: RegistrarDocumento(nutriologoId, nombre, ruta, tipo, tamano)" --> Facade
    Facade -- "11: Add(estado='pendiente')"                          --> Doc
    Doc    -- "12: SaveChanges()"                                    --> DB
    Facade -- "13: DocumentoNutriologo"                              --> NCtrl
    NCtrl  -- "14: Redirect(Index)"                                  --> Nutri

    Admin  -- "15: index()"                                          --> ACtrl
    ACtrl  -- "16: ObtenerNutriologosPendientes()"                    --> Facade
    Facade -- "17: Lista&lt;Nutriologo&gt; + Documentos"             --> ACtrl
    ACtrl  -- "18: View(Pendientes)"                                 --> Admin

    Admin  -- "19a: aprobar(nutriologoId)"                           --> ACtrl
    ACtrl  -- "20a: ValidarNutriologo(nutriologoId, true)"           --> Facade
    Facade -- "21a: SetValidado(true)"                               --> NutriObj
    Facade -- "22a: SetEstado('aprobado')"                            --> Doc

    Admin  -- "19b: rechazar(nutriologoId)"                          --> ACtrl
    ACtrl  -- "20b: ValidarNutriologo(nutriologoId, false)"          --> Facade
    Facade -- "21b: SetValidado(false)"                              --> NutriObj
    Facade -- "22b: SetEstado('rechazado')"                          --> Doc

    Facade -- "23: ok"                                               --> ACtrl
    ACtrl  -- "24: Redirect(Index)"                                  --> Admin
```
