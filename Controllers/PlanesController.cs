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

        if (pacienteId > 0)
            ViewBag.Planes = _facade.ObtenerPlanesPaciente(pacienteId);

        return View();
    }

    [HttpPost]
    public IActionResult Crear(int pacienteId, string nombre, string comidasJson)
    {
        if (!CheckAuth(out int nutriologoId)) return Json(new { ok = false });

        var comidas = new List<ComidaPlan>();
        try
        {
            var raw = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(comidasJson ?? "[]");
            if (raw != null)
                foreach (var c in raw)
                    comidas.Add(new ComidaPlan
                    {
                        Momento = c.TryGetValue("momento", out var m) ? m.GetString() ?? "" : "",
                        Descripcion = c.TryGetValue("descripcion", out var d) ? d.GetString() ?? "" : "",
                        Calorias = c.TryGetValue("calorias", out var cal) ? cal.GetSingle() : 0,
                        AlimentoIds = c.TryGetValue("alimentoIds", out var ids)
                            ? ids.EnumerateArray().Select(x => x.GetInt32()).ToList()
                            : new()
                    });
        }
        catch { }

        var plan = _facade.CrearPlanAlimenticio(nutriologoId, pacienteId, nombre, comidas);
        return Json(new { ok = true, id = plan.Id });
    }

    private bool CheckAuth(out int nutriologoId)
    {
        nutriologoId = 0;
        var id = HttpContext.Session.GetString("UserId");
        var r = HttpContext.Session.GetString("UserRole");
        return id != null && r == "Nutriologo" && int.TryParse(id, out nutriologoId);
    }
}
