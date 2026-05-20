using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class CuentaController : Controller
{
    private readonly ICalorixFacade _facade;

    public CuentaController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserId") != null)
            return RedirectToAction("Index", "Seguimiento");
        return View();
    }

    [HttpPost]
    public IActionResult Registrar(string nombre, string email, string password, int edad, float peso, float altura, string genero, string nivelActividad)
    {
        var (usuario, error) = _facade.RegistrarUsuario(nombre, email, password, edad, peso, altura, genero, nivelActividad);
        if (usuario == null)
        {
            ViewBag.Error = error ?? "No se pudo crear la cuenta.";
            ViewBag.Form = new { nombre, email, edad, peso, altura, genero, nivelActividad };
            return View("Index");
        }
        HttpContext.Session.SetString("UserId", usuario.Id.ToString());
        HttpContext.Session.SetString("UserRole", "Usuario");
        HttpContext.Session.SetString("UserName", usuario.Nombre);
        TempData["Msg"] = "¡Bienvenido a Calorix! Tu cuenta fue creada.";
        return RedirectToAction("Index", "Seguimiento");
    }
}
