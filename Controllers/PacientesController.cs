using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class PacientesController : Controller
{
    private readonly ICalorixFacade _facade;

    public PacientesController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index(string q = "", int pacienteId = 0)
    {
        if (!CheckAuth(out int nutriologoId)) return RedirectToAction("Login", "Home");

        ViewBag.Pacientes = _facade.ObtenerPacientes(nutriologoId);
        ViewBag.Busqueda = string.IsNullOrEmpty(q) ? new List<object>() : _facade.BuscarUsuarios(q).Cast<object>().ToList();
        ViewBag.Q = q;

        if (pacienteId > 0)
        {
            ViewBag.ResumenPaciente = _facade.ObtenerResumenPaciente(nutriologoId, pacienteId, DateTime.Today);
            ViewBag.PacienteSeleccionado = pacienteId;
        }

        return View();
    }

    [HttpPost]
    public IActionResult Vincular(int usuarioId)
    {
        if (!CheckAuth(out int nutriologoId)) return Json(new { ok = false });
        var ok = _facade.VincularPaciente(nutriologoId, usuarioId);
        return Json(new { ok });
    }

    private bool CheckAuth(out int nutriologoId)
    {
        nutriologoId = 0;
        var id = HttpContext.Session.GetString("UserId");
        var r = HttpContext.Session.GetString("UserRole");
        return id != null && r == "Nutriologo" && int.TryParse(id, out nutriologoId);
    }
}
