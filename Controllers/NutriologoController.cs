using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;

namespace Calorix.Controllers;

public class NutriologoController : Controller
{
    private static readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly ICalorixFacade _facade;
    private readonly IWebHostEnvironment _env;

    public NutriologoController(ICalorixFacade facade, IWebHostEnvironment env)
    {
        _facade = facade;
        _env = env;
    }

    public IActionResult Index()
    {
        var idStr = HttpContext.Session.GetString("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (idStr != null && role == "Nutriologo" && int.TryParse(idStr, out int nId))
        {
            ViewBag.Nutriologo = _facade.ObtenerNutriologo(nId);
            ViewBag.YaRegistrado = true;
            ViewBag.Documentos = _facade.ObtenerDocumentos(nId);
            ViewBag.EsAdmin = _facade.EsAdmin(nId);
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
        var (nutriologo, error) = _facade.RegistrarNutriologo(nombre, email, password, cedula, especialidad, aniosExp);
        if (nutriologo == null)
        {
            ViewBag.Error = error ?? "No se pudo crear la cuenta.";
            ViewBag.Form = new { nombre, email, cedula, especialidad, aniosExp };
            ViewBag.YaRegistrado = false;
            return View("Index");
        }
        HttpContext.Session.SetString("UserId", nutriologo.Id.ToString());
        HttpContext.Session.SetString("UserRole", "Nutriologo");
        HttpContext.Session.SetString("UserName", nutriologo.Nombre);
        TempData["Msg"] = "Registro completado. Tu cuenta está pendiente de validación.";
        return RedirectToAction("Index", "Pacientes");
    }

    [HttpPost]
    public async Task<IActionResult> SubirDocumento(IFormFile documento)
    {
        var idStr = HttpContext.Session.GetString("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (idStr == null || role != "Nutriologo" || !int.TryParse(idStr, out int nId))
            return RedirectToAction("Login", "Home");

        if (documento == null || documento.Length == 0)
        {
            TempData["Err"] = "Selecciona un archivo válido.";
            return RedirectToAction("Index");
        }

        var ext = Path.GetExtension(documento.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(ext))
        {
            TempData["Err"] = "Formato no permitido. Sube un PDF, JPG o PNG.";
            return RedirectToAction("Index");
        }

        if (documento.Length > MaxFileSize)
        {
            TempData["Err"] = "El archivo supera el límite de 5 MB.";
            return RedirectToAction("Index");
        }

        var carpeta = Path.Combine(_env.WebRootPath, "uploads", "nutriologos", nId.ToString());
        Directory.CreateDirectory(carpeta);

        var nombreSeguro = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
        var rutaCompleta = Path.Combine(carpeta, nombreSeguro);
        await using (var fs = new FileStream(rutaCompleta, FileMode.Create))
        {
            await documento.CopyToAsync(fs);
        }

        var rutaRelativa = $"uploads/nutriologos/{nId}/{nombreSeguro}";
        _facade.RegistrarDocumento(nId, documento.FileName, rutaRelativa, documento.ContentType ?? "", documento.Length);

        TempData["Msg"] = $"Documento \"{documento.FileName}\" subido. Queda pendiente de revisión.";
        return RedirectToAction("Index");
    }
}
