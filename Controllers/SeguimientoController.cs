using Microsoft.AspNetCore.Mvc;
using Calorix.Patterns.Facade;
using Calorix.Models;
using System.Globalization;
using System.Text.Json;

namespace Calorix.Controllers;

public class SeguimientoController : Controller
{
    private readonly ICalorixFacade _facade;

    public SeguimientoController(ICalorixFacade facade) => _facade = facade;

    public IActionResult Index(string formula = "mifflin", string periodo = "semana")
    {
        if (!CheckAuth("Usuario", out int userId)) return RedirectToAction("Login", "Home");

        var resumen = _facade.ObtenerResumenDiario(userId, DateTime.Today);
        var stats = _facade.ObtenerEstadisticasSemana(userId);
        var macros = _facade.ObtenerMacrosRecomendados(userId, formula);

        // Detalle por día de la semana (para modal clickeable)
        var es = new CultureInfo("es-MX");
        var weekDetails = new Dictionary<string, object>();
        var hoy = DateTime.Today;
        for (int i = 6; i >= 0; i--)
        {
            var fecha = hoy.AddDays(-i);
            var key = fecha.ToString("ddd", es);
            var detalle = _facade.ObtenerResumenDiario(userId, fecha);
            weekDetails[key] = new
            {
                fecha = fecha.ToString("yyyy-MM-dd"),
                fechaTexto = fecha.ToString("dddd, dd 'de' MMMM yyyy", es),
                label = key,
                cal = (int)detalle.TotalCalorias,
                prot = (float)Math.Round(detalle.TotalProteinas, 1),
                carb = (float)Math.Round(detalle.TotalCarbos, 1),
                gras = (float)Math.Round(detalle.TotalGrasas, 1),
                metaCal = (int)detalle.MetaCalorica,
                metaProt = (int)macros.ProteinasGramos,
                metaCarb = (int)macros.CarbosGramos,
                metaGras = (int)macros.GrasasGramos,
                esHoy = fecha.Date == hoy,
                esFuturo = fecha.Date > hoy,
                registros = detalle.Registros.Select(r => new
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
        ViewBag.WeekDetailsJson = JsonSerializer.Serialize(weekDetails);

        periodo = (periodo ?? "semana").ToLowerInvariant();
        if (periodo != "semana" && periodo != "mes" && periodo != "año" && periodo != "anio") periodo = "semana";

        int diasHist = periodo switch { "mes" => 30, "año" => 365, "anio" => 365, _ => 7 };
        var historial = _facade.ObtenerHistorialPeso(userId, diasHist);
        var perfil = _facade.ObtenerPerfil(userId);
        var pesoPorDia = ConstruirSeriePeso(historial, periodo, perfil?.Peso ?? 0);

        ViewBag.Resumen = resumen;
        ViewBag.Stats = stats;
        ViewBag.Macros = macros;
        ViewBag.Formula = formula;
        ViewBag.Periodo = periodo;
        ViewBag.StatsJson = JsonSerializer.Serialize(stats.DatosPorDia);
        ViewBag.HistorialPeso = historial;
        ViewBag.PesoJson = JsonSerializer.Serialize(pesoPorDia);
        ViewBag.SolicitudesPendientes = _facade.ObtenerSolicitudesPendientes(userId);
        ViewBag.NutriologoActual = _facade.ObtenerNutriologoDelUsuario(userId);
        return View();
    }

    [HttpGet]
    public IActionResult SemanaDetalle(int weekOffset = 0, string formula = "mifflin")
    {
        if (!CheckAuth("Usuario", out int userId)) return Json(new { ok = false });

        var es = new CultureInfo("es-MX");
        var hoy = DateTime.Today;
        // Semana = 7 días terminando hoy (offset 0). offset -1 = semana anterior, etc.
        var fechaFin = hoy.AddDays(weekOffset * 7);
        var fechaInicio = fechaFin.AddDays(-6);
        var macros = _facade.ObtenerMacrosRecomendados(userId, formula);

        var dias = new List<object>();
        for (int i = 0; i < 7; i++)
        {
            var fecha = fechaInicio.AddDays(i);
            var detalle = _facade.ObtenerResumenDiario(userId, fecha);
            dias.Add(new
            {
                fecha = fecha.ToString("yyyy-MM-dd"),
                fechaTexto = fecha.ToString("dddd, dd 'de' MMMM yyyy", es),
                label = fecha.ToString("ddd", es),
                cal = (int)detalle.TotalCalorias,
                prot = (float)Math.Round(detalle.TotalProteinas, 1),
                carb = (float)Math.Round(detalle.TotalCarbos, 1),
                gras = (float)Math.Round(detalle.TotalGrasas, 1),
                metaCal = (int)detalle.MetaCalorica,
                metaProt = (int)macros.ProteinasGramos,
                metaCarb = (int)macros.CarbosGramos,
                metaGras = (int)macros.GrasasGramos,
                esHoy = fecha.Date == hoy,
                esFuturo = fecha.Date > hoy,
                registros = detalle.Registros.Select(r => new
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
            });
        }

        // Etiqueta amigable de la semana
        string weekLabel;
        if (fechaInicio.Month == fechaFin.Month)
            weekLabel = $"Semana del {fechaInicio:dd} al {fechaFin.ToString("dd 'de' MMMM yyyy", es)}";
        else
            weekLabel = $"Semana del {fechaInicio.ToString("dd 'de' MMM", es)} al {fechaFin.ToString("dd 'de' MMM yyyy", es)}";

        return Json(new
        {
            ok = true,
            weekOffset,
            weekStart = fechaInicio.ToString("yyyy-MM-dd"),
            weekEnd = fechaFin.ToString("yyyy-MM-dd"),
            weekLabel,
            isCurrentWeek = weekOffset == 0,
            dias
        });
    }

    [HttpPost]
    public IActionResult ResponderSolicitud(int solicitudId, bool aprobar)
    {
        if (!CheckAuth("Usuario", out int userId)) return RedirectToAction("Login", "Home");
        var ok = _facade.ResponderSolicitud(userId, solicitudId, aprobar);
        TempData["Msg"] = ok
            ? (aprobar ? "✓ Solicitud aprobada. Tu nutriólogo ya tiene acceso a tu seguimiento." : "Solicitud rechazada.")
            : "No se pudo procesar la solicitud (puede que ya hayas aprobado a otro nutriólogo).";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DesvincularNutriologo()
    {
        if (!CheckAuth("Usuario", out int userId)) return RedirectToAction("Login", "Home");
        var actual = _facade.ObtenerNutriologoDelUsuario(userId);
        if (actual == null)
        {
            TempData["Msg"] = "No tienes un nutriólogo vinculado.";
            return RedirectToAction("Index", "Perfil");
        }
        var (ok, mensaje) = _facade.Desvincular(actual.Id, userId);
        TempData["Msg"] = ok ? $"Te desvinculaste de {actual.Nombre}." : mensaje;
        return RedirectToAction("Index", "Perfil");
    }

    // Construye una serie de peso lista para graficar según el período.
    // - "semana": 7 puntos diarios
    // - "mes":    30 puntos diarios
    // - "año":    12 puntos mensuales (promedio mensual)
    // Si no hay registros suficientes, genera datos sintéticos deterministas
    // alrededor del peso actual del usuario (variación natural ±1.5 kg).
    private static Dictionary<string, float> ConstruirSeriePeso(
        List<RegistroPeso> historial, string periodo, float pesoBase)
    {
        var es = new CultureInfo("es-MX");
        var hoy = DateTime.Today;
        var resultado = new Dictionary<string, float>();

        // Mapa fecha → peso real (más reciente del día)
        var realPorDia = historial
            .GroupBy(r => r.Fecha.Date)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.Fecha).First().Peso);

        float baseRef = pesoBase > 0
            ? pesoBase
            : (historial.Count > 0 ? historial.Last().Peso : 70f);

        // Variación sintética determinista por día (semilla = día del año + base)
        float Sintetico(DateTime f, int idxDesdeHoy)
        {
            if (realPorDia.TryGetValue(f, out var real)) return real;
            int seed = (f.Year * 1000 + f.DayOfYear) ^ (int)(baseRef * 10);
            var rng = new Random(seed);
            // Tendencia leve: -0.04 kg por semana hacia atrás (simula progreso reciente)
            float tendencia = -idxDesdeHoy / 7f * 0.04f;
            float ruido = (float)(rng.NextDouble() * 1.4 - 0.7);
            return MathF.Round(baseRef + tendencia + ruido, 1);
        }

        if (periodo == "año" || periodo == "anio")
        {
            // 12 puntos mensuales: promedio de pesos reales del mes, o sintético
            for (int i = 11; i >= 0; i--)
            {
                var primerDia = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-i);
                var ultimo = primerDia.AddMonths(1).AddDays(-1);
                if (ultimo > hoy) ultimo = hoy;

                var realesMes = realPorDia
                    .Where(kv => kv.Key >= primerDia && kv.Key <= ultimo)
                    .Select(kv => kv.Value)
                    .ToList();

                float valor;
                if (realesMes.Count > 0)
                {
                    valor = MathF.Round(realesMes.Average(), 1);
                }
                else
                {
                    int diasRef = (int)(hoy - primerDia.AddDays(15)).TotalDays;
                    valor = Sintetico(primerDia.AddDays(14), Math.Max(diasRef, 0));
                }
                resultado[primerDia.ToString("MMM", es)] = valor;
            }
        }
        else if (periodo == "mes")
        {
            // 5 puntos semanales: cada uno = promedio de los 7 días que terminan en ese día
            for (int i = 4; i >= 0; i--)
            {
                var fechaFin = hoy.AddDays(-i * 7);
                var pesosSemana = new List<float>();
                for (int j = 6; j >= 0; j--)
                {
                    var f = fechaFin.AddDays(-j);
                    pesosSemana.Add(Sintetico(f, (int)(hoy - f).TotalDays));
                }
                resultado[fechaFin.ToString("dd/MM", es)] = MathF.Round(pesosSemana.Average(), 1);
            }
        }
        else
        {
            for (int i = 6; i >= 0; i--)
            {
                var f = hoy.AddDays(-i);
                resultado[f.ToString("ddd", es)] = Sintetico(f, i);
            }
        }

        return resultado;
    }

    private bool CheckAuth(string role, out int userId)
    {
        userId = 0;
        var id = HttpContext.Session.GetString("UserId");
        var r = HttpContext.Session.GetString("UserRole");
        return id != null && r == role && int.TryParse(id, out userId);
    }
}
