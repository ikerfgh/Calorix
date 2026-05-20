# CU7 — Perfil / Información Personal

| Campo | Valor |
| --- | --- |
| **Actor principal** | Usuario |
| **Alcance** | Aplicación de nutrición (Calorix) |
| **Nivel** | Usuario (Nivel 1) |
| **Precondición** | Usuario autenticado |
| **Postcondición** | Datos antropométricos actualizados; meta calórica recalculada; nuevo registro de peso si cambió |

## Flujo principal

1. El usuario accede a la configuración de su perfil.
2. El usuario actualiza sus medidas antropométricas (peso, altura).
3. El usuario modifica datos básicos como edad y nivel de actividad física.
4. El sistema recalcula los requerimientos calóricos basados en la nueva información.

## Diagrama de secuencia

```mermaid
sequenceDiagram
    actor Usuario
    participant PerfilController
    participant CalorixFacade
    participant CalculadorNutricional
    participant Usuario_BD as Usuario(BD)
    participant RegistroPeso

    Usuario->>PerfilController: Index()
    PerfilController->>CalorixFacade: ObtenerPerfil(usuarioId)
    CalorixFacade->>Usuario_BD: FirstOrDefault(id)
    Usuario_BD-->>CalorixFacade: Usuario
    CalorixFacade-->>PerfilController: Usuario
    PerfilController-->>Usuario: View(Formulario)

    Usuario->>PerfilController: Actualizar(peso, altura, edad, actividad)
    PerfilController->>CalorixFacade: ActualizarPerfil(usuarioId, peso, altura, edad, actividad)
    CalorixFacade->>Usuario_BD: Update(propiedades)
    CalorixFacade->>CalculadorNutricional: CalcularMeta(usuario)
    CalculadorNutricional-->>CalorixFacade: nueva MetaCalorica
    CalorixFacade->>Usuario_BD: SetMetaCalorica(nuevaMeta)
    alt Peso cambió
        CalorixFacade->>RegistroPeso: Add(usuarioId, peso, fecha)
        RegistroPeso-->>CalorixFacade: registro creado
    end
    CalorixFacade-->>PerfilController: ok
    PerfilController-->>Usuario: Redirect a Index() (metaRecalculada)
```

## Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> Visualizando : Abrir /Perfil
    Visualizando --> Editando : Modificar campos
    Editando --> Validando : Submit form
    Validando --> if_validacion
    state if_validacion <<choice>>
    if_validacion --> Recalculando : Datos válidos
    if_validacion --> Fallido : Valores fuera de rango
    Fallido --> Editando : Corregir
    Recalculando --> RegistrandoPeso : Peso cambió
    Recalculando --> Guardado : Peso sin cambios
    RegistrandoPeso --> Guardado : Peso registrado
    Guardado --> Visualizando : Reset form
    Guardado --> [*] : Salir
```

## Diagrama de colaboración

```mermaid
flowchart LR
    Actor(["👤 Usuario"])
    Ctrl[":PerfilController"]
    Facade[":CalorixFacade"]
    Strategy[":CalculoNutricionalStrategy"]
    UsuarioObj[":Usuario"]
    Peso[":RegistroPeso"]
    DB[":CalorixDbContext"]

    Actor  -- "1: index()"                                    --> Ctrl
    Ctrl   -- "2: ObtenerPerfil(usuarioId)"                    --> Facade
    Facade -- "3: FirstOrDefault(Id)"                          --> UsuarioObj
    UsuarioObj -- "3.1: Usuario"                               --> DB
    Facade -- "4: Usuario"                                     --> Ctrl
    Ctrl   -- "5: View(Formulario)"                            --> Actor

    Actor  -- "6: actualizar(peso, altura, edad, actividad)"   --> Ctrl
    Ctrl   -- "7: validarRango()"                              --> Ctrl
    Ctrl   -- "7a [fuera de rango]: error"                     --> Actor
    Ctrl   -- "7b [válido]: ActualizarPerfil(usuarioId, ...)"  --> Facade
    Facade -- "8: Update(propiedades)"                         --> UsuarioObj
    Facade -- "9: CalcularMeta(usuario)"                       --> Strategy
    Strategy -- "10: nueva MetaCalorica"                       --> Facade
    Facade -- "11: SetMetaCalorica(nuevaMeta)"                 --> UsuarioObj

    Facade -- "12a [peso cambió]: Add(usuarioId, peso, fecha)" --> Peso
    Peso   -- "12a.1: SaveChanges()"                           --> DB
    Facade -- "12b [peso sin cambios]: skip"                   --> Facade

    Facade -- "13: ok"                                         --> Ctrl
    Ctrl   -- "14: Redirect(Index, metaRecalculada)"           --> Actor
```
