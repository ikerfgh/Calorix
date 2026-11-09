# Documentación de uso de las APIs de Calorix

Calorix expone dos servicios sobre la misma lógica de negocio (`CalorixFacade`):

| Servicio | URL base | Documentación interactiva |
|---|---|---|
| REST | `http://localhost:5080/api` | `http://localhost:5080/swagger` |
| GraphQL | `http://localhost:5080/graphql` | El mismo URL en el navegador abre el explorador **Nitro** |

## Autenticación

Ambas APIs usan la **cookie de sesión** de la aplicación.

- **Desde el navegador** (Swagger o Nitro): inicia sesión en `http://localhost:5080` con `maria@demo.com` / `demo123` y luego abre `/swagger` o `/graphql` en la misma pestaña.
- **Desde la terminal** (curl): guarda la cookie al iniciar sesión y reúsala.

```bash
curl -c cookies.txt -d "email=maria@demo.com&password=demo123" http://localhost:5080/Home/Login
```

El catálogo de alimentos es público. Los demás recursos responden **401** (REST) o el error **`NO_AUTENTICADO`** (GraphQL) si no hay sesión.

---

## API REST

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| GET | `/api/alimentos?buscar=pollo` | Catálogo (público), filtra por nombre o categoría | 200 |
| GET | `/api/alimentos/{id}` | Un alimento | 200, 404 |
| GET | `/api/perfil` | Perfil del usuario en sesión (sin contraseña) | 200, 401 |
| GET | `/api/resumen?fecha=2026-11-09` | Calorías y macros del día vs. meta | 200, 401 |
| GET | `/api/registros?fecha=2026-11-09` | Comidas registradas en el día | 200, 401 |
| POST | `/api/registros` | Registra una comida | 201, 400, 401 |
| DELETE | `/api/registros/{id}` | Elimina un registro propio | 204, 404, 401 |
| GET | `/api/planes` | Planes alimenticios asignados | 200, 401 |
| GET | `/api/peso?dias=30` | Historial de peso | 200, 401 |
| POST | `/api/peso` | Registra el peso | 201, 400, 401 |

Los errores devuelven `{ "error": "mensaje" }`.

### Ejemplos

```bash
# Catálogo (sin sesión)
curl "http://localhost:5080/api/alimentos?buscar=arroz"

# Perfil (con sesión)
curl -b cookies.txt http://localhost:5080/api/perfil

# Registrar 150 g de arroz (alimento 2) en el desayuno
curl -b cookies.txt -H "Content-Type: application/json" \
     -d '{"alimentoId":2,"cantidad":150,"momento":"Desayuno"}' \
     http://localhost:5080/api/registros

# Resumen del día
curl -b cookies.txt http://localhost:5080/api/resumen
```

Respuesta de `POST /api/registros` (201):

```json
{ "id": 42, "usuarioId": 1, "alimentoId": 2, "nombreAlimento": "Arroz Blanco Cocido",
  "cantidad": 150, "unidadCantidad": "g", "momento": "Desayuno",
  "calorias": 195, "proteinas": 4, "carbohidratos": 42, "grasas": 0 }
```

Validaciones: `cantidad` entre 1 y 5000 g; `peso` entre 20 y 400 kg; el alimento debe existir.

---

## API GraphQL

Un solo endpoint `POST /graphql`. A diferencia de REST, el cliente pide **exactamente los campos que necesita** y puede combinar varios recursos en una sola petición.

### Consultas (Query)

| Campo | Argumentos | Devuelve |
|---|---|---|
| `alimentos` | `buscar: String` | `[Alimento]` (público) |
| `alimento` | `id: Int!` | `Alimento` (público) |
| `perfil` | — | `PerfilDto` |
| `resumenDiario` | `fecha: DateTime` | `ResumenDiario` |
| `macrosRecomendados` | `formula: String!` (`mifflin`, `harris`, `deportiva`) | `MacrosRecomendados` |
| `historialPeso` | `dias: Int!` | `[RegistroPeso]` |
| `misPlanes` | — | `[PlanAlimenticio]` |

### Mutaciones (Mutation)

| Campo | Argumentos | Devuelve |
|---|---|---|
| `registrarComida` | `alimentoId: Int!, cantidad: Float!, momento: String!` | `RegistroComida` |
| `eliminarRegistro` | `registroId: Int!` | `Boolean` |
| `registrarPeso` | `peso: Float!, notas: String` | `RegistroPeso` |

### Ejemplos

Dashboard completo en **una sola petición**:

```graphql
{
  perfil { nombre metaCalorica }
  resumenDiario { totalCalorias totalProteinas porcentajeCumplimiento
                  registros { nombreAlimento momento calorias } }
  macrosRecomendados(formula: "mifflin") { caloriasMeta proteinasGramos }
  misPlanes { nombre activo comidas { momento descripcion calorias } }
}
```

Registrar una comida:

```graphql
mutation {
  registrarComida(alimentoId: 1, cantidad: 150, momento: "Desayuno") { id calorias }
}
```

Con curl:

```bash
curl -b cookies.txt -H "Content-Type: application/json" \
     -d '{"query":"{ perfil { nombre } resumenDiario { totalCalorias } }"}' \
     http://localhost:5080/graphql
```

Error sin sesión:

```json
{ "errors": [ { "message": "No autenticado: inicia sesión como usuario en Calorix.",
                "extensions": { "code": "NO_AUTENTICADO" } } ], "data": null }
```

---

## REST vs GraphQL en Calorix

| | REST | GraphQL |
|---|---|---|
| Endpoints | Uno por recurso (10) | Uno solo |
| Datos devueltos | Fijos por endpoint | Los que pide el cliente |
| Dashboard del paciente | 4 peticiones (`perfil`, `resumen`, `planes`, `peso`) | 1 petición |
| Errores | Códigos HTTP (400, 401, 404) | Lista `errors` con código |
| Documentación | Swagger / OpenAPI | Esquema introspectivo (Nitro) |
| Uso recomendado | Integraciones simples y cacheables | Apps móviles y tableros |

## Pruebas de las APIs

Las APIs se prueban automáticamente en cada push (GitHub Actions): REST con **RS-01 … RS-07** y GraphQL con **GQ-01 … GQ-05** en `tests/Calorix.UITests`.
