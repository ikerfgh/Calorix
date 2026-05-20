using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;
using Calorix.Models;
using System.Text.Json;

namespace Calorix.Controllers;

public class PlanesController : Controller
{
    private readonly ICalorixFacade _facade;

    public PlanesController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index(int pacienteId = 0)
    {
        if (!CheckAuth(out int nutriologoId)) return RedirectToAction("Login", "Home");

        ViewBag.Pacientes = _facade.ObtenerPacientes(nutriologoId);
        ViewBag.TodosAlimentos = _facade.ObtenerTodosAlimentos();
        ViewBag.PacienteId = pacienteId;
        ViewBag.RecetasClinicas = _facade.ObtenerRecetasClinicas(nutriologoId);

        if (pacienteId > 0)
            ViewBag.Planes = _facade.ObtenerPlanesPaciente(pacienteId);

        return View();
    }

    [HttpPost]
    public IActionResult Crear(string nombre, string comidasJson, string pacienteIdsJson)
    {
        if (!CheckAuth(out int nutriologoId)) return Json(new { ok = false, mensaje = "Sin sesión" });

        var comidas = ParseComidas(comidasJson);
        var pacienteIds = ParsePacienteIds(pacienteIdsJson);
        if (pacienteIds.Count == 0) return Json(new { ok = false, mensaje = "Selecciona al menos un paciente" });

        var planes = _facade.CrearPlanesAlimenticios(nutriologoId, pacienteIds, nombre, comidas);
        return Json(new { ok = true, creados = planes.Count, ids = planes.Select(p => p.Id) });
    }

    [HttpPost]
    public IActionResult Editar(int planId, string nombre, string comidasJson)
    {
        if (!CheckAuth(out int nutriologoId)) return Json(new { ok = false, mensaje = "Sin sesión" });
        var comidas = ParseComidas(comidasJson);
        var ok = _facade.EditarPlanAlimenticio(nutriologoId, planId, nombre, comidas);
        return Json(new { ok, mensaje = ok ? "Plan actualizado" : "No se pudo actualizar el plan" });
    }

    [HttpPost]
    public IActionResult Eliminar(int planId)
    {
        if (!CheckAuth(out int nutriologoId)) return Json(new { ok = false, mensaje = "Sin sesión" });
        var ok = _facade.EliminarPlanAlimenticio(nutriologoId, planId);
        return Json(new { ok, mensaje = ok ? "Plan eliminado" : "No se pudo eliminar" });
    }

    [HttpGet]
    public IActionResult Detalle(int planId)
    {
        if (!CheckAuth(out int nutriologoId)) return Json(new { ok = false });
        var plan = _facade.ObtenerPlan(planId);
        if (plan == null || plan.NutriologoId != nutriologoId) return Json(new { ok = false });
        return Json(new
        {
            ok = true,
            id = plan.Id,
            nombre = plan.Nombre,
            pacienteId = plan.PacienteId,
            comidas = plan.Comidas.Select(c => new
            {
                momento = c.Momento,
                descripcion = c.Descripcion,
                calorias = c.Calorias,
                recetaId = c.RecetaId
            })
        });
    }

    [HttpPost]
    public IActionResult CrearRecetaClinica(string nombre, string descripcion, string ingredientesJson)
    {
        if (!CheckAuth(out int nutriologoId)) return Json(new { ok = false, mensaje = "Sin sesión" });
        if (string.IsNullOrWhiteSpace(nombre)) return Json(new { ok = false, mensaje = "Nombre obligatorio" });

        var ingredientes = ParseIngredientes(ingredientesJson);
        if (ingredientes.Count == 0) return Json(new { ok = false, mensaje = "Agrega al menos un ingrediente" });

        var receta = _facade.CrearRecetaClinica(nutriologoId, nombre.Trim(), descripcion ?? "", ingredientes);
        return Json(new
        {
            ok = true,
            id = receta.Id,
            nombre = receta.Nombre,
            calorias = (int)receta.CaloriasTotal,
            proteinas = (int)receta.ProteinasTotal,
            carbos = (int)receta.CarbosTotal,
            grasas = (int)receta.GrasasTotal
        });
    }

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static List<ComidaPlan> ParseComidas(string comidasJson)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<List<ComidaPlan>>(comidasJson ?? "[]", _jsonOpts);
            if (parsed == null) return new();
            foreach (var c in parsed) { c.Id = 0; c.PlanAlimenticioId = 0; c.AlimentoIds ??= new(); }
            return parsed;
        }
        catch
        {
            return new();
        }
    }

    private static List<int> ParsePacienteIds(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<int>>(json ?? "[]", _jsonOpts) ?? new();
        }
        catch
        {
            return new();
        }
    }

    private static List<IngredienteReceta> ParseIngredientes(string json)
    {
        var list = new List<IngredienteReceta>();
        if (string.IsNullOrEmpty(json)) return list;
        try
        {
            var raw = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json, _jsonOpts);
            if (raw == null) return list;
            foreach (var item in raw)
            {
                int aId = 0; float cant = 0;
                foreach (var kv in item)
                {
                    var k = kv.Key.ToLowerInvariant();
                    if (k == "alimentoid") aId = kv.Value.GetInt32();
                    else if (k == "cantidad") cant = kv.Value.GetSingle();
                }
                if (aId > 0 && cant > 0)
                    list.Add(new IngredienteReceta { AlimentoId = aId, Cantidad = cant });
            }
        }
        catch { }
        return list;
    }

    private bool CheckAuth(out int nutriologoId)
    {
        nutriologoId = 0;
        var id = HttpContext.Session.GetString("UserId");
        var r = HttpContext.Session.GetString("UserRole");
        return id != null && r == "Nutriologo" && int.TryParse(id, out nutriologoId);
    }
}
