using System.Globalization;
using Calorix.Models;
using Calorix.Patterns.Facade;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Calorix.Services;

/// <summary>
/// Genera los reportes PDF que se exportan al usuario.
/// Reporte 1: Progreso nutricional (consumo diario vs meta + historial de peso).
/// Reporte 2: Planes alimenticios asignados + observaciones del nutriólogo.
/// </summary>
public interface IReportePdfService
{
    byte[] GenerarReporteProgreso(int usuarioId, int dias);
    byte[] GenerarReportePlanes(int usuarioId);
}

public class ReportePdfService : IReportePdfService
{
    private static readonly CultureInfo Es = new("es-MX");
    private const string Verde = "#16A34A";
    private readonly ICalorixFacade _facade;

    public ReportePdfService(ICalorixFacade facade) => _facade = facade;

    public byte[] GenerarReporteProgreso(int usuarioId, int dias)
    {
        if (dias is not (7 or 30 or 90)) dias = 30;
        var usuario = _facade.ObtenerPerfil(usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado");
        var macros = _facade.ObtenerMacrosRecomendados(usuarioId, "mifflin");
        var hoy = DateTime.Today;
        var desde = hoy.AddDays(-(dias - 1));

        var resumenes = Enumerable.Range(0, dias)
            .Select(i => _facade.ObtenerResumenDiario(usuarioId, desde.AddDays(i)))
            .ToList();
        var conRegistro = resumenes.Where(r => r.Registros.Count > 0).ToList();
        var pesos = _facade.ObtenerHistorialPeso(usuarioId, dias);

        float promCal = conRegistro.Count > 0 ? conRegistro.Average(r => r.TotalCalorias) : 0;
        int diasEnMeta = conRegistro.Count(r => r.PorcentajeCumplimiento is >= 90 and <= 110);
        float cambioPeso = pesos.Count >= 2 ? pesos.Last().Peso - pesos.First().Peso : 0;

        return Document.Create(doc => doc.Page(page =>
        {
            ConfigurarPagina(page);
            Encabezado(page, "Reporte de progreso nutricional",
                $"{usuario.Nombre} · {desde.ToString("dd MMM yyyy", Es)} – {hoy.ToString("dd MMM yyyy", Es)}");

            page.Content().PaddingVertical(10).Column(col =>
            {
                col.Spacing(12);

                col.Item().Text("Perfil").Bold().FontSize(13);
                col.Item().Text($"Edad: {usuario.Edad} años   Peso: {usuario.Peso:0.#} kg   Altura: {usuario.Altura:0} cm   Actividad: {usuario.NivelActividad}");
                col.Item().Text($"Meta diaria: {macros.CaloriasMeta:0} kcal · Proteínas {macros.ProteinasGramos:0} g · Carbohidratos {macros.CarbosGramos:0} g · Grasas {macros.GrasasGramos:0} g");

                col.Item().Row(row =>
                {
                    Indicador(row, "Días registrados", $"{conRegistro.Count} / {dias}");
                    Indicador(row, "Promedio diario", $"{promCal:0} kcal");
                    Indicador(row, "Días en meta (±10%)", $"{diasEnMeta}");
                    Indicador(row, "Cambio de peso", $"{cambioPeso:+0.0;-0.0;0} kg");
                });

                col.Item().Text("Consumo diario").Bold().FontSize(13);
                if (conRegistro.Count == 0)
                {
                    col.Item().Text("No hay comidas registradas en este periodo.").Italic();
                }
                else
                {
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn();
                            c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                        });
                        t.Header(h =>
                        {
                            foreach (var titulo in new[] { "Fecha", "Kcal", "Prot (g)", "Carb (g)", "Grasa (g)", "% meta" })
                                h.Cell().Element(CeldaHeader).Text(titulo);
                        });
                        foreach (var r in conRegistro)
                        {
                            t.Cell().Element(Celda).Text(r.Fecha.ToString("ddd dd/MM", Es));
                            t.Cell().Element(Celda).Text($"{r.TotalCalorias:0}");
                            t.Cell().Element(Celda).Text($"{r.TotalProteinas:0.#}");
                            t.Cell().Element(Celda).Text($"{r.TotalCarbos:0.#}");
                            t.Cell().Element(Celda).Text($"{r.TotalGrasas:0.#}");
                            t.Cell().Element(Celda).Text($"{r.PorcentajeCumplimiento:0}%");
                        }
                    });
                }

                col.Item().Text("Historial de peso").Bold().FontSize(13);
                if (pesos.Count == 0)
                {
                    col.Item().Text("Sin registros de peso en este periodo.").Italic();
                }
                else
                {
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(2); });
                        t.Header(h =>
                        {
                            h.Cell().Element(CeldaHeader).Text("Fecha");
                            h.Cell().Element(CeldaHeader).Text("Peso (kg)");
                            h.Cell().Element(CeldaHeader).Text("Notas");
                        });
                        foreach (var p in pesos)
                        {
                            t.Cell().Element(Celda).Text(p.Fecha.ToString("dd/MM/yyyy", Es));
                            t.Cell().Element(Celda).Text($"{p.Peso:0.0}");
                            t.Cell().Element(Celda).Text(p.Notas ?? "—");
                        }
                    });
                }
            });

            PiePagina(page);
        })).GeneratePdf();
    }

    public byte[] GenerarReportePlanes(int usuarioId)
    {
        var usuario = _facade.ObtenerPerfil(usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado");
        var planes = _facade.ObtenerPlanesPaciente(usuarioId)
            .OrderByDescending(p => p.Activo).ThenByDescending(p => p.FechaCreacion).ToList();
        var observaciones = _facade.ObtenerObservacionesPaciente(usuarioId)
            .OrderByDescending(o => o.Fecha).ToList();
        var nutriologo = _facade.ObtenerNutriologoDelUsuario(usuarioId);

        return Document.Create(doc => doc.Page(page =>
        {
            ConfigurarPagina(page);
            Encabezado(page, "Planes alimenticios y observaciones",
                $"{usuario.Nombre} · Nutriólogo: {nutriologo?.Nombre ?? "sin asignar"}");

            page.Content().PaddingVertical(10).Column(col =>
            {
                col.Spacing(12);

                if (planes.Count == 0)
                    col.Item().Text("Aún no tienes planes alimenticios asignados.").Italic();

                foreach (var plan in planes)
                {
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(pc =>
                    {
                        pc.Spacing(6);
                        pc.Item().Row(r =>
                        {
                            r.RelativeItem().Text(plan.Nombre).Bold().FontSize(13);
                            r.AutoItem().Text(plan.Activo ? "ACTIVO" : "Inactivo")
                                .FontColor(plan.Activo ? Verde : Colors.Grey.Medium).Bold();
                        });
                        pc.Item().Text($"Creado el {plan.FechaCreacion.ToString("dd 'de' MMMM yyyy", Es)} · Total: {plan.Comidas.Sum(c => c.Calorias):0} kcal")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        pc.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(3); c.RelativeColumn(); });
                            t.Header(h =>
                            {
                                h.Cell().Element(CeldaHeader).Text("Momento");
                                h.Cell().Element(CeldaHeader).Text("Descripción");
                                h.Cell().Element(CeldaHeader).Text("Kcal");
                            });
                            foreach (var c in plan.Comidas)
                            {
                                t.Cell().Element(Celda).Text(c.Momento);
                                t.Cell().Element(Celda).Text(c.Descripcion);
                                t.Cell().Element(Celda).Text($"{c.Calorias:0}");
                            }
                        });
                    });
                }

                col.Item().PaddingTop(6).Text("Observaciones del nutriólogo").Bold().FontSize(13);
                if (observaciones.Count == 0)
                    col.Item().Text("Sin observaciones.").Italic();
                foreach (var o in observaciones)
                {
                    col.Item().Column(oc =>
                    {
                        oc.Item().Text(o.Fecha.ToString("dd/MM/yyyy HH:mm", Es)).FontSize(9).FontColor(Colors.Grey.Darken1);
                        oc.Item().Text(o.Texto);
                    });
                }
            });

            PiePagina(page);
        })).GeneratePdf();
    }

    // ---------- Helpers de estilo ----------

    private static void ConfigurarPagina(PageDescriptor page)
    {
        page.Size(PageSizes.Letter);
        page.Margin(36);
        page.DefaultTextStyle(t => t.FontSize(10));
    }

    private static void Encabezado(PageDescriptor page, string titulo, string subtitulo)
    {
        page.Header().BorderBottom(2).BorderColor(Verde).PaddingBottom(6).Column(c =>
        {
            c.Item().Text("CaloriX").Bold().FontSize(18).FontColor(Verde);
            c.Item().Text(titulo).SemiBold().FontSize(14);
            c.Item().Text(subtitulo).FontSize(9).FontColor(Colors.Grey.Darken1);
        });
    }

    private static void PiePagina(PageDescriptor page)
    {
        page.Footer().AlignCenter().Text(t =>
        {
            t.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Medium));
            t.Span($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm", Es)} · Página ");
            t.CurrentPageNumber();
            t.Span(" de ");
            t.TotalPages();
        });
    }

    private static void Indicador(RowDescriptor row, string etiqueta, string valor)
    {
        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
        {
            c.Item().Text(etiqueta).FontSize(8).FontColor(Colors.Grey.Darken1);
            c.Item().Text(valor).Bold().FontSize(13);
        });
    }

    private static IContainer CeldaHeader(IContainer c) =>
        c.Background(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(s => s.SemiBold());

    private static IContainer Celda(IContainer c) =>
        c.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4);
}
