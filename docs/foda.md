# Análisis FODA — Calorix

> Plataforma web de seguimiento nutricional y vinculación paciente–nutriólogo, construida sobre **ASP.NET Core MVC 9**, **Entity Framework Core 9** y **SQLite**, con frontend en Razor + JavaScript vanilla (gráficas SVG propias).

El presente análisis FODA tiene como objetivo identificar la posición estratégica del sistema **Calorix** en su estado actual de desarrollo, considerando tanto los factores internos derivados de su arquitectura e implementación, como los factores externos del mercado de soluciones digitales para nutrición y bienestar.

---

## Matriz FODA

|  | **Positivo** ✅ | **Negativo** ⚠️ |
| --- | --- | --- |
| **Interno** 🏠 | **Fortalezas** | **Debilidades** |
| **Externo** 🌐 | **Oportunidades** | **Amenazas** |

---

## 🟢 Fortalezas (Internas — Positivas)

1. **Stack tecnológico moderno y mantenido**: .NET 9, EF Core 9, Razor Runtime Compilation y SQLite, todos con soporte vigente y comunidad activa.
2. **Patrones de diseño aplicados correctamente**: implementación de **Facade** (`CalorixFacade` con más de 50 métodos que centralizan la lógica de negocio), **Singleton** (`DatabaseConnection` thread-safe) y **Strategy** (3 fórmulas calóricas: Mifflin-St Jeor, Harris-Benedict y Katch-McArdle).
3. **Modelo de datos normalizado**: 11 entidades bien diseñadas con índices estratégicos definidos en `Models/Models.cs` y soportadas por migraciones EF Core versionadas.
4. **Frontend rico y autosuficiente**: gráficas SVG propias (barras y línea), modales de detalle día/semana, búsqueda AJAX en tiempo real para alimentos y recetas, sin dependencias pesadas de frameworks externos.
5. **Cobertura funcional completa del MVP**: las 7 historias de usuario macro están implementadas (gestión de alimentos/recetas, registro de comidas, seguimiento, gestión de pacientes, planes alimenticios, perfil con reevaluación automática, validación profesional).
6. **Arquitectura por capas clara**: separación entre Controllers → Facade → DbContext → Entities, lo que facilita el mantenimiento y la incorporación de nuevas funcionalidades.
7. **Roles bien definidos**: tres perfiles (Paciente, Nutriólogo, Administrador) con flujos diferenciados y redirección automática post-login según el rol.
8. **Documentación arquitectónica disponible**: `docs/diagramas/` contiene 8 diagramas (uno por caso de uso), modelo lógico de BD y diagrama de flujo de datos, complementados por `historias-usuario.md` con 34 historias detalladas.
9. **Cálculo nutricional sólido**: meta calórica diaria calculada automáticamente y recalculada al actualizar el perfil biométrico del paciente.
10. **Despliegue ligero**: SQLite + .NET self-contained permite distribución y arranque inmediato sin infraestructura adicional para el MVP.

---

## 🔵 Oportunidades (Externas — Positivas)

1. **Crecimiento sostenido del mercado de wellness y nutrición digital**, impulsado por la conciencia post-pandemia sobre salud preventiva.
2. **Auge de la telemedicina y atención remota**, que abre espacio a plataformas que conecten profesionales acreditados con pacientes a distancia.
3. **Integración futura con wearables** (Apple Health, Google Fit, Fitbit, Garmin) para automatizar la captura de actividad y peso.
4. **Posibilidad de monetización por suscripción**: planes premium para nutriólogos (más pacientes, recetas clínicas avanzadas, exportación de reportes).
5. **Expansión del catálogo con IA**: generación automática de recetas, reconocimiento de alimentos por foto, recomendaciones personalizadas.
6. **API pública para integraciones** con gimnasios, clínicas, EPS, aseguradoras y consultorios médicos.
7. **Versión móvil PWA o nativa** para captura inmediata de comidas (escaneo de código de barras, foto del plato).
8. **Demanda creciente de profesionales certificados** que buscan diferenciarse con plataformas digitales propias.

---

## 🟡 Debilidades (Internas — Negativas)

