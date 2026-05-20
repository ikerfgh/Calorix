# Historias de Usuario — Calorix

Documento de **historias de usuario** (enfoque ágil) para la plataforma de nutrición Calorix. Cada historia describe una funcionalidad desde la perspectiva del usuario final usando la plantilla estándar:

> **Como** `<Rol Usuario>` **Quiero** `<algún objetivo>` **Para que** `<motivo>`

## Roles del sistema

| Rol | Descripción |
| --- | --- |
| **Paciente** | Usuario final que registra su consumo nutricional y busca asesoría profesional. |
| **Nutriólogo** | Profesional acreditado que crea planes alimenticios y supervisa pacientes. |
| **Administrador** | Nutriólogo con permisos extra para validar credenciales de nuevos profesionales. |

## Leyenda de prioridad

| Etiqueta | Significado |
| --- | --- |
| 🔴 **Alta** | Funcionalidad core, indispensable para el MVP. |
| 🟡 **Media** | Funcionalidad importante pero no bloqueante. |
| 🟢 **Baja** | Mejora o nice-to-have. |

---

## 1. Historias del Paciente

### HU-P-01 — Registro de cuenta

> **Como** visitante **quiero** registrarme con mi email, peso, altura, edad y nivel de actividad **para que** el sistema calcule mi meta calórica diaria personalizada.

**Criterios de aceptación**
- El formulario valida email único y contraseña mínima de 6 caracteres.
- Solicita datos biométricos: peso (kg), altura (cm), edad, género y nivel de actividad.
- Tras registrarse, el sistema calcula automáticamente la meta calórica con la fórmula Mifflin-St Jeor.
- El usuario queda autenticado y es redirigido a su panel de seguimiento.

**Prioridad:** 🔴 Alta

---

### HU-P-02 — Inicio de sesión

> **Como** paciente **quiero** iniciar sesión con mi email y contraseña **para que** pueda acceder a mi panel de seguimiento nutricional.

**Criterios de aceptación**
- Login con email + contraseña.
- Mensaje claro ante credenciales inválidas.
- Redirección al dashboard según rol (Paciente → Seguimiento).
- Sesión persistente mediante cookies.

**Prioridad:** 🔴 Alta

---

### HU-P-03 — Búsqueda de alimentos

> **Como** paciente **quiero** buscar alimentos del catálogo por nombre **para que** conozca su información nutricional antes de registrarlos.

**Criterios de aceptación**
- Búsqueda en tiempo real por nombre parcial.
- Resultados muestran calorías, proteínas, carbohidratos y grasas por porción.
- Filtro por categoría (frutas, lácteos, proteínas, etc.).
- El catálogo está disponible en cualquier momento sin restricción de rol paciente.

**Prioridad:** 🔴 Alta

---

### HU-P-04 — Creación de recetas personalizadas

> **Como** paciente **quiero** crear recetas personalizadas combinando varios ingredientes **para que** pueda reutilizarlas fácilmente en mis registros diarios.

**Criterios de aceptación**
- Permite seleccionar varios alimentos del catálogo y asignar cantidad a cada uno.
- Calcula automáticamente totales de calorías y macros.
- La receta queda guardada en la biblioteca personal del usuario.
- Permite asignar nombre y descripción opcional.

**Prioridad:** 🟡 Media

---

### HU-P-05 — Registro de comidas diarias

> **Como** paciente **quiero** registrar lo que como indicando alimento, cantidad y momento del día (desayuno/almuerzo/cena/merienda) **para que** lleve un control real de mi consumo calórico.

**Criterios de aceptación**
- Selección del alimento desde el catálogo o desde mis recetas.
- Captura cantidad y unidad (gramos / porciones).
- Selección del momento del día (desayuno, almuerzo, merienda, cena).
- La fecha por defecto es hoy, pero permite registrar para fechas pasadas.
- El resumen diario se actualiza inmediatamente.

**Prioridad:** 🔴 Alta

---

### HU-P-06 — Eliminar registro de comida

> **Como** paciente **quiero** eliminar un registro de comida que cargué por error **para que** mis datos se mantengan limpios y precisos.

**Criterios de aceptación**
- Botón de eliminar disponible en cada registro del día.
- Confirmación antes de eliminar.
- Tras eliminar, los totales diarios se recalculan.

**Prioridad:** 🟡 Media

---

### HU-P-07 — Resumen nutricional diario

> **Como** paciente **quiero** ver mi resumen diario de calorías, proteínas, carbohidratos y grasas **para que** sepa qué tan cerca estoy de mi meta diaria.

