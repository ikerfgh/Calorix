using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class NutriologoController : Controller
{
    private readonly ICalorixFacade _facade;

    public NutriologoController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index()
    {
        var idStr = HttpContext.Session.GetString("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (idStr != null && role == "Nutriologo" && int.TryParse(idStr, out int nId))
        {
            ViewBag.Nutriologo = _facade.ObtenerNutriologo(nId);
            ViewBag.YaRegistrado = true;
        }
        else
        {
            ViewBag.YaRegistrado = false;
        }
        return View();
    }

    [HttpPost]
    public IActionResult Registrar(string nombre, string email, string password, string cedula, string especialidad, int aniosExp)
    {
        var nutriologo = _facade.RegistrarNutriologo(nombre, email, password, cedula, especialidad, aniosExp);
        HttpContext.Session.SetString("UserId", nutriologo.Id.ToString());
        HttpContext.Session.SetString("UserRole", "Nutriologo");
        HttpContext.Session.SetString("UserName", nutriologo.Nombre);
        TempData["Msg"] = "Registro completado. Tu cuenta está pendiente de validación.";
        return RedirectToAction("Index", "Pacientes");
    }
}
