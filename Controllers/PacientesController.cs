using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;
using System.Globalization;
using System.Text.Json;

namespace Calorix.Controllers;

public class PacientesController : Controller
{
    private readonly ICalorixFacade _facade;

    public PacientesController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index(string q = "", int pacienteId = 0, string vista = "dia", string fecha = "", int offset = 0)
    {
        if (!CheckAuth(out int nutriologoId)) return RedirectToAction("Login", "Home");

        ViewBag.Pacientes = _facade.ObtenerPacientes(nutriologoId);
        ViewBag.Busqueda = string.IsNullOrEmpty(q) ? new List<object>() : _facade.BuscarUsuarios(q).Cast<object>().ToList();
        ViewBag.Q = q;

        if (pacienteId > 0)
        {
            var (desde, hasta, etiqueta, fechaActual) = CalcularRango(vista, fecha, offset);
            var resumenRango = _facade.ObtenerResumenPacienteRango(nutriologoId, pacienteId, desde, hasta);
            resumenRango.Etiqueta = etiqueta;

            ViewBag.PacienteSeleccionado = pacienteId;
            ViewBag.Vista = vista;
            ViewBag.FechaActual = fechaActual.ToString("yyyy-MM-dd");
            ViewBag.ResumenRango = resumenRango;
            ViewBag.CaloriasJson = JsonSerializer.Serialize(resumenRango.CaloriasPorDia);
            ViewBag.PesoJson = JsonSerializer.Serialize(resumenRango.PesoPorDia);
            ViewBag.Observaciones = _facade.ObtenerObservacionesNutriologo(nutriologoId, pacienteId);
            ViewBag.PlanesPaciente = _facade.ObtenerPlanesPaciente(pacienteId);

            // Detalle por día (clickeable en la gráfica de calorías)
            var es = new CultureInfo("es-MX");
            var macros = _facade.ObtenerMacrosRecomendados(pacienteId, "mifflin");
            int dias = (int)(hasta.Date - desde.Date).TotalDays + 1;
            var dayDetails = new Dictionary<string, object>();
            for (int i = 0; i < dias; i++)
            {
                var f = desde.Date.AddDays(i);
                var key = dias <= 7 ? f.ToString("ddd", es) : f.ToString("dd/MM", es);
                var regsDia = resumenRango.Registros.Where(r => r.Fecha.Date == f).ToList();
                dayDetails[key] = new
                {
                    fecha = f.ToString("yyyy-MM-dd"),
                    fechaTexto = f.ToString("dddd, dd 'de' MMMM yyyy", es),
                    label = key,
                    cal = (int)regsDia.Sum(r => r.Calorias),
                    prot = (float)Math.Round(regsDia.Sum(r => r.Proteinas), 1),
                    carb = (float)Math.Round(regsDia.Sum(r => r.Carbohidratos), 1),
                    gras = (float)Math.Round(regsDia.Sum(r => r.Grasas), 1),
                    metaCal = (int)resumenRango.MetaCalorica,
                    metaProt = (int)macros.ProteinasGramos,
                    metaCarb = (int)macros.CarbosGramos,
                    metaGras = (int)macros.GrasasGramos,
                    esHoy = f.Date == DateTime.Today,
                    registros = regsDia.Select(r => new
                    {
                        nombre = r.NombreAlimento,
                        momento = r.Momento,
                        cantidad = r.Cantidad,
                        unidad = r.UnidadCantidad,
                        cal = (int)r.Calorias,
                        prot = (float)Math.Round(r.Proteinas, 1),
                        carb = (float)Math.Round(r.Carbohidratos, 1),
                        gras = (float)Math.Round(r.Grasas, 1)
                    }).ToList()
                };
            }
            ViewBag.DayDetailsJson = JsonSerializer.Serialize(dayDetails);
        }

        return View();
    }

    [HttpPost]
    public IActionResult Vincular(int usuarioId)
    {
        if (!CheckAuth(out int nutriologoId)) return Json(new { ok = false, mensaje = "Sin sesión" });
        var (ok, mensaje) = _facade.EnviarSolicitudVinculacion(nutriologoId, usuarioId);
        return Json(new { ok, mensaje });
    }

    [HttpPost]
    public IActionResult Desvincular(int pacienteId)
    {
        if (!CheckAuth(out int nutriologoId)) return RedirectToAction("Login", "Home");
        var (ok, mensaje) = _facade.Desvincular(nutriologoId, pacienteId);
        TempData["Msg"] = ok ? "Paciente desvinculado." : mensaje;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult EscribirObservacion(int pacienteId, int? planId, string texto)
    {
        if (!CheckAuth(out int nutriologoId)) return RedirectToAction("Login", "Home");
        if (!string.IsNullOrWhiteSpace(texto))
        {
            _facade.EscribirObservacion(nutriologoId, pacienteId, planId == 0 ? null : planId, texto.Trim());
            TempData["Msg"] = "Observación enviada al paciente.";
        }
        return RedirectToAction("Index", new { pacienteId });
    }

    private (DateTime desde, DateTime hasta, string etiqueta, DateTime fechaActual) CalcularRango(string vista, string fecha, int offset)
    {
        var es = new CultureInfo("es-MX");
        DateTime baseDate = DateTime.Today;
        if (!string.IsNullOrWhiteSpace(fecha) && DateTime.TryParse(fecha, out var parsed))
            baseDate = parsed.Date;

        DateTime desde, hasta, nuevaBase;
        string etiqueta;

        switch (vista)
        {
            case "semana":
                int diff = (7 + (int)baseDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var lunes = baseDate.AddDays(-diff).AddDays(offset * 7);
                desde = lunes;
                hasta = lunes.AddDays(6);
                nuevaBase = lunes;
                etiqueta = $"Semana del {desde:dd MMM} – {hasta:dd MMM yyyy}";
                break;

            case "mes":
                var primerDia = new DateTime(baseDate.Year, baseDate.Month, 1).AddMonths(offset);
                desde = primerDia;
                hasta = primerDia.AddMonths(1).AddDays(-1);
                nuevaBase = primerDia;
                etiqueta = primerDia.ToString("MMMM yyyy", es);
                break;

            default: // "dia"
                var dia = baseDate.AddDays(offset);
                desde = dia;
                hasta = dia;
                nuevaBase = dia;
                etiqueta = dia.ToString("dddd, dd MMMM yyyy", es);
                break;
        }
        return (desde, hasta, etiqueta, nuevaBase);
    }

    private bool CheckAuth(out int nutriologoId)
    {
        nutriologoId = 0;
        var id = HttpContext.Session.GetString("UserId");
        var r = HttpContext.Session.GetString("UserRole");
        return id != null && r == "Nutriologo" && int.TryParse(id, out nutriologoId);
    }
}