**Criterios de aceptación**
- Muestra consumido vs. meta para cada macro.
- Indica porcentaje de cumplimiento de meta calórica.
- Se actualiza en tiempo real al registrar/eliminar comidas.
- Distingue visualmente sobreconsumo y subconsumo.

**Prioridad:** 🔴 Alta

---

### HU-P-08 — Gráficas de calorías por período

> **Como** paciente **quiero** ver gráficas de mis calorías consumidas por semana, mes o año **para que** identifique tendencias en mi alimentación.

**Criterios de aceptación**
- Permite cambiar el rango (semanal / mensual / anual).
- Muestra promedio del período como línea de referencia.
- Diferencia visualmente días por debajo, dentro o por encima de la meta.

**Prioridad:** 🟡 Media

---

### HU-P-09 — Gráficas de evolución de peso

> **Como** paciente **quiero** ver gráficas de la evolución de mi peso **para que** monitoree mi progreso a largo plazo.

**Criterios de aceptación**
- Gráfica de líneas con el peso registrado a lo largo del tiempo.
- Soporta los mismos rangos que la gráfica de calorías.
- Muestra tendencia (subida / bajada / estable).

**Prioridad:** 🟡 Media

---

### HU-P-10 — Detalle diario desde la gráfica

> **Como** paciente **quiero** hacer click en un día de la gráfica para ver el detalle de comidas de ese día **para que** entienda qué alimentos influyeron en un día específico.

**Criterios de aceptación**
- Click sobre un punto de la gráfica abre un detalle del día.
- Lista todas las comidas registradas con calorías y macros.
- Permite volver a la gráfica sin perder la selección.

**Prioridad:** 🟢 Baja

---

### HU-P-11 — Registro de peso

> **Como** paciente **quiero** registrar mi peso actual con una nota opcional **para que** construya un historial de seguimiento corporal.

**Criterios de aceptación**
- Captura peso (kg) y nota opcional.
- La fecha por defecto es hoy.
- Aparece inmediatamente en la gráfica de peso.

**Prioridad:** 🟡 Media

---

### HU-P-12 — Actualización de perfil

> **Como** paciente **quiero** actualizar mis datos biométricos (peso, altura, edad, actividad) **para que** la meta calórica se recalcule automáticamente.

**Criterios de aceptación**
- Formulario con los datos actuales prellenados.
- Al guardar, la meta calórica se recalcula con Mifflin-St Jeor.
- Muestra confirmación con la nueva meta.

**Prioridad:** 🟡 Media

---

### HU-P-13 — Solicitudes de nutriólogo

> **Como** paciente **quiero** recibir solicitudes de vinculación de nutriólogos y aprobarlas o rechazarlas **para que** controle quién accede a mis datos nutricionales.

**Criterios de aceptación**
- Notificación visible al recibir una solicitud nueva.
- Muestra nombre, cédula y especialidad del nutriólogo solicitante.
- Botones de aprobar y rechazar.
- Tras aprobar, el nutriólogo aparece como "Mi nutriólogo".

**Prioridad:** 🟡 Media

---

### HU-P-14 — Desvinculación de nutriólogo

> **Como** paciente **quiero** desvincularme de mi nutriólogo actual **para que** pueda cambiar de profesional cuando lo necesite.

**Criterios de aceptación**
- Botón de desvincular disponible en el panel.
- Confirmación antes de desvincular.
- El nutriólogo pierde acceso a los datos del paciente al instante.

**Prioridad:** 🟢 Baja

---

### HU-P-15 — Visualización de planes asignados

> **Como** paciente **quiero** ver los planes alimenticios que me asignó mi nutriólogo **para que** siga las indicaciones profesionales en mi día a día.

**Criterios de aceptación**
- Listado de planes activos asignados por el nutriólogo.
- Cada plan muestra sus comidas con alimentos/recetas y calorías objetivo.
- Plan activo destacado visualmente.

**Prioridad:** 🔴 Alta

---

### HU-P-16 — Lectura de observaciones del nutriólogo

> **Como** paciente **quiero** ver y marcar como leídas las observaciones que me envió mi nutriólogo **para que** esté al tanto de las recomendaciones profesionales.

**Criterios de aceptación**
- Listado de observaciones recibidas en orden cronológico inverso.
- Observaciones no leídas destacadas visualmente.
- Acción de marcar como leída.
- Indicador del plan asociado a cada observación (si aplica).

**Prioridad:** 🟡 Media

---

## 2. Historias del Nutriólogo

### HU-N-01 — Registro profesional

> **Como** visitante profesional **quiero** registrarme indicando cédula, especialidad y años de experiencia **para que** pueda solicitar acceso al sistema como nutriólogo certificado.

