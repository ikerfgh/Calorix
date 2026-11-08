using Calorix.Models;
using Calorix.Patterns.Facade;
using Calorix.Services;
using Calorix.Patterns.Strategy;

namespace Calorix.Tests;

/// <summary>
/// Pruebas de los casos de uso principales a través de la fachada (CU1–CU8).
/// Cada prueba indica el ID del caso de prueba del Plan de Pruebas (TC-xx).
/// </summary>
public class CalorixFacadeTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly CalorixFacade _f;

    public CalorixFacadeTests() => _f = new CalorixFacade(_t.Db);
    public void Dispose() => _t.Dispose();

    // ---------- CU7 Autenticación / perfil ----------

    [Fact(DisplayName = "TC-01 Login de usuario con credenciales válidas")]
    public void Login_Usuario_Valido()
    {
        var r = _f.Login("maria@test.com", "demo123");
        Assert.NotNull(r);
        Assert.Equal("Usuario", r!.Value.role);
        Assert.Equal(_t.Maria.Id, r.Value.id);
    }

    [Fact(DisplayName = "TC-02 Login de nutriólogo devuelve rol Nutriologo")]
    public void Login_Nutriologo()
    {
        var r = _f.Login("carlos@test.com", "demo123");
        Assert.Equal("Nutriologo", r!.Value.role);
    }

    [Theory(DisplayName = "TC-03 Login con credenciales inválidas es rechazado")]
    [InlineData("maria@test.com", "malapass")]
    [InlineData("noexiste@test.com", "demo123")]
    public void Login_Invalido(string email, string pass) =>
        Assert.Null(_f.Login(email, pass));

    [Fact(DisplayName = "TC-04 Registro de usuario calcula meta calórica")]
    public void RegistrarUsuario_CalculaMeta()
    {
        var (u, error) = _f.RegistrarUsuario("Pedro", "Pedro@Test.com ", "x1", 30, 80, 180, "M", "Moderado");
        Assert.Null(error);
        Assert.Equal("pedro@test.com", u!.Email);
        Assert.True(u.MetaCalorica > 1500);
    }

    [Fact(DisplayName = "TC-05 Registro con correo duplicado es rechazado")]
    public void RegistrarUsuario_Duplicado()
    {
        var (u, error) = _f.RegistrarUsuario("Otra", "MARIA@test.com", "x", 20, 60, 160, "F", "Ligero");
        Assert.Null(u);
        Assert.NotNull(error);
    }

    [Fact(DisplayName = "TC-06 Actualizar perfil cambia el peso")]
    public void ActualizarPerfil()
    {
        Assert.True(_f.ActualizarPerfil(_t.Maria.Id, 63, 165, 28, "Moderado"));
        Assert.Equal(63, _f.ObtenerPerfil(_t.Maria.Id)!.Peso);
    }

    // ---------- CU1 / CU2 Alimentos y registro de comidas ----------

    [Fact(DisplayName = "TC-07 Búsqueda de alimentos por nombre ignora mayúsculas")]
    public void BuscarAlimentos()
    {
        Assert.Single(_f.BuscarAlimentos("MANZ"));
        Assert.Empty(_f.BuscarAlimentos("pizza"));
    }

    [Fact(DisplayName = "TC-08 Registrar comida escala macros por cantidad (200 g)")]
    public void RegistrarComida_EscalaMacros()
    {
        var reg = _f.RegistrarComida(_t.Maria.Id, _t.Manzana.Id, 200, "Desayuno");
        Assert.Equal(104, reg.Calorias);
        Assert.Equal(28, reg.Carbohidratos);
        Assert.Equal("Manzana", reg.NombreAlimento);
    }

    [Fact(DisplayName = "TC-09 Resumen diario suma registros y calcula % de meta")]
    public void ResumenDiario()
    {
        _f.RegistrarComida(_t.Maria.Id, _t.Manzana.Id, 1000, "Comida"); // 520 kcal
        _f.RegistrarComida(_t.Maria.Id, _t.Manzana.Id, 1000, "Cena");   // 520 kcal
        var r = _f.ObtenerResumenDiario(_t.Maria.Id, DateTime.Today);
        Assert.Equal(1040, r.TotalCalorias);
        Assert.Equal(52, r.PorcentajeCumplimiento, 1);
    }

    [Fact(DisplayName = "TC-10 Solo el dueño puede eliminar su registro")]
    public void EliminarRegistro_SoloDueno()
    {
        var reg = _f.RegistrarComida(_t.Maria.Id, _t.Manzana.Id, 100, "Snack");
        Assert.False(_f.EliminarRegistro(reg.Id, usuarioId: 999));
        Assert.True(_f.EliminarRegistro(reg.Id, _t.Maria.Id));
        Assert.Empty(_f.ObtenerRegistrosDia(_t.Maria.Id, DateTime.Today));
    }

    // ---------- CU3 Seguimiento ----------

    [Fact(DisplayName = "TC-11 Registrar peso aparece en el historial")]
    public void HistorialPeso()
    {
        _f.RegistrarPeso(_t.Maria.Id, 64.5f, "semana 1");
        var h = _f.ObtenerHistorialPeso(_t.Maria.Id);
        Assert.Contains(h, p => p.Peso == 64.5f && p.Notas == "semana 1");
    }

    [Theory(DisplayName = "TC-12 Strategy: cada fórmula produce una meta distinta y positiva")]
    [InlineData("mifflin")]
    [InlineData("harris")]
    [InlineData("deportiva")]
    public void MacrosRecomendados(string formula)
    {
        var m = _f.ObtenerMacrosRecomendados(_t.Maria.Id, formula);
        Assert.True(m.CaloriasMeta > 1000);
        Assert.True(m.ProteinasGramos > 0 && m.CarbosGramos > 0 && m.GrasasGramos > 0);
    }

    [Fact(DisplayName = "TC-13 Mifflin-St Jeor coincide con la fórmula (mujer)")]
    public void Mifflin_Formula()
    {
        var bmr = new MifflinStJeorStrategy().CalcularCaloriasBase(_t.Maria);
        Assert.Equal(10 * 65 + 6.25f * 165 - 5 * 28 - 161, bmr, 1);
    }

    // ---------- CU4 / CU5 Vinculación nutriólogo–paciente ----------

    [Fact(DisplayName = "TC-14 Solicitud de vinculación aprobada agrega al paciente")]
    public void Vinculacion_Aprobada()
    {
        var (ok, _) = _f.EnviarSolicitudVinculacion(_t.Carlos.Id, _t.Maria.Id);
        Assert.True(ok);
        var pendiente = Assert.Single(_f.ObtenerSolicitudesPendientes(_t.Maria.Id));
        Assert.True(_f.ResponderSolicitud(_t.Maria.Id, pendiente.solicitud.Id, aprobar: true));
        Assert.Contains(_f.ObtenerPacientes(_t.Carlos.Id), p => p.Id == _t.Maria.Id);
        Assert.Equal(_t.Carlos.Id, _f.ObtenerNutriologoDelUsuario(_t.Maria.Id)!.Id);
    }

    [Fact(DisplayName = "TC-15 Solicitud rechazada no vincula al paciente")]
    public void Vinculacion_Rechazada()
    {
        _f.EnviarSolicitudVinculacion(_t.Carlos.Id, _t.Maria.Id);
        var s = _f.ObtenerSolicitudesPendientes(_t.Maria.Id).Single();
        Assert.True(_f.ResponderSolicitud(_t.Maria.Id, s.solicitud.Id, aprobar: false));
        Assert.Null(_f.ObtenerNutriologoDelUsuario(_t.Maria.Id));
    }

    // ---------- CU6 Planes alimenticios ----------

    [Fact(DisplayName = "TC-16 Nutriólogo crea plan y el paciente lo ve")]
    public void CrearPlan()
    {
        _f.VincularPaciente(_t.Carlos.Id, _t.Maria.Id);
        var plan = _f.CrearPlanAlimenticio(_t.Carlos.Id, _t.Maria.Id, "Plan base",
            new List<ComidaPlan> { new() { Momento = "Desayuno", Descripcion = "Avena", Calorias = 350 } });
        var planes = _f.ObtenerPlanesPaciente(_t.Maria.Id);
        Assert.Contains(planes, p => p.Id == plan.Id && p.Comidas.Count == 1);
    }

    [Fact(DisplayName = "TC-17 Otro nutriólogo no puede eliminar el plan")]
    public void EliminarPlan_SoloAutor()
    {
        _f.VincularPaciente(_t.Carlos.Id, _t.Maria.Id);
        var plan = _f.CrearPlanAlimenticio(_t.Carlos.Id, _t.Maria.Id, "Plan",
            new List<ComidaPlan> { new() { Momento = "Cena", Descripcion = "Ensalada", Calorias = 300 } });
        Assert.False(_f.EliminarPlanAlimenticio(nutriologoId: 999, plan.Id));
        Assert.True(_f.EliminarPlanAlimenticio(_t.Carlos.Id, plan.Id));
    }

    // ---------- CU8 Registro profesional ----------

    [Fact(DisplayName = "TC-18 Nutriólogo nuevo queda pendiente de validación")]
    public void RegistrarNutriologo_Pendiente()
    {
        var (n, error) = _f.RegistrarNutriologo("Dra. Ana", "ana@test.com", "x", "7654321", "Deportiva", 3);
        Assert.Null(error);
        Assert.False(n!.Validado);
        Assert.Contains(_f.ObtenerNutriologosPendientes(), p => p.Id == n.Id);
        Assert.True(_f.ValidarNutriologo(n.Id, aprobar: true));
        Assert.True(_f.ObtenerNutriologo(n.Id)!.Validado);
    }

    // ---------- Reportes PDF ----------

    [Theory(DisplayName = "TC-19 Reporte de progreso genera un PDF válido")]
    [InlineData(7)]
    [InlineData(30)]
    public void ReporteProgreso_Pdf(int dias)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        _f.RegistrarComida(_t.Maria.Id, _t.Manzana.Id, 150, "Desayuno");
        var pdf = new ReportePdfService(_f).GenerarReporteProgreso(_t.Maria.Id, dias);
        Assert.True(pdf.Length > 1000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact(DisplayName = "TC-20 Reporte de planes genera un PDF válido")]
    public void ReportePlanes_Pdf()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var pdf = new ReportePdfService(_f).GenerarReportePlanes(_t.Maria.Id);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