1. **Almacenamiento de contraseñas en texto plano**: no hay aplicación de hash criptográfico (PBKDF2, bcrypt, Argon2). Es la deuda técnica más urgente antes de cualquier despliegue productivo.
2. **Ausencia de pruebas automatizadas**: no existen tests unitarios ni de integración, por lo que cualquier refactor o cambio mayor implica riesgo de regresión.
3. **Operaciones de base de datos síncronas**: ausencia generalizada de `async/await` en accesos a EF Core, lo que limita la escalabilidad bajo carga concurrente.
4. **Validación servidor débil**: gran parte de la validación recae en el cliente (JavaScript), sin verificación robusta de `ModelState.IsValid` ni atributos `[DataAnnotations]` exhaustivos.
5. **Sin tokens anti-CSRF explícitos**: los formularios no aplican consistentemente `[ValidateAntiForgeryToken]`.
6. **Logging operativo no implementado**: `ILogger` está configurado por defecto pero no se utiliza en el código del Facade ni en los Controllers.
7. **Base de datos SQLite limita la escalabilidad horizontal**: adecuada para MVP, pero requiere migración a SQL Server / PostgreSQL para producción multinodo.
8. **Catálogo de alimentos de carga manual**: no hay integración con bases de datos nutricionales públicas (USDA, OpenFoodFacts), lo que limita la riqueza inicial.
9. **Sin internacionalización**: actualmente sólo en español; no hay archivos de recursos `.resx` ni soporte para múltiples culturas.
10. **Sin notificaciones push o por email**: las observaciones del nutriólogo y los nuevos planes sólo se descubren al abrir la app.

---

## 🔴 Amenazas (Externas — Negativas)

1. **Competidores establecidos** con bases de datos masivas y comunidades de usuarios consolidadas: MyFitnessPal, Cronometer, Nutritionix, FatSecret.
2. **Riesgo legal por manejo de datos personales sensibles**: la LFPDPPP (México), el GDPR (Europa) y normativas equivalentes exigen cifrado, consentimiento explícito y mecanismos de baja, actualmente no implementados.
3. **Disponibilidad de apps gratuitas con catálogos enormes**, que dificultan la captación de usuarios pacientes sin un valor diferencial claro.
4. **Posibles fraudes con cédulas profesionales falsas** durante el alta de nutriólogos, que comprometen la reputación de la plataforma.
5. **Cambios regulatorios sobre apps de salud**: nuevas obligaciones de auditoría, certificaciones HIPAA-equivalentes o registros sanitarios.
6. **Dependencia de conectividad**: usuarios en zonas con baja cobertura quedan excluidos hasta que exista una versión offline-first.
7. **Brechas de seguridad reportadas en proyectos similares** que generan desconfianza generalizada en el segmento.

---

## Estrategias cruzadas

### F + O — Estrategias ofensivas (usar fortalezas para capturar oportunidades)
- Aprovechar el **stack moderno y la arquitectura limpia** para construir rápidamente endpoints de **API pública** que permitan integraciones con wearables y clínicas.
- Capitalizar el **frontend rico y los gráficos propios** como diferenciador visual frente a competidores grandes pero menos personalizables.

### F + A — Estrategias defensivas (usar fortalezas para neutralizar amenazas)
- Apoyarse en el **modelo de datos normalizado** y los **patrones aplicados** para incorporar progresivamente cifrado en reposo y auditoría, blindando el sistema frente al riesgo regulatorio.
- Convertir el **flujo formal de validación de cédulas profesionales** en un sello de confianza diferenciador frente a apps gratuitas sin curaduría.

### D + O — Estrategias adaptativas (corregir debilidades para aprovechar oportunidades)
- Resolver primero la **deuda de seguridad (hash de contraseñas, CSRF, logging)** para habilitar la monetización y la integración B2B con clínicas.
- Implementar **async/await** y migrar SQLite → PostgreSQL/SQL Server para soportar el crecimiento esperado por la tendencia de wellness digital.

### D + A — Estrategias de supervivencia (mitigar debilidades frente a amenazas)
- Establecer una **política de pruebas automatizadas mínimas** sobre los flujos críticos (login, registro, cálculo calórico, vinculación) antes de exponer el sistema a un público amplio.
- Implementar **notificaciones por email o push** para reducir la brecha funcional con competidores y mejorar la retención.

---

## Conclusión

Calorix presenta una **base técnica sólida y una cobertura funcional completa del MVP**, lo que lo posiciona favorablemente para validar el modelo de negocio con usuarios reales. Las oportunidades de mercado son amplias, pero exigen resolver de forma prioritaria las debilidades de **seguridad** (hash de contraseñas, CSRF, cifrado en reposo) y **calidad** (cobertura de pruebas, logging) antes de un despliegue productivo o comercial.
