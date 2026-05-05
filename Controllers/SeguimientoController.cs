using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;
using System.Text.Json;

namespace Calorix.Controllers;

public class SeguimientoController : Controller
{
    private readonly ICalorixFacade _facade;

    public SeguimientoController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index(string formula = "mifflin")
    {
        if (!CheckAuth("Usuario", out int userId)) return RedirectToAction("Login", "Home");

        var resumen = _facade.ObtenerResumenDiario(userId, DateTime.Today);
        var stats = _facade.ObtenerEstadisticasSemana(userId);
        var macros = _facade.ObtenerMacrosRecomendados(userId, formula);

        ViewBag.Resumen = resumen;
        ViewBag.Stats = stats;
        ViewBag.Macros = macros;
        ViewBag.Formula = formula;
        ViewBag.StatsJson = JsonSerializer.Serialize(stats.DatosPorDia);
        return View();
    }

    private bool CheckAuth(string role, out int userId)
    {
        userId = 0;
        var id = HttpContext.Session.GetString("UserId");
        var r = HttpContext.Session.GetString("UserRole");
        return id != null && r == role && int.TryParse(id, out userId);
    }
}
