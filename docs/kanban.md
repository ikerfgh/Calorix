# Tablero Kanban — Calorix

> Tablero ágil del proyecto **Calorix** estructurado sobre **7 historias de usuario macro** que representan los entregables funcionales del producto. Cada historia se ubica en una columna según su estado de avance real, validado contra el código actual del repositorio.

## Leyenda

| Columna | Significado |
| --- | --- |
| 📥 **Backlog** | Idea registrada, sin priorizar para este sprint. |
| 🗒️ **Por hacer** | Priorizado y listo para iniciar desarrollo. |
| 🛠️ **En proceso** | Desarrollo activo en este momento. |
| 👀 **En revisión** | Implementado, pendiente de QA o ajustes menores. |
| ✅ **Hecho** | Implementado, validado y desplegable. |

| Prioridad | Significado |
| --- | --- |
| 🔴 Alta | Indispensable para el MVP. |
| 🟡 Media | Importante pero no bloqueante. |
| 🟢 Baja | Mejora o nice-to-have. |

---

## Vista del tablero

| 📥 Backlog | 🗒️ Por hacer | 🛠️ En proceso | 👀 En revisión | ✅ Hecho |
| --- | --- | --- | --- | --- |
| — | — | — | HU5 — Planes alimenticios | HU1 — Gestión alimentos y recetas |
| — | — | — | HU7 — Validación profesional | HU2 — Registro de comidas |
| — | — | — | — | HU3 — Seguimiento nutricional |
| — | — | — | — | HU4 — Gestión de pacientes |
| — | — | — | — | HU6 — Perfil y reevaluaciones |

---

## Detalle de cards

### ✅ HU1 — Gestión de alimentos y recetas

> **Como** Usuario **quiero** crear recetas propias combinando múltiples ingredientes **para que** pueda conocer el valor nutrimental total de los platillos y guardarlos en una biblioteca personal.

- **Estado:** ✅ Hecho
- **Prioridad:** 🔴 Alta
- **Implementación:** `Controllers/AlimentosController.cs`, entidades `Receta` e `IngredienteReceta` en `Models/Models.cs`, vistas en `Views/Alimentos/`.
- **Pendiente:** ninguno bloqueante.

---

### ✅ HU2 — Registro de comidas

> **Como** Usuario **quiero** registrar lo consumido durante el día, especificando momento del día y cantidad **para que** el sistema haga automáticamente los cálculos del total de comida actual.

- **Estado:** ✅ Hecho
- **Prioridad:** 🔴 Alta
- **Implementación:** `Controllers/SeguimientoController.cs`, entidad `RegistroComida` con campo `MomentoDelDia` (desayuno/almuerzo/merienda/cena), vista `Views/Seguimiento/Index.cshtml`.
- **Pendiente:** ninguno bloqueante.

---

### ✅ HU3 — Seguimiento nutricional

> **Como** Usuario **quiero** visualizar de forma gráfica mi progreso siempre que lo desee **para que** pueda analizar tendencias y ver si cumplo o no con las metas establecidas.

- **Estado:** ✅ Hecho
- **Prioridad:** 🔴 Alta
- **Implementación:** gráficas SVG (barras y línea) en `wwwroot/js/site.js`, vistas con rango semanal/mensual/anual y modales día-detalle, datos servidos por `SeguimientoController`.
- **Pendiente:** ninguno bloqueante.

---

### ✅ HU4 — Gestión de pacientes

> **Como** Nutriólogo **quiero** buscar y vincular pacientes a mi cuenta mediante su correo o nombre **para que** tenga acceso rápido y sencillo a los datos de mis pacientes.

- **Estado:** ✅ Hecho
- **Prioridad:** 🔴 Alta
- **Implementación:** `Controllers/NutriologoController.cs` con búsqueda por email/nombre, entidad `SolicitudVinculacion` y flujo aprobación/rechazo desde el lado del paciente.
- **Pendiente:** ninguno bloqueante.

---

### 👀 HU5 — Planes alimenticios

> **Como** Nutriólogo **quiero** diseñar y asignar planes de alimentación personalizados a mis pacientes **para que** reciban una notificación y puedan consultar de forma rápida y sencilla los nuevos planes o modificaciones.

- **Estado:** 👀 En revisión
- **Prioridad:** 🔴 Alta
- **Implementación:** `Controllers/PlanesController.cs`, `Controllers/MisPlanesController.cs`, entidades `PlanAlimenticio` y `ComidaPlan`, vistas `Views/Planes/` y `Views/MisPlanes/`.
- **Pendiente:**
  - 🔔 Implementar **notificación push o por correo** al paciente cuando se asigna o modifica un plan.
  - Indicador visual de plan "nuevo" en el panel del paciente.

---

### ✅ HU6 — Perfil y reevaluaciones

> **Como** Usuario **quiero** actualizar mi peso y actividad física desde la aplicación **para que** el sistema recalcule los requerimientos calóricos automáticamente.

- **Estado:** ✅ Hecho
- **Prioridad:** 🟡 Media
- **Implementación:** `Controllers/PerfilController.cs` recalcula la meta calórica con la fórmula Mifflin-St Jeor al guardar; `Strategy` permite alternar entre Mifflin, Harris-Benedict y Katch-McArdle.
- **Pendiente:** ninguno bloqueante.

---

### 👀 HU7 — Validación profesional

> **Como** Nutriólogo **quiero** adjuntar mi cédula y documentación a la aplicación **para que** el equipo valide mi identidad y pueda ejercer dentro de la plataforma de forma oficial y verificada.

- **Estado:** 👀 En revisión
- **Prioridad:** 🔴 Alta
- **Implementación:** `Controllers/AdminController.cs` con flujo aprobar/rechazar (motivo opcional), `Controllers/CuentaController.cs` para registro profesional, entidad `DocumentoNutriologo` con subida ≤ 5 MB en formatos PDF/JPG/PNG.
- **Pendiente:**
  - Flujo automatizado de **re-envío de documentos** tras un rechazo.
  - Notificación al nutriólogo cuando cambia el estado de su validación.

---

## Métricas del tablero

| Indicador | Valor |
| --- | --- |
| Total de historias | **7** |
| ✅ Hecho | 5 (71 %) |
| 👀 En revisión | 2 (29 %) |
| 🛠️ En proceso | 0 |
| 🗒️ Por hacer | 0 |
| 📥 Backlog | 0 |

**Estado general:** el MVP está completo. Quedan dos historias en revisión cuyo gap principal es el **subsistema de notificaciones** (push o email), que actualmente no existe en la plataforma.

## Próximos pasos sugeridos

1. Cerrar las dos historias en revisión implementando un servicio mínimo de notificaciones (email transaccional inicialmente).
2. Mover a **Backlog** las mejoras técnicas detectadas en el FODA (hash de contraseñas, async/await, tests automatizados, CSRF) para priorizarlas en el siguiente sprint.
3. Plantear nuevas historias para integraciones (wearables, API pública) y para la versión móvil PWA, alineadas con las oportunidades del FODA.
