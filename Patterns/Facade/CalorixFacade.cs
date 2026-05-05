using Calorix.Models;
using Calorix.Patterns.Singleton;
using Calorix.Patterns.Strategy;

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
    List<PlanAlimenticio> ObtenerPlanesPaciente(int pacienteId);
    Usuario? ObtenerPerfil(int usuarioId);
    bool ActualizarPerfil(int usuarioId, float peso, float altura, int edad, string actividad);
    (int id, string role)? Login(string email, string password);
    Nutriologo? ObtenerNutriologo(int id);
    Nutriologo RegistrarNutriologo(string nombre, string email, string password, string cedula, string especialidad, int aniosExp);
    List<Usuario> BuscarUsuarios(string termino);
    List<Alimento> ObtenerTodosAlimentos();
}

public class CalorixFacade : ICalorixFacade
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

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
            Id = _db.GetNextRecetaId(),
            Nombre = nombre, Descripcion = desc, Ingredientes = ingredientes,
            CaloriasTotal = MathF.Round(cal), ProteinasTotal = MathF.Round(prot),
            CarbosTotal = MathF.Round(carb), GrasasTotal = MathF.Round(grasas),
            UsuarioId = usuarioId, FechaCreacion = DateTime.Now
        };
        _db.Recetas.Add(receta);
        return receta;
    }

    public List<Receta> ObtenerRecetasUsuario(int usuarioId) =>
        _db.Recetas.Where(r => r.UsuarioId == usuarioId).ToList();

    public RegistroComida RegistrarComida(int usuarioId, int alimentoId, float cantidad, string momento)
    {
        var a = _db.Alimentos.FirstOrDefault(x => x.Id == alimentoId);
        float f = cantidad / 100f;
        var reg = new RegistroComida
        {
            Id = _db.GetNextRegistroId(),
            UsuarioId = usuarioId, AlimentoId = alimentoId,
            NombreAlimento = a?.Nombre ?? "Desconocido",
            Cantidad = cantidad, UnidadCantidad = "g", Momento = momento, Fecha = DateTime.Today,
            Calorias = MathF.Round((a?.Calorias ?? 0) * f),
            Proteinas = MathF.Round((a?.Proteinas ?? 0) * f),
            Carbohidratos = MathF.Round((a?.Carbohidratos ?? 0) * f),
            Grasas = MathF.Round((a?.Grasas ?? 0) * f),
        };
        _db.RegistrosComida.Add(reg);
        return reg;
    }

    public RegistroComida RegistrarRecetaComida(int usuarioId, int recetaId, float porciones, string momento)
    {
        var r = _db.Recetas.FirstOrDefault(x => x.Id == recetaId);
        var reg = new RegistroComida
        {
            Id = _db.GetNextRegistroId(),
            UsuarioId = usuarioId, AlimentoId = -recetaId,
            NombreAlimento = r?.Nombre ?? "Receta",
            Cantidad = porciones, UnidadCantidad = "porción(es)", Momento = momento, Fecha = DateTime.Today,
            Calorias = MathF.Round((r?.CaloriasTotal ?? 0) * porciones),
            Proteinas = MathF.Round((r?.ProteinasTotal ?? 0) * porciones),
            Carbohidratos = MathF.Round((r?.CarbosTotal ?? 0) * porciones),
            Grasas = MathF.Round((r?.GrasasTotal ?? 0) * porciones),
        };
        _db.RegistrosComida.Add(reg);
        return reg;
    }

    public bool EliminarRegistro(int registroId, int usuarioId)
    {
        var reg = _db.RegistrosComida.FirstOrDefault(r => r.Id == registroId && r.UsuarioId == usuarioId);
        if (reg == null) return false;
        _db.RegistrosComida.Remove(reg);
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
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        if (n == null || !_db.Usuarios.Any(u => u.Id == usuarioId)) return false;
        if (!n.PacienteIds.Contains(usuarioId)) n.PacienteIds.Add(usuarioId);
        return true;
    }

    public List<Usuario> ObtenerPacientes(int nutriologoId)
    {
        var n = _db.Nutriologos.FirstOrDefault(x => x.Id == nutriologoId);
        if (n == null) return new();
        return _db.Usuarios.Where(u => n.PacienteIds.Contains(u.Id)).ToList();
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
            Id = _db.GetNextPlanId(),
            Nombre = nombre, NutriologoId = nutriologoId, PacienteId = pacienteId,
            Comidas = comidas, FechaCreacion = DateTime.Now, Activo = true
        };
        _db.Planes.Add(plan);
        return plan;
    }

    public List<PlanAlimenticio> ObtenerPlanesPaciente(int pacienteId) =>
        _db.Planes.Where(p => p.PacienteId == pacienteId).ToList();

    public Usuario? ObtenerPerfil(int usuarioId) => _db.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

    public bool ActualizarPerfil(int usuarioId, float peso, float altura, int edad, string actividad)
    {
        var u = _db.Usuarios.FirstOrDefault(x => x.Id == usuarioId);
        if (u == null) return false;
        u.Peso = peso; u.Altura = altura; u.Edad = edad; u.NivelActividad = actividad;
        u.MetaCalorica = MathF.Round(new CalculadorNutricional(new MifflinStJeorStrategy()).CalcularMeta(u));
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
            Id = _db.GetNextNutriologoId(),
            Nombre = nombre, Email = email, Password = password,
            Cedula = cedula, Especialidad = especialidad, AniosExp = aniosExp,
            Validado = false, PacienteIds = new()
        };
        _db.Nutriologos.Add(n);
        return n;
    }

    public List<Usuario> BuscarUsuarios(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino)) return _db.Usuarios.ToList();
        termino = termino.ToLower();
        return _db.Usuarios.Where(u => u.Nombre.ToLower().Contains(termino) || u.Email.ToLower().Contains(termino)).ToList();
    }
}
