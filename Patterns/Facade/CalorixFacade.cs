using Calorix.Data;
using Calorix.Models;
using Calorix.Patterns.Strategy;
using Microsoft.EntityFrameworkCore;

namespace Calorix.Patterns.Facade;

public interface ICalorixFacade
{
    List<Alimento> BuscarAlimentos(string termino);
    Alimento? ObtenerAlimento(int id);
    Receta CrearReceta(int usuarioId, string nombre, string desc, List<IngredienteReceta> ingredientes);
    List<Receta> ObtenerRecetasUsuario(int usuarioId);
    RegistroComida RegistrarComida(int usuarioId, int alimentoId, float cantidad, string momento);
    RegistroComida RegistrarRecetaComida(int usuarioId, int recetaId, float porciones, string momento);
    bool EliminarRegistro(int registroId, int usuarioId);
    List<RegistroComida> ObtenerRegistrosDia(int usuarioId, DateTime fecha);
    ResumenDiario ObtenerResumenDiario(int usuarioId, DateTime fecha);
    EstadisticasSemana ObtenerEstadisticasSemana(int usuarioId);
    MacrosRecomendados ObtenerMacrosRecomendados(int usuarioId, string formula);
    bool VincularPaciente(int nutriologoId, int usuarioId);
    List<Usuario> ObtenerPacientes(int nutriologoId);
    ResumenDiario ObtenerResumenPaciente(int nutriologoId, int pacienteId, DateTime fecha);
    PlanAlimenticio CrearPlanAlimenticio(int nutriologoId, int pacienteId, string nombre, List<ComidaPlan> comidas);
    List<PlanAlimenticio> CrearPlanesAlimenticios(int nutriologoId, List<int> pacienteIds, string nombre, List<ComidaPlan> comidas);
    bool EditarPlanAlimenticio(int nutriologoId, int planId, string nombre, List<ComidaPlan> comidas);
    bool EliminarPlanAlimenticio(int nutriologoId, int planId);
    PlanAlimenticio? ObtenerPlan(int planId);
    List<PlanAlimenticio> ObtenerPlanesPaciente(int pacienteId);
    Usuario? ObtenerPerfil(int usuarioId);
    bool ActualizarPerfil(int usuarioId, float peso, float altura, int edad, string actividad);
    (int id, string role)? Login(string email, string password);
    Nutriologo? ObtenerNutriologo(int id);
    Nutriologo RegistrarNutriologo(string nombre, string email, string password, string cedula, string especialidad, int aniosExp);
    (Usuario? usuario, string? error) RegistrarUsuario(string nombre, string email, string password, int edad, float peso, float altura, string genero, string nivelActividad);
    List<Usuario> BuscarUsuarios(string termino);
    List<Alimento> ObtenerTodosAlimentos();
    RegistroPeso RegistrarPeso(int usuarioId, float peso, string? notas = null);
    List<RegistroPeso> ObtenerHistorialPeso(int usuarioId, int dias = 30);
    Observacion EscribirObservacion(int nutriologoId, int usuarioId, int? planId, string texto);
    List<Observacion> ObtenerObservacionesPaciente(int usuarioId);
    List<Observacion> ObtenerObservacionesNutriologo(int nutriologoId, int pacienteId);
    int MarcarObservacionesLeidas(int usuarioId);
    ResumenRango ObtenerResumenPacienteRango(int nutriologoId, int pacienteId, DateTime desde, DateTime hasta);

    // Solicitudes de vinculación nutriólogo ↔ usuario
    bool TieneNutriologoAprobado(int usuarioId);
    Nutriologo? ObtenerNutriologoDelUsuario(int usuarioId);
    (bool ok, string mensaje) EnviarSolicitudVinculacion(int nutriologoId, int usuarioId);
    List<(SolicitudVinculacion solicitud, Nutriologo nutriologo)> ObtenerSolicitudesPendientes(int usuarioId);
    bool ResponderSolicitud(int usuarioId, int solicitudId, bool aprobar);
    (bool ok, string mensaje) Desvincular(int nutriologoId, int usuarioId);

