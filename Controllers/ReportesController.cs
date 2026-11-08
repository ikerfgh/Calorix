using Microsoft.AspNetCore.Mvc;
using Calorix.Services;

namespace Calorix.Controllers;

/// <summary>Exporta reportes PDF al usuario (paciente).</summary>
public class ReportesController : Controller
{
    private readonly IReportePdfService _reportes;

    public ReportesController(IReportePdfService reportes) => _reportes = reportes;

    [HttpGet]
    public IActionResult Progreso(int dias = 30)
    {
        if (!CheckAuth(out int userId)) return RedirectToAction("Login", "Home");
        var pdf = _reportes.GenerarReporteProgreso(userId, dias);
        return File(pdf, "application/pdf", $"Calorix_Progreso_{DateTime.Today:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public IActionResult Planes()
    {
        if (!CheckAuth(out int userId)) return RedirectToAction("Login", "Home");
        var pdf = _reportes.GenerarReportePlanes(userId);
        return File(pdf, "application/pdf", $"Calorix_Planes_{DateTime.Today:yyyyMMdd}.pdf");
    }

    private bool CheckAuth(out int userId)
    {
        userId = 0;
        var id = HttpContext.Session.GetString("UserId");
        return id != null && HttpContext.Session.GetString("UserRole") == "Usuario" && int.TryParse(id, out userId);
    }
}
