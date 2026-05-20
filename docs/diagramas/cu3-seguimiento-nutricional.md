# CU3 — Seguimiento Nutricional

| Campo | Valor |
| --- | --- |
| **Actor principal** | Usuario |
| **Alcance** | Aplicación de nutrición (Calorix) |
| **Nivel** | Usuario (Nivel 1) |
| **Precondición** | Usuario autenticado con registros de comidas |
| **Postcondición** | El usuario visualiza su progreso vs. metas con gráficas |

## Flujo principal

1. El usuario ingresa al dashboard de estadísticas.
2. El sistema calcula la suma total de calorías y macronutrientes consumidos.
3. El sistema compara los datos actuales con las metas diarias.
4. El usuario visualiza gráficas de progreso semanal o mensual.

## Diagrama de secuencia

```mermaid
sequenceDiagram
    actor Usuario
    participant SeguimientoController
    participant CalorixFacade
    participant CalculadorNutricional
    participant RegistroComida
    participant Usuario_BD as Usuario(BD)

    Usuario->>SeguimientoController: Index(formula, periodo)
    SeguimientoController->>CalorixFacade: ObtenerResumenDiario(usuarioId, hoy)
    CalorixFacade->>RegistroComida: Where(usuarioId, fecha)
    RegistroComida-->>CalorixFacade: Lista<RegistroComida>
    CalorixFacade->>CalorixFacade: SumarMacros()
    CalorixFacade->>Usuario_BD: ObtenerMetaCalorica()
    Usuario_BD-->>CalorixFacade: meta
    CalorixFacade->>CalorixFacade: PorcentajeCumplimiento()
    CalorixFacade-->>SeguimientoController: ResumenDiario

    SeguimientoController->>CalorixFacade: ObtenerEstadisticasSemana(usuarioId, weekOffset)
    CalorixFacade->>RegistroComida: AgruparPorDia(últimos 7)
    RegistroComida-->>CalorixFacade: DatosPorDia
    CalorixFacade-->>SeguimientoController: EstadisticasSemana

    SeguimientoController->>CalorixFacade: ObtenerMacrosRecomendados(usuarioId, formula)
    CalorixFacade->>CalculadorNutricional: CalcularMacros(usuario)
    CalculadorNutricional-->>CalorixFacade: MacrosRecomendados
    CalorixFacade-->>SeguimientoController: MacrosRecomendados

    SeguimientoController-->>Usuario: View(Graficas + Progreso)
```

## Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> CargandoDashboard : Abrir /Seguimiento
    CargandoDashboard --> CalculandoTotales : Obtener registros
    CalculandoTotales --> ComparandoVsMeta : Aplicar fórmula
    ComparandoVsMeta --> if_datos
    state if_datos <<choice>>
    if_datos --> RenderizandoGraficas : Hay datos
    if_datos --> SinDatos : Sin registros
    SinDatos --> CargandoDashboard : Recargar
    RenderizandoGraficas --> Visualizado
    Visualizado --> NavegandoSemana : Cambiar período
    NavegandoSemana --> CalculandoTotales : Nuevo offset
    Visualizado --> [*] : Cerrar
```

## Diagrama de colaboración

```mermaid
flowchart LR
    Actor(["👤 Usuario"])
    Ctrl[":SeguimientoController"]
    Facade[":CalorixFacade"]
    Strategy[":CalculoNutricionalStrategy"]
    Registros[":RegistroComida"]
    UsuarioObj[":Usuario"]
    DB[":CalorixDbContext"]

    Actor    -- "1: index(formula, periodo)"                       --> Ctrl
    Ctrl     -- "2: ObtenerResumenDiario(usuarioId, hoy)"          --> Facade
    Facade   -- "3: Where(UsuarioId, Fecha)"                       --> Registros
    Registros -- "3.1: Lista&lt;RegistroComida&gt;"                --> DB
    DB       -- "3.2: query result"                                --> Facade
    Facade   -- "4: SumarMacros()"                                 --> Facade
    Facade   -- "5: ObtenerMetaCalorica()"                         --> UsuarioObj
    UsuarioObj -- "5.1: meta"                                      --> Facade
    Facade   -- "6: PorcentajeCumplimiento()"                      --> Facade
    Facade   -- "7: ResumenDiario"                                 --> Ctrl

    Ctrl     -- "8: ObtenerEstadisticasSemana(usuarioId, offset)"  --> Facade
    Facade   -- "9: AgruparPorDia(7 días)"                         --> Registros
    Facade   -- "10: EstadisticasSemana"                           --> Ctrl

    Ctrl     -- "11: ObtenerMacrosRecomendados(usuarioId, formula)" --> Facade
    Facade   -- "12a [formula=Mifflin]: calcular(usuario)"          --> Strategy
    Facade   -- "12b [formula=HarrisBenedict]: calcular(usuario)"   --> Strategy
    Facade   -- "12c [formula=Deportiva]: calcular(usuario)"        --> Strategy
    Strategy -- "13: MacrosRecomendados"                            --> Facade
    Facade   -- "14: MacrosRecomendados"                            --> Ctrl
    Ctrl     -- "15: View(Graficas + Progreso)"                     --> Actor
```