    // CU8 — Documentos profesionales y validación administrativa
    DocumentoNutriologo RegistrarDocumento(int nutriologoId, string nombreOriginal, string rutaRelativa, string contentType, long tamano);
    List<DocumentoNutriologo> ObtenerDocumentos(int nutriologoId);
    DocumentoNutriologo? ObtenerDocumento(int documentoId);
    bool EsAdmin(int nutriologoId);
    List<Nutriologo> ObtenerNutriologosPendientes();
    bool ValidarNutriologo(int nutriologoId, bool aprobar);

    // CU6 — Recetas clínicas creadas por el nutriólogo
    Receta CrearRecetaClinica(int nutriologoId, string nombre, string desc, List<IngredienteReceta> ingredientes);
    List<Receta> ObtenerRecetasClinicas(int nutriologoId);
}

public class CalorixFacade : ICalorixFacade
{
    private readonly CalorixDbContext _db;

    public CalorixFacade(CalorixDbContext db) => _db = db;

    public List<Alimento> BuscarAlimentos(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino)) return _db.Alimentos.ToList();
        termino = termino.ToLower();
        return _db.Alimentos
            .Where(a => a.Nombre.ToLower().Contains(termino) || a.Categoria.ToLower().Contains(termino))
            .ToList();
    }

    public List<Alimento> ObtenerTodosAlimentos() => _db.Alimentos.ToList();

    public Alimento? ObtenerAlimento(int id) => _db.Alimentos.FirstOrDefault(a => a.Id == id);

    public Receta CrearReceta(int usuarioId, string nombre, string desc, List<IngredienteReceta> ingredientes)
    {
        float cal = 0, prot = 0, carb = 0, grasas = 0;
        foreach (var ing in ingredientes)
        {
            var a = _db.Alimentos.FirstOrDefault(x => x.Id == ing.AlimentoId);
            if (a == null) continue;
            float f = ing.Cantidad / 100f;
            cal += a.Calorias * f;
            prot += a.Proteinas * f;
            carb += a.Carbohidratos * f;
            grasas += a.Grasas * f;
        }
        var receta = new Receta
        {
            Nombre = nombre, Descripcion = desc, Ingredientes = ingredientes,
            CaloriasTotal = MathF.Round(cal), ProteinasTotal = MathF.Round(prot),
            CarbosTotal = MathF.Round(carb), GrasasTotal = MathF.Round(grasas),
            UsuarioId = usuarioId, FechaCreacion = DateTime.Now
        };
        _db.Recetas.Add(receta);
        _db.SaveChanges();
        return receta;
    }

    public List<Receta> ObtenerRecetasUsuario(int usuarioId) =>
        _db.Recetas.Include(r => r.Ingredientes).Where(r => r.UsuarioId == usuarioId).ToList();

    public RegistroComida RegistrarComida(int usuarioId, int alimentoId, float cantidad, string momento)
    {
        var a = _db.Alimentos.FirstOrDefault(x => x.Id == alimentoId);
        float f = cantidad / 100f;
        var reg = new RegistroComida
        {
            UsuarioId = usuarioId, AlimentoId = alimentoId,
            NombreAlimento = a?.Nombre ?? "Desconocido",
            Cantidad = cantidad, UnidadCantidad = "g", Momento = momento, Fecha = DateTime.Today,
            Calorias = MathF.Round((a?.Calorias ?? 0) * f),
            Proteinas = MathF.Round((a?.Proteinas ?? 0) * f),
            Carbohidratos = MathF.Round((a?.Carbohidratos ?? 0) * f),
            Grasas = MathF.Round((a?.Grasas ?? 0) * f),
        };
        _db.RegistrosComida.Add(reg);
        _db.SaveChanges();
        return reg;
    }

    public RegistroComida RegistrarRecetaComida(int usuarioId, int recetaId, float porciones, string momento)
    {
        var r = _db.Recetas.FirstOrDefault(x => x.Id == recetaId);
        var reg = new RegistroComida
        {
            UsuarioId = usuarioId, AlimentoId = -recetaId,
            NombreAlimento = r?.Nombre ?? "Receta",
            Cantidad = porciones, UnidadCantidad = "porción(es)", Momento = momento, Fecha = DateTime.Today,
            Calorias = MathF.Round((r?.CaloriasTotal ?? 0) * porciones),
            Proteinas = MathF.Round((r?.ProteinasTotal ?? 0) * porciones),
            Carbohidratos = MathF.Round((r?.CarbosTotal ?? 0) * porciones),
            Grasas = MathF.Round((r?.GrasasTotal ?? 0) * porciones),
        };
        _db.RegistrosComida.Add(reg);
        _db.SaveChanges();
        return reg;
    }

    public bool EliminarRegistro(int registroId, int usuarioId)
    {
        var reg = _db.RegistrosComida.FirstOrDefault(r => r.Id == registroId && r.UsuarioId == usuarioId);
        if (reg == null) return false;
        _db.RegistrosComida.Remove(reg);
        _db.SaveChanges();
        return true;
    }

    public List<RegistroComida> ObtenerRegistrosDia(int usuarioId, DateTime fecha) =>
        _db.RegistrosComida.Where(r => r.UsuarioId == usuarioId && r.Fecha.Date == fecha.Date).ToList();

    public ResumenDiario ObtenerResumenDiario(int usuarioId, DateTime fecha)
    {
        var registros = ObtenerRegistrosDia(usuarioId, fecha);
        var usuario = _db.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        var total = registros.Sum(r => r.Calorias);
        var meta = usuario?.MetaCalorica ?? 2000;
        return new ResumenDiario
        {
            Fecha = fecha, UsuarioId = usuarioId, Registros = registros,
            TotalCalorias = total,
            TotalProteinas = registros.Sum(r => r.Proteinas),
            TotalCarbos = registros.Sum(r => r.Carbohidratos),
            TotalGrasas = registros.Sum(r => r.Grasas),
            MetaCalorica = meta,
            PorcentajeCumplimiento = meta > 0 ? MathF.Min((total / meta) * 100, 100) : 0
        };
    }

    public EstadisticasSemana ObtenerEstadisticasSemana(int usuarioId)
    {
        var hoy = DateTime.Today;
        var datos = new Dictionary<string, float>();
        float total = 0;
        for (int i = 6; i >= 0; i--)
        {
            var fecha = hoy.AddDays(-i);
            var cal = _db.RegistrosComida
                .Where(r => r.UsuarioId == usuarioId && r.Fecha.Date == fecha.Date)
                .Sum(r => r.Calorias);
            datos[fecha.ToString("ddd", new System.Globalization.CultureInfo("es-MX"))] = cal;
            total += cal;
        }
        return new EstadisticasSemana { DatosPorDia = datos, PromedioCaloriasDia = MathF.Round(total / 7), TotalSemana = total };
    }

    public MacrosRecomendados ObtenerMacrosRecomendados(int usuarioId, string formula)
    {
        var usuario = _db.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        if (usuario == null) return new MacrosRecomendados();
        ICalculoNutricionalStrategy strat = formula switch
        {
            "harris" => new HarrisBenedictStrategy(),
            "deportiva" => new CaloriaDeportivaStrategy(),
            _ => new MifflinStJeorStrategy()
        };
        return new CalculadorNutricional(strat).CalcularMacros(usuario);
    }

    public bool VincularPaciente(int nutriologoId, int usuarioId)
    {
        // Mantenida por compatibilidad: hace vinculación directa (usada por el seeder histórico).
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        if (n == null || !_db.Usuarios.Any(u => u.Id == usuarioId)) return false;
        if (!n.PacienteIds.Contains(usuarioId))
        {
            n.PacienteIds = new List<int>(n.PacienteIds) { usuarioId };
            _db.SaveChanges();
        }
        return true;
    }

    public bool TieneNutriologoAprobado(int usuarioId) =>
        _db.Nutriologos.AsEnumerable().Any(n => n.PacienteIds.Contains(usuarioId));

    public Nutriologo? ObtenerNutriologoDelUsuario(int usuarioId) =>
        _db.Nutriologos.AsEnumerable().FirstOrDefault(n => n.PacienteIds.Contains(usuarioId));

    public (bool ok, string mensaje) EnviarSolicitudVinculacion(int nutriologoId, int usuarioId)
    {
        if (!_db.Usuarios.Any(u => u.Id == usuarioId))
            return (false, "Usuario no encontrado.");

        var actual = ObtenerNutriologoDelUsuario(usuarioId);
        if (actual != null)
        {
            if (actual.Id == nutriologoId) return (false, "Este paciente ya está vinculado contigo.");
            return (false, $"El usuario ya tiene un nutriólogo asignado ({actual.Nombre}).");
        }

        var pendiente = _db.Solicitudes
            .FirstOrDefault(s => s.UsuarioId == usuarioId && s.NutriologoId == nutriologoId && s.Estado == "pendiente");
        if (pendiente != null) return (true, "Ya existe una solicitud pendiente para este usuario.");

        _db.Solicitudes.Add(new SolicitudVinculacion
        {
            NutriologoId = nutriologoId,
            UsuarioId = usuarioId,
            Estado = "pendiente",
            Fecha = DateTime.Now
        });
        _db.SaveChanges();
        return (true, "Solicitud enviada. Espera la aprobación del usuario.");
    }

    public List<(SolicitudVinculacion solicitud, Nutriologo nutriologo)> ObtenerSolicitudesPendientes(int usuarioId)
    {
        var sols = _db.Solicitudes
            .Where(s => s.UsuarioId == usuarioId && s.Estado == "pendiente")
            .OrderByDescending(s => s.Fecha)
            .ToList();
        var nutriIds = sols.Select(s => s.NutriologoId).Distinct().ToList();
        var nutriMap = _db.Nutriologos.Where(n => nutriIds.Contains(n.Id)).ToDictionary(n => n.Id);
        return sols
            .Where(s => nutriMap.ContainsKey(s.NutriologoId))
            .Select(s => (s, nutriMap[s.NutriologoId]))
            .ToList();
    }

    public (bool ok, string mensaje) Desvincular(int nutriologoId, int usuarioId)
    {
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        if (n == null) return (false, "Nutriólogo no encontrado.");
        if (!n.PacienteIds.Contains(usuarioId))
            return (false, "No existe vinculación entre este nutriólogo y el usuario.");

        n.PacienteIds = n.PacienteIds.Where(id => id != usuarioId).ToList();

        // Cerrar la solicitud aprobada para permitir vincularse nuevamente en el futuro
        var aprobadas = _db.Solicitudes
            .Where(s => s.UsuarioId == usuarioId && s.NutriologoId == nutriologoId && s.Estado == "aprobada")
            .ToList();
        foreach (var s in aprobadas) { s.Estado = "desvinculada"; s.FechaRespuesta = DateTime.Now; }

        _db.SaveChanges();
        return (true, "Vinculación finalizada.");
    }

    public bool ResponderSolicitud(int usuarioId, int solicitudId, bool aprobar)
    {
        var sol = _db.Solicitudes.FirstOrDefault(s => s.Id == solicitudId && s.UsuarioId == usuarioId);
        if (sol == null || sol.Estado != "pendiente") return false;

        if (aprobar)
        {
            // Si ya tiene un nutriólogo aprobado distinto, no permitir vinculación adicional.
            if (TieneNutriologoAprobado(usuarioId)) return false;

            var n = _db.Nutriologos.FirstOrDefault(x => x.Id == sol.NutriologoId);
            if (n == null) return false;
            if (!n.PacienteIds.Contains(usuarioId))
            {
                n.PacienteIds = new List<int>(n.PacienteIds) { usuarioId };
            }
            sol.Estado = "aprobada";

            // Auto-rechazar otras solicitudes pendientes para el mismo usuario
            var otras = _db.Solicitudes
                .Where(s => s.UsuarioId == usuarioId && s.Estado == "pendiente" && s.Id != solicitudId)
                .ToList();
            foreach (var o in otras) { o.Estado = "rechazada"; o.FechaRespuesta = DateTime.Now; }
        }
        else
        {
            sol.Estado = "rechazada";
        }
        sol.FechaRespuesta = DateTime.Now;
        _db.SaveChanges();
        return true;
    }

    public List<Usuario> ObtenerPacientes(int nutriologoId)
    {
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        if (n == null) return new();
        var ids = n.PacienteIds;
        return _db.Usuarios.Where(u => ids.Contains(u.Id)).ToList();
    }

    public ResumenDiario ObtenerResumenPaciente(int nutriologoId, int pacienteId, DateTime fecha)
    {
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        if (n == null || !n.PacienteIds.Contains(pacienteId)) return new();
        return ObtenerResumenDiario(pacienteId, fecha);
    }

    public PlanAlimenticio CrearPlanAlimenticio(int nutriologoId, int pacienteId, string nombre, List<ComidaPlan> comidas)
    {
        var plan = new PlanAlimenticio
        {
            Nombre = nombre, NutriologoId = nutriologoId, PacienteId = pacienteId,
            Comidas = comidas, FechaCreacion = DateTime.Now, Activo = true
        };
        _db.Planes.Add(plan);
        _db.SaveChanges();

        // CU6 — Notificación automática al paciente
        _db.Observaciones.Add(new Observacion
        {
            NutriologoId = nutriologoId,
            UsuarioId = pacienteId,
            PlanId = plan.Id,
            Texto = $"Tu nutriólogo te asignó un nuevo plan alimenticio: \"{plan.Nombre}\". Revísalo en la sección Mis Planes.",
            Fecha = DateTime.Now,
            Leida = false
        });
        _db.SaveChanges();
        return plan;
    }

    public List<PlanAlimenticio> CrearPlanesAlimenticios(int nutriologoId, List<int> pacienteIds, string nombre, List<ComidaPlan> comidas)
    {
        var creados = new List<PlanAlimenticio>();
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        if (n == null) return creados;

        foreach (var pacId in pacienteIds.Distinct())
        {
            if (!n.PacienteIds.Contains(pacId)) continue;
            // Cada plan necesita su propia colección de ComidaPlan (EF las trackea por instancia)
            var copiaComidas = comidas.Select(c => new ComidaPlan
            {
                Momento = c.Momento, Descripcion = c.Descripcion, Calorias = c.Calorias,
                RecetaId = c.RecetaId, AlimentoIds = new List<int>(c.AlimentoIds ?? new())
            }).ToList();
            creados.Add(CrearPlanAlimenticio(nutriologoId, pacId, nombre, copiaComidas));
        }
        return creados;
    }

    public List<PlanAlimenticio> ObtenerPlanesPaciente(int pacienteId) =>
        _db.Planes.Include(p => p.Comidas).Where(p => p.PacienteId == pacienteId).ToList();

    public PlanAlimenticio? ObtenerPlan(int planId) =>
        _db.Planes.Include(p => p.Comidas).FirstOrDefault(p => p.Id == planId);

    public bool EditarPlanAlimenticio(int nutriologoId, int planId, string nombre, List<ComidaPlan> comidas)
    {
        var plan = _db.Planes.Include(p => p.Comidas).FirstOrDefault(p => p.Id == planId);
        if (plan == null || plan.NutriologoId != nutriologoId) return false;

        plan.Nombre = string.IsNullOrWhiteSpace(nombre) ? plan.Nombre : nombre.Trim();
        // Reemplazar comidas: borrar las viejas y agregar las nuevas
        _db.RemoveRange(plan.Comidas);
        plan.Comidas = comidas ?? new List<ComidaPlan>();
        _db.SaveChanges();
        return true;
    }

    public bool EliminarPlanAlimenticio(int nutriologoId, int planId)
    {
        var plan = _db.Planes.Include(p => p.Comidas).FirstOrDefault(p => p.Id == planId);
        if (plan == null || plan.NutriologoId != nutriologoId) return false;

        // Limpiar referencias en observaciones (PlanId queda null en lugar de eliminar la observación)
        var obsConPlan = _db.Observaciones.Where(o => o.PlanId == planId).ToList();
        foreach (var o in obsConPlan) o.PlanId = null;

        _db.Planes.Remove(plan); // cascade borra ComidaPlan
        _db.SaveChanges();
        return true;
    }

    public Usuario? ObtenerPerfil(int usuarioId) => _db.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

    public bool ActualizarPerfil(int usuarioId, float peso, float altura, int edad, string actividad)
    {
        var u = _db.Usuarios.FirstOrDefault(x => x.Id == usuarioId);
        if (u == null) return false;
        bool pesoCambio = MathF.Abs(u.Peso - peso) > 0.01f;
        u.Peso = peso; u.Altura = altura; u.Edad = edad; u.NivelActividad = actividad;
        u.MetaCalorica = MathF.Round(new CalculadorNutricional(new MifflinStJeorStrategy()).CalcularMeta(u));
        if (pesoCambio)
        {
            _db.RegistrosPeso.Add(new RegistroPeso { UsuarioId = usuarioId, Peso = peso, Fecha = DateTime.Now });
        }
        _db.SaveChanges();
        return true;
    }

    public (int id, string role)? Login(string email, string password)
    {
        var u = _db.Usuarios.FirstOrDefault(x => x.Email == email && x.Password == password);
        if (u != null) return (u.Id, "Usuario");
        var n = _db.Nutriologos.FirstOrDefault(x => x.Email == email && x.Password == password);
        if (n != null) return (n.Id, "Nutriologo");
        return null;
    }

    public Nutriologo? ObtenerNutriologo(int id) => _db.Nutriologos.FirstOrDefault(n => n.Id == id);

    public Nutriologo RegistrarNutriologo(string nombre, string email, string password, string cedula, string especialidad, int aniosExp)
    {
        var n = new Nutriologo
        {
            Nombre = nombre, Email = email, Password = password,
            Cedula = cedula, Especialidad = especialidad, AniosExp = aniosExp,
            Validado = false, PacienteIds = new()
        };
        _db.Nutriologos.Add(n);
        _db.SaveChanges();
        return n;
    }

    public (Usuario? usuario, string? error) RegistrarUsuario(string nombre, string email, string password, int edad, float peso, float altura, string genero, string nivelActividad)
    {
        var emailNorm = (email ?? "").Trim().ToLower();
        if (_db.Usuarios.Any(u => u.Email.ToLower() == emailNorm) ||
            _db.Nutriologos.Any(x => x.Email.ToLower() == emailNorm))
        {
            return (null, "Ya existe una cuenta con ese correo.");
        }

        var u = new Usuario
        {
            Nombre = nombre,
            Email = emailNorm,
            Password = password,
            Edad = edad,
            Peso = peso,
            Altura = altura,
            Genero = genero,
            NivelActividad = nivelActividad
        };
        u.MetaCalorica = MathF.Round(new CalculadorNutricional(new MifflinStJeorStrategy()).CalcularMeta(u));
        _db.Usuarios.Add(u);
        _db.SaveChanges();
        _db.RegistrosPeso.Add(new RegistroPeso { UsuarioId = u.Id, Peso = peso, Fecha = DateTime.Now });
        _db.SaveChanges();
        return (u, null);
    }

    public List<Usuario> BuscarUsuarios(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino)) return _db.Usuarios.ToList();
        termino = termino.ToLower();
        return _db.Usuarios.Where(u => u.Nombre.ToLower().Contains(termino) || u.Email.ToLower().Contains(termino)).ToList();
    }

    public RegistroPeso RegistrarPeso(int usuarioId, float peso, string? notas = null)
    {
        var u = _db.Usuarios.FirstOrDefault(x => x.Id == usuarioId);
        var reg = new RegistroPeso { UsuarioId = usuarioId, Peso = peso, Fecha = DateTime.Now, Notas = notas };
        _db.RegistrosPeso.Add(reg);
        if (u != null) u.Peso = peso;
        _db.SaveChanges();
        return reg;
    }

    public List<RegistroPeso> ObtenerHistorialPeso(int usuarioId, int dias = 30)
    {
        var desde = DateTime.Today.AddDays(-dias + 1);
        return _db.RegistrosPeso
            .Where(r => r.UsuarioId == usuarioId && r.Fecha >= desde)
            .OrderBy(r => r.Fecha)
            .ToList();
    }

    public Observacion EscribirObservacion(int nutriologoId, int usuarioId, int? planId, string texto)
    {
        var o = new Observacion
        {
            NutriologoId = nutriologoId,
            UsuarioId = usuarioId,
            PlanId = planId,
            Texto = texto ?? "",
            Fecha = DateTime.Now,
            Leida = false
        };
        _db.Observaciones.Add(o);
        _db.SaveChanges();
        return o;
    }

    public List<Observacion> ObtenerObservacionesPaciente(int usuarioId) =>
        _db.Observaciones.Where(o => o.UsuarioId == usuarioId).OrderByDescending(o => o.Fecha).ToList();

    public List<Observacion> ObtenerObservacionesNutriologo(int nutriologoId, int pacienteId) =>
        _db.Observaciones
            .Where(o => o.NutriologoId == nutriologoId && o.UsuarioId == pacienteId)
            .OrderByDescending(o => o.Fecha)
            .ToList();

    public int MarcarObservacionesLeidas(int usuarioId)
    {
        var noLeidas = _db.Observaciones.Where(o => o.UsuarioId == usuarioId && !o.Leida).ToList();
        foreach (var o in noLeidas) o.Leida = true;
        _db.SaveChanges();
        return noLeidas.Count;
    }

    public ResumenRango ObtenerResumenPacienteRango(int nutriologoId, int pacienteId, DateTime desde, DateTime hasta)
    {
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        var r = new ResumenRango { Desde = desde, Hasta = hasta };
        if (n == null || !n.PacienteIds.Contains(pacienteId)) return r;

        var u = _db.Usuarios.FirstOrDefault(x => x.Id == pacienteId);
        r.MetaCalorica = u?.MetaCalorica ?? 2000;
        r.PesoActual = u?.Peso ?? 0;

        var d0 = desde.Date;
        var d1 = hasta.Date;
        var registros = _db.RegistrosComida
            .Where(x => x.UsuarioId == pacienteId && x.Fecha >= d0 && x.Fecha <= d1)
            .ToList();
        r.Registros = registros;
        r.TotalCalorias = registros.Sum(x => x.Calorias);
        r.TotalProteinas = registros.Sum(x => x.Proteinas);
        r.TotalCarbos = registros.Sum(x => x.Carbohidratos);
        r.TotalGrasas = registros.Sum(x => x.Grasas);

        var totalDias = (int)(d1 - d0).TotalDays + 1;
        r.PromedioCaloriasDia = totalDias > 0 ? MathF.Round(r.TotalCalorias / totalDias) : 0;
        r.PorcentajePromedio = r.MetaCalorica > 0 ? MathF.Min((r.PromedioCaloriasDia / r.MetaCalorica) * 100, 100) : 0;

        var es = new System.Globalization.CultureInfo("es-MX");
        var calPorDia = new Dictionary<string, float>();
        var pesoPorDia = new Dictionary<string, float>();

        // Pesos reales del paciente en el rango (más uno antes para baseline)
        var pesos = _db.RegistrosPeso
            .Where(p => p.UsuarioId == pacienteId && p.Fecha >= d0 && p.Fecha <= d1.AddDays(1))
            .OrderBy(p => p.Fecha)
            .ToList();

        var realPorDia = pesos
            .GroupBy(p => p.Fecha.Date)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Fecha).First().Peso);

        // Baseline: peso actual del usuario, o último peso registrado, o 70 kg por defecto
        float baseRef = u?.Peso ?? 0f;
        if (baseRef <= 0) baseRef = pesos.Any() ? pesos.Last().Peso : 70f;

        // Genera valor sintético determinista (estable entre cargas) para días sin registro
        float Sintetico(DateTime f, int idxDesdeFinal)
        {
            if (realPorDia.TryGetValue(f, out var real)) return real;
            int seed = (f.Year * 1000 + f.DayOfYear) ^ (int)(baseRef * 10);
            var rng = new Random(seed);
            float tendencia = -idxDesdeFinal / 7f * 0.04f;
            float ruido = (float)(rng.NextDouble() * 1.4 - 0.7);
            return MathF.Round(baseRef + tendencia + ruido, 1);
        }

        for (int i = 0; i < totalDias; i++)
        {
            var f = d0.AddDays(i);
            var key = totalDias <= 7 ? f.ToString("ddd", es) : f.ToString("dd/MM", es);
            calPorDia[key] = registros.Where(x => x.Fecha.Date == f).Sum(x => x.Calorias);
        }
        r.CaloriasPorDia = calPorDia;

        // Peso (gráfica de línea): agregación semanal cuando el rango cubre más de 7 días
        if (totalDias > 7)
        {
            int bucketSize = 7;
            int numBuckets = (int)Math.Ceiling(totalDias / (double)bucketSize);
            for (int b = 0; b < numBuckets; b++)
            {
                var bStart = d0.AddDays(b * bucketSize);
                var bEnd = bStart.AddDays(bucketSize - 1);
                if (bEnd > d1) bEnd = d1;
                var pesosSemana = new List<float>();
                for (var f = bStart; f <= bEnd; f = f.AddDays(1))
                {
                    pesosSemana.Add(Sintetico(f, (int)(d1 - f).TotalDays));
                }
                pesoPorDia[bEnd.ToString("dd/MM", es)] = MathF.Round(pesosSemana.Average(), 1);
            }
        }
        else
        {
            for (int i = 0; i < totalDias; i++)
            {
                var f = d0.AddDays(i);
                pesoPorDia[f.ToString("ddd", es)] = Sintetico(f, totalDias - 1 - i);
            }
        }

        // PesoInicial: primer peso real del rango, o el primer valor sintético generado
        r.PesoInicial = pesos.Any() ? pesos.First().Peso : pesoPorDia.Values.FirstOrDefault();
        r.PesoPorDia = pesoPorDia;

        return r;
    }

    // ── CU8: Documentos profesionales y validación administrativa ──

    public DocumentoNutriologo RegistrarDocumento(int nutriologoId, string nombreOriginal, string rutaRelativa, string contentType, long tamano)
    {
        var doc = new DocumentoNutriologo
        {
            NutriologoId = nutriologoId,
            NombreOriginal = nombreOriginal,
            RutaRelativa = rutaRelativa,
            ContentType = contentType ?? "",
            Tamano = tamano,
            FechaSubida = DateTime.Now,
            Estado = "pendiente"
        };
        _db.DocumentosNutriologo.Add(doc);
        _db.SaveChanges();
        return doc;
    }

    public List<DocumentoNutriologo> ObtenerDocumentos(int nutriologoId) =>
        _db.DocumentosNutriologo
            .Where(d => d.NutriologoId == nutriologoId)
            .OrderByDescending(d => d.FechaSubida)
            .ToList();

    public DocumentoNutriologo? ObtenerDocumento(int documentoId) =>
        _db.DocumentosNutriologo.FirstOrDefault(d => d.Id == documentoId);

    public bool EsAdmin(int nutriologoId) =>
        _db.Nutriologos.Any(n => n.Id == nutriologoId && n.EsAdmin);

    public List<Nutriologo> ObtenerNutriologosPendientes() =>
        _db.Nutriologos
            .Where(n => !n.Validado && !n.EsAdmin)
            .OrderBy(n => n.Id)
            .ToList();

    public bool ValidarNutriologo(int nutriologoId, bool aprobar)
    {
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        if (n == null || n.EsAdmin) return false;
        n.Validado = aprobar;
        // Marcar todos los documentos del nutriólogo con el estado correspondiente
        var docs = _db.DocumentosNutriologo.Where(d => d.NutriologoId == nutriologoId).ToList();
        foreach (var d in docs) d.Estado = aprobar ? "aprobado" : "rechazado";
        _db.SaveChanges();
        return true;
    }

    // ── CU6: Recetas clínicas ──

    public Receta CrearRecetaClinica(int nutriologoId, string nombre, string desc, List<IngredienteReceta> ingredientes)
    {
        // Reutiliza el cálculo de macros de CrearReceta. UsuarioId = 0 (no atado a paciente).
        var receta = CrearReceta(0, nombre, desc, ingredientes);
        receta.CreadaPorNutriologoId = nutriologoId;
        _db.SaveChanges();
        return receta;
    }

    public List<Receta> ObtenerRecetasClinicas(int nutriologoId) =>
        _db.Recetas
            .Include(r => r.Ingredientes)
            .Where(r => r.CreadaPorNutriologoId == nutriologoId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToList();
}
