using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class AdminController : Controller
{
    private readonly ICalorixFacade _facade;
    private readonly IWebHostEnvironment _env;

    public AdminController(ICalorixFacade facade, IWebHostEnvironment env)
    {
        _facade = facade;
        _env = env;
    }

    public IActionResult Index()
    {
        if (!CheckAdmin()) return RedirectToAction("Login", "Home");

        var pendientes = _facade.ObtenerNutriologosPendientes();
        ViewBag.Pendientes = pendientes;
        ViewBag.DocumentosPorNutriologo = pendientes
            .ToDictionary(n => n.Id, n => _facade.ObtenerDocumentos(n.Id));
        return View();
    }

    [HttpPost]
    public IActionResult Aprobar(int nutriologoId)
    {
        if (!CheckAdmin()) return RedirectToAction("Login", "Home");
        var ok = _facade.ValidarNutriologo(nutriologoId, true);
        TempData["Msg"] = ok ? "Nutriólogo aprobado." : "No se pudo aprobar.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Rechazar(int nutriologoId)
    {
        if (!CheckAdmin()) return RedirectToAction("Login", "Home");
        var ok = _facade.ValidarNutriologo(nutriologoId, false);
        TempData["Msg"] = ok ? "Nutriólogo rechazado." : "No se pudo rechazar.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Documento(int id)
    {
        if (!CheckAdmin()) return RedirectToAction("Login", "Home");
        var doc = _facade.ObtenerDocumento(id);
        if (doc == null) return NotFound();

        var rutaCompleta = Path.Combine(_env.WebRootPath, doc.RutaRelativa.Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(rutaCompleta)) return NotFound();

        var contentType = string.IsNullOrEmpty(doc.ContentType) ? "application/octet-stream" : doc.ContentType;
        return PhysicalFile(rutaCompleta, contentType, doc.NombreOriginal);
    }

    private bool CheckAdmin()
    {
        var idStr = HttpContext.Session.GetString("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Nutriologo" || idStr == null || !int.TryParse(idStr, out int nId)) return false;
        return _facade.EsAdmin(nId);
    }
}