**Criterios de aceptación**
- Formulario diferenciado del registro de paciente.
- Captura cédula profesional, especialidad y años de experiencia.
- Email único validado contra base de datos.
- Tras enviarse, la cuenta queda en estado "Pendiente de validación".

**Prioridad:** 🔴 Alta

---

### HU-N-02 — Carga de documentos de credenciales

> **Como** nutriólogo en validación **quiero** subir mis documentos de credenciales (PDF/JPG/PNG hasta 5 MB) **para que** demuestre mi acreditación al administrador.

**Criterios de aceptación**
- Permite subir uno o varios archivos.
- Solo acepta extensiones PDF, JPG, PNG.
- Tamaño máximo por archivo: 5 MB.
- Los documentos quedan asociados a la cuenta en estado "Pendiente".

**Prioridad:** 🔴 Alta

---

### HU-N-03 — Estado de validación

> **Como** nutriólogo en validación **quiero** ver el estado de validación de mi cuenta (Pendiente/Aprobado/Rechazado) **para que** sepa cuándo puedo comenzar a atender pacientes.

**Criterios de aceptación**
- Indicador visible del estado actual.
- Si está rechazado, muestra motivo y permite reenviar documentos.
- Si está aprobado, habilita todas las funcionalidades profesionales.

**Prioridad:** 🟡 Media

---

### HU-N-04 — Búsqueda de pacientes potenciales

> **Como** nutriólogo **quiero** buscar usuarios por email o nombre **para que** encuentre pacientes que quiera asesorar.

**Criterios de aceptación**
- Búsqueda parcial por email o nombre.
- Resultados muestran nombre, email y si ya tiene nutriólogo asignado.
- No expone datos sensibles del usuario hasta la vinculación.

**Prioridad:** 🟡 Media

---

### HU-N-05 — Envío de solicitud de vinculación

> **Como** nutriólogo **quiero** enviar una solicitud de vinculación a un usuario **para que** comience a hacerle seguimiento profesional una vez aprobada.

**Criterios de aceptación**
- Botón "Solicitar vinculación" en el resultado de búsqueda.
- La solicitud queda en estado "Pendiente" hasta que el paciente decida.
- No se puede enviar más de una solicitud activa al mismo paciente.

**Prioridad:** 🔴 Alta

---

### HU-N-06 — Lista de pacientes vinculados

> **Como** nutriólogo **quiero** ver mi lista de pacientes vinculados **para que** acceda rápidamente a cada uno desde un solo panel.

**Criterios de aceptación**
- Tabla con nombre, edad, peso actual y meta calórica de cada paciente.
- Click en un paciente abre su panel de seguimiento.
- Búsqueda/filtro dentro de la lista.

**Prioridad:** 🔴 Alta

---

### HU-N-07 — Seguimiento detallado del paciente

> **Como** nutriólogo **quiero** ver el seguimiento detallado de un paciente (calorías, macros, peso) por día/semana/mes **para que** diagnostique su evolución con datos reales.

**Criterios de aceptación**
- Panel con gráficas de calorías y peso por rango.
- Visualización del cumplimiento de meta calórica.
- Comparativa de macros consumidos vs. recomendados.

**Prioridad:** 🔴 Alta

---

### HU-N-08 — Análisis de un día específico

> **Como** nutriólogo **quiero** hacer click en un día de la gráfica del paciente y ver qué comió **para que** analice episodios puntuales (atracones, ayunos, etc.).

**Criterios de aceptación**
- Click sobre la gráfica abre detalle de comidas del día.
- Lista cada alimento con cantidad, momento y macros.
- Permite volver al panel general sin perder contexto.

**Prioridad:** 🟡 Media

---

### HU-N-09 — Envío de observaciones

> **Como** nutriólogo **quiero** escribir observaciones dirigidas a mi paciente **para que** le dé feedback profesional sin necesidad de medios externos.

**Criterios de aceptación**
- Editor de texto simple asociado al paciente o a un plan específico.
- Las observaciones quedan registradas con fecha.
- El paciente las ve en su panel y puede marcarlas como leídas.

**Prioridad:** 🟡 Media

---

### HU-N-10 — Desvinculación de paciente

> **Como** nutriólogo **quiero** desvincular a un paciente **para que** cierre la relación profesional cuando corresponda.

**Criterios de aceptación**
- Botón de desvincular en el panel del paciente.
- Confirmación antes de desvincular.
- Pierdo el acceso a sus datos al instante.

**Prioridad:** 🟢 Baja

---

### HU-N-11 — Creación de planes alimenticios

> **Como** nutriólogo **quiero** crear un plan alimenticio con varias comidas, alimentos y recetas **para que** estructure la dieta del paciente de manera ordenada.

