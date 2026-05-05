using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class HomeController : Controller
{
    private readonly ICalorixFacade _facade;

    public HomeController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserId") != null)
            return RedirectByRole();
        return RedirectToAction("Login");
    }

    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("UserId") != null)
            return RedirectByRole();
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var result = _facade.Login(email, password);
        if (result == null)
        {
            ViewBag.Error = "Credenciales incorrectas. Usa los accesos demo.";
            return View();
        }
        HttpContext.Session.SetString("UserId", result.Value.id.ToString());
        HttpContext.Session.SetString("UserRole", result.Value.role);
        string nombre = result.Value.role == "Usuario"
            ? _facade.ObtenerPerfil(result.Value.id)?.Nombre ?? email
            : _facade.ObtenerNutriologo(result.Value.id)?.Nombre ?? email;
        HttpContext.Session.SetString("UserName", nombre);
        return RedirectByRole();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private IActionResult RedirectByRole()
    {
        return HttpContext.Session.GetString("UserRole") == "Nutriologo"
            ? RedirectToAction("Index", "Pacientes")
            : RedirectToAction("Index", "Seguimiento");
    }
}
