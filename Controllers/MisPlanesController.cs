using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class MisPlanesController : Controller
{
    private readonly ICalorixFacade _facade;

    public MisPlanesController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index()
    {
        if (!CheckAuth(out int userId)) return RedirectToAction("Login", "Home");
        ViewBag.Planes = _facade.ObtenerPlanesPaciente(userId);
        ViewBag.Observaciones = _facade.ObtenerObservacionesPaciente(userId);
        return View();
    }

    [HttpPost]
    public IActionResult MarcarLeidas()
    {
        if (!CheckAuth(out int userId)) return Json(new { ok = false });
        var n = _facade.MarcarObservacionesLeidas(userId);
        return Json(new { ok = true, marcadas = n });
    }

    private bool CheckAuth(out int userId)
    {
        userId = 0;
        var id = HttpContext.Session.GetString("UserId");
        return id != null && HttpContext.Session.GetString("UserRole") == "Usuario" && int.TryParse(id, out userId);
    }
}
