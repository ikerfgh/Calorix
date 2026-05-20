using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class PerfilController : Controller
{
    private readonly ICalorixFacade _facade;

    public PerfilController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index()
    {
        if (!CheckAuth(out int userId)) return RedirectToAction("Login", "Home");
        ViewBag.Usuario = _facade.ObtenerPerfil(userId);
        ViewBag.NutriologoActual = _facade.ObtenerNutriologoDelUsuario(userId);
        return View();
    }

    [HttpPost]
    public IActionResult Actualizar(float peso, float altura, int edad, string actividad)
    {
        if (!CheckAuth(out int userId)) return RedirectToAction("Login", "Home");
        _facade.ActualizarPerfil(userId, peso, altura, edad, actividad);
        var u = _facade.ObtenerPerfil(userId);
        HttpContext.Session.SetString("UserName", u?.Nombre ?? "");
        TempData["Msg"] = "Perfil actualizado. Meta calórica recalculada.";
        return RedirectToAction("Index");
    }

    private bool CheckAuth(out int userId)
    {
        userId = 0;
        var id = HttpContext.Session.GetString("UserId");
        return id != null && HttpContext.Session.GetString("UserRole") == "Usuario" && int.TryParse(id, out userId);
    }
}
