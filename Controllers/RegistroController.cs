using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class RegistroController : Controller
{
    private readonly ICalorixFacade _facade;

    public RegistroController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index(string q = "")
    {
        if (!CheckAuth("Usuario", out int userId)) return RedirectToAction("Login", "Home");

        var resumen = _facade.ObtenerResumenDiario(userId, DateTime.Today);
        ViewBag.Resumen = resumen;
        ViewBag.Alimentos = _facade.BuscarAlimentos(q);
        ViewBag.Recetas = _facade.ObtenerRecetasUsuario(userId);
        ViewBag.Q = q;
        return View();
    }

    [HttpPost]
    public IActionResult Registrar(int alimentoId, bool esReceta, float cantidad, string momento)
    {
        if (!CheckAuth("Usuario", out int userId)) return Json(new { ok = false });

        if (esReceta)
            _facade.RegistrarRecetaComida(userId, alimentoId, cantidad, momento);
        else
            _facade.RegistrarComida(userId, alimentoId, cantidad, momento);

        return Json(new { ok = true });
    }

    [HttpPost]
    public IActionResult Eliminar(int registroId)
    {
        if (!CheckAuth("Usuario", out int userId)) return Json(new { ok = false });
        var ok = _facade.EliminarRegistro(registroId, userId);
        return Json(new { ok });
    }

    private bool CheckAuth(string role, out int userId)
    {
        userId = 0;
        var id = HttpContext.Session.GetString("UserId");
        var r = HttpContext.Session.GetString("UserRole");
        return id != null && r == role && int.TryParse(id, out userId);
    }
}