**Criterios de aceptación**
- Formulario para nombrar el plan y agregar comidas (desayuno, almuerzo, cena, merienda).
- Cada comida acepta varios alimentos y/o recetas con cantidades.
- El sistema calcula calorías y macros totales por comida y por día.
- El plan queda asociado al paciente seleccionado.

**Prioridad:** 🔴 Alta

---

### HU-N-12 — Asignación masiva de planes

> **Como** nutriólogo **quiero** asignar el mismo plan a múltiples pacientes a la vez **para que** ahorre tiempo cuando varios pacientes siguen el mismo enfoque.

**Criterios de aceptación**
- Selector múltiple de pacientes vinculados.
- Un solo plan se replica para cada paciente seleccionado.
- Cada paciente puede luego modificar / desactivar su copia.

**Prioridad:** 🟢 Baja

---

### HU-N-13 — Edición / eliminación de planes

> **Como** nutriólogo **quiero** editar o eliminar un plan existente **para que** lo adapte a la evolución del paciente.

**Criterios de aceptación**
- Acceso a todos los planes creados por el nutriólogo.
- Edición de cualquier campo del plan o sus comidas.
- Eliminación con confirmación.

**Prioridad:** 🟡 Media

---

### HU-N-14 — Recetas clínicas reutilizables

> **Como** nutriólogo **quiero** crear recetas clínicas reutilizables **para que** las incluya como bloques en distintos planes.

**Criterios de aceptación**
- Las recetas creadas por el nutriólogo quedan disponibles para usarlas en cualquier plan.
- Calculan automáticamente sus macros totales.
- Se distinguen visualmente de las recetas personales del paciente.

**Prioridad:** 🟡 Media

---

## 3. Historias del Administrador

### HU-A-01 — Lista de nutriólogos pendientes

> **Como** administrador **quiero** ver la lista de nutriólogos pendientes de validación **para que** priorice las revisiones de acreditación.

**Criterios de aceptación**
- Tabla con nombre, cédula, especialidad y fecha de registro.
- Ordenable por fecha (más antiguos primero por defecto).
- Indica cantidad de documentos subidos por cada nutriólogo.

**Prioridad:** 🔴 Alta

---

### HU-A-02 — Descarga de documentos

> **Como** administrador **quiero** descargar los documentos de credenciales subidos por cada nutriólogo **para que** verifique su autenticidad antes de aprobar.

**Criterios de aceptación**
- Listado de documentos asociados al nutriólogo.
- Botón de descarga por archivo (PDF/JPG/PNG).
- Conserva el nombre y formato original del archivo.

**Prioridad:** 🟡 Media

---

### HU-A-03 — Aprobación de nutriólogo

> **Como** administrador **quiero** aprobar el registro de un nutriólogo **para que** le habilite el acceso completo a la plataforma.

**Criterios de aceptación**
- Botón "Aprobar" con confirmación.
- Cambia el estado del nutriólogo a "Aprobado".
- El nutriólogo puede iniciar sesión y usar todas las funciones profesionales.

**Prioridad:** 🔴 Alta

---

### HU-A-04 — Rechazo de nutriólogo

> **Como** administrador **quiero** rechazar el registro de un nutriólogo con documentación inválida **para que** mantenga la calidad profesional del sistema.

**Criterios de aceptación**
- Botón "Rechazar" con confirmación.
- Permite registrar un motivo de rechazo.
- El nutriólogo ve el estado "Rechazado" y el motivo en su panel.

**Prioridad:** 🟡 Media

---

## 4. Trazabilidad Historia ↔ Caso de Uso

| Caso de uso | Historias relacionadas |
| --- | --- |
| CU1 — Gestión de Alimentos | HU-P-03, HU-P-04 |
| CU2 — Registro de Comidas | HU-P-05, HU-P-06, HU-P-07 |
| CU3 — Seguimiento Nutricional | HU-P-08, HU-P-09, HU-P-10, HU-P-11, HU-P-13, HU-P-14 |
| CU4 — Gestión de Pacientes | HU-N-04, HU-N-05, HU-N-06, HU-N-10 |
| CU5 — Seguimiento Profesional | HU-N-07, HU-N-08, HU-N-09, HU-P-16 |
| CU6 — Planes Alimenticios | HU-N-11, HU-N-12, HU-N-13, HU-N-14, HU-P-15 |
| CU7 — Perfil Personal | HU-P-01, HU-P-02, HU-P-12 |
| CU8 — Registro Profesional | HU-N-01, HU-N-02, HU-N-03, HU-A-01, HU-A-02, HU-A-03, HU-A-04 |

**Total: 34 historias de usuario** (16 Paciente · 14 Nutriólogo · 4 Administrador).
