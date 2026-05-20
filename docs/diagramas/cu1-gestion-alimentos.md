# CU1 — Gestión de Alimentos

| Campo | Valor |
| --- | --- |
| **Actor principal** | Usuario |
| **Alcance** | Aplicación de nutrición (Calorix) |
| **Nivel** | Usuario (Nivel 1) |
| **Precondición** | Usuario autenticado |
| **Postcondición** | Receta personalizada guardada en biblioteca del usuario |

## Flujo principal

1. El usuario accede al buscador de alimentos.
2. El sistema muestra la información nutricional (calorías, macros) del alimento seleccionado.
3. El usuario crea una receta personalizada.
4. El usuario agrega múltiples ingredientes y cantidades.
5. El sistema calcula el valor total de la receta.
6. El sistema guarda la receta en la biblioteca personal.

## Diagrama de secuencia

```mermaid
sequenceDiagram
    actor Usuario
    participant AlimentosController
    participant CalorixFacade
    participant Alimento
    participant Receta

    Usuario->>AlimentosController: Index(q)
    AlimentosController->>CalorixFacade: BuscarAlimentos(termino)
    CalorixFacade->>Alimento: Where(nombre/categoria)
    Alimento-->>CalorixFacade: Lista<Alimento>
    CalorixFacade-->>AlimentosController: Lista<Alimento>
    AlimentosController-->>Usuario: View(InfoNutricional)

    Note over Usuario: Agrega ingredientes en la UI<br/>(se serializan a ingredientesJson)
    Usuario->>AlimentosController: CrearReceta(nombre, descripcion, ingredientesJson)
    AlimentosController->>CalorixFacade: CrearReceta(usuarioId, nombre, descripcion, ingredientes)
    CalorixFacade->>Alimento: Obtener(alimentoId)
    Alimento-->>CalorixFacade: macros base
    CalorixFacade->>CalorixFacade: CalcularTotalReceta()
    CalorixFacade->>Receta: Add(receta)
    Receta-->>CalorixFacade: receta guardada
    CalorixFacade-->>AlimentosController: Receta
    AlimentosController-->>Usuario: View(RecetaCreada)
```

## Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> Buscando : Abrir buscador
    Buscando --> ConsultandoInfo : Seleccionar alimento
    ConsultandoInfo --> AgregandoIngredientes : Crear receta
    AgregandoIngredientes --> AgregandoIngredientes : + ingrediente
    AgregandoIngredientes --> Calculando : Confirmar receta
    Calculando --> if_validacion
    state if_validacion <<choice>>
    if_validacion --> Guardando : Ingredientes válidos
    if_validacion --> Fallido : Sin ingredientes
    Fallido --> AgregandoIngredientes : Corregir
    Guardando --> Guardada : Persistir en BD
    Guardada --> [*]
```

## Diagrama de colaboración

```mermaid
flowchart LR
    Actor(["👤 Usuario"])
    Ctrl[":AlimentosController"]
    Facade[":CalorixFacade"]
    DB[":CalorixDbContext"]
    AlimentoObj[":Alimento"]
    RecetaObj[":Receta"]

    Actor       -- "1: buscar(q)"                                     --> Ctrl
    Ctrl        -- "2: BuscarAlimentos(termino)"                      --> Facade
    Facade      -- "3: Where(Nombre/Categoria LIKE)"                  --> DB
    DB          -- "3.1: Lista&lt;Alimento&gt;"                       --> AlimentoObj
    AlimentoObj -- "3.2: macros por 100g"                             --> Facade
    Facade      -- "4: Lista&lt;Alimento&gt;"                         --> Ctrl
    Ctrl        -- "5: View(InfoNutricional)"                         --> Actor

    Actor   -- "6: crearReceta(nombre, ingredientesJson)"         --> Ctrl
    Ctrl    -- "7a [sin ingredientes]: error"                     --> Actor
    Ctrl    -- "7b [válido]: CrearReceta(usuarioId, ingredientes)" --> Facade
    Facade  -- "8: CalcularTotalReceta()"                         --> Facade
    Facade  -- "9: Add(receta)"                                   --> RecetaObj
    RecetaObj -- "10: SaveChanges()"                              --> DB
    Facade  -- "11: Receta guardada"                              --> Ctrl
    Ctrl    -- "12: View(RecetaCreada)"                           --> Actor
```
