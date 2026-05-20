# CU2 — Registro de Comidas

| Campo | Valor |
| --- | --- |
| **Actor principal** | Usuario |
| **Alcance** | Aplicación de nutrición (Calorix) |
| **Nivel** | Usuario (Nivel 1) |
| **Precondición** | Usuario autenticado |
| **Postcondición** | Comida registrada en el diario diario con macros calculados |

## Flujo principal

1. El usuario selecciona el momento del día (Desayuno, Comida, Cena o Snack).
2. El usuario busca y selecciona un alimento o una receta guardada.
3. El usuario especifica la cantidad consumida (gramos o porciones).
4. El sistema registra la entrada en el diario del día actual.
5. El usuario consulta el historial diario para verificar el total.

## Diagrama de secuencia

```mermaid
sequenceDiagram
    actor Usuario
    participant RegistroController
    participant CalorixFacade
    participant Alimento
    participant Receta
    participant RegistroComida

    Note over Usuario: Selecciona momento del día en la UI
    Usuario->>RegistroController: Index(q)
    RegistroController->>CalorixFacade: ObtenerTodosAlimentos()
    CalorixFacade-->>RegistroController: Alimentos + Recetas
    RegistroController->>CalorixFacade: ObtenerResumenDiario(usuarioId, hoy)
    CalorixFacade-->>RegistroController: ResumenDiario
    RegistroController-->>Usuario: View(Resultados + Diario)

    Usuario->>RegistroController: Registrar(alimentoId, esReceta, cantidad, momento)
    alt Es Receta
        RegistroController->>CalorixFacade: RegistrarRecetaComida(usuarioId, recetaId, porciones, momento)
        CalorixFacade->>Receta: Obtener(recetaId)
        Receta-->>CalorixFacade: macros base
    else Es Alimento
        RegistroController->>CalorixFacade: RegistrarComida(usuarioId, alimentoId, cantidad, momento)
        CalorixFacade->>Alimento: Obtener(alimentoId)
        Alimento-->>CalorixFacade: macros por 100g
    end
    CalorixFacade->>CalorixFacade: CalcularMacrosConsumidos()
    CalorixFacade->>RegistroComida: Add(registro)
    RegistroComida-->>CalorixFacade: registro persistido
    CalorixFacade-->>RegistroController: RegistroComida
    RegistroController-->>Usuario: Redirect a Index()
```

## Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> SeleccionandoMomento : Abrir registro
    SeleccionandoMomento --> BuscandoItem : Confirmar momento
    BuscandoItem --> ItemSeleccionado : Seleccionar alimento/receta
    ItemSeleccionado --> EspecificandoCantidad : Definir cantidad
    EspecificandoCantidad --> if_validacion
    state if_validacion <<choice>>
    if_validacion --> Calculando : Cantidad > 0
    if_validacion --> Fallido : Cantidad inválida
    Fallido --> EspecificandoCantidad : Corregir
    Calculando --> Registrado : Persistir entrada
    Registrado --> [*]
```

## Diagrama de colaboración

```mermaid
flowchart LR
    Actor(["👤 Usuario"])
    Ctrl[":RegistroController"]
    Facade[":CalorixFacade"]
    AlimentoObj[":Alimento"]
    RecetaObj[":Receta"]
    Registro[":RegistroComida"]
    DB[":CalorixDbContext"]

    Actor  -- "1: registrar(alimentoId, esReceta, cantidad, momento)" --> Ctrl
    Ctrl   -- "2: validarCantidad()"                                   --> Ctrl
    Ctrl   -- "2a [cantidad &lt;= 0]: error"                            --> Actor

    Ctrl   -- "3a [esReceta=false]: RegistrarComida(usuarioId, alimentoId, cantidad, momento)" --> Facade
    Facade -- "4a: Obtener(alimentoId)"                                --> AlimentoObj
    AlimentoObj -- "4a.1: macros por 100g"                             --> Facade

    Ctrl   -- "3b [esReceta=true]: RegistrarRecetaComida(usuarioId, recetaId, porciones, momento)" --> Facade
    Facade -- "4b: Obtener(recetaId)"                                  --> RecetaObj
    RecetaObj -- "4b.1: macros totales"                                --> Facade

    Facade -- "5: CalcularMacrosConsumidos(cantidad)"                  --> Facade
    Facade -- "6: Add(registro)"                                       --> Registro
    Registro -- "7: SaveChanges()"                                     --> DB
    Facade -- "8: RegistroComida persistido"                           --> Ctrl
    Ctrl   -- "9: Redirect(Index)"                                     --> Actor
```
