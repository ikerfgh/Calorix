using Microsoft.Playwright;

namespace Calorix.UITests;

/// <summary>
/// Robot de pruebas de interfaz (Playwright) que sigue la Guía de Pruebas.
/// Requisitos:
///   1) La app corriendo:  dotnet run --urls http://localhost:5080   (desde la raíz)
///   2) Navegador instalado una vez: pwsh bin/Debug/net9.0/playwright.ps1 install chromium
/// Variables opcionales: CALORIX_URL (default http://localhost:5080), HEADED=1 para ver el navegador.
/// </summary>
public class RobotUiTests : IAsyncLifetime
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("CALORIX_URL") ?? "http://localhost:5080";

    private IPlaywright _pw = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        _pw = await Playwright.CreateAsync();
        _browser = await _pw.Chromium.LaunchAsync(new()
        {
            Headless = Environment.GetEnvironmentVariable("HEADED") != "1",
            SlowMo = Environment.GetEnvironmentVariable("HEADED") == "1" ? 400 : 0
        });
        var ctx = await _browser.NewContextAsync(new() { AcceptDownloads = true });
        _page = await ctx.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _pw.Dispose();
    }

    private async Task LoginAsync(string email, string password = "demo123")
    {
        await _page.GotoAsync($"{BaseUrl}/Home/Login");
        await _page.FillAsync("#email", email);
        await _page.FillAsync("#password", password);
        await _page.ClickAsync("button[type=submit]");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact(DisplayName = "UI-01 Login válido de usuario lleva al Dashboard")]
    public async Task Login_Valido()
    {
        await LoginAsync("maria@demo.com");
        Assert.Contains("/Seguimiento", _page.Url);
        Assert.True(await _page.Locator(".page-title", new() { HasTextString = "Dashboard" }).IsVisibleAsync());
    }

    [Fact(DisplayName = "UI-02 Login inválido muestra mensaje de error")]
    public async Task Login_Invalido()
    {
        await LoginAsync("maria@demo.com", "incorrecta");
        Assert.True(await _page.Locator(".alert-error").IsVisibleAsync());
        Assert.Contains("/Home/Login", _page.Url);
    }

    [Fact(DisplayName = "UI-03 Página protegida sin sesión redirige al login")]
    public async Task SinSesion_Redirige()
    {
        await _page.GotoAsync($"{BaseUrl}/Seguimiento");
        Assert.Contains("/Home/Login", _page.Url);
    }

    [Theory(DisplayName = "UI-04 El menú del usuario abre cada pantalla")]
    [InlineData("/Seguimiento")]
    [InlineData("/Alimentos")]
    [InlineData("/Registro")]
    [InlineData("/MisPlanes")]
    [InlineData("/Perfil")]
    public async Task Menu_Usuario(string ruta)
    {
        await LoginAsync("maria@demo.com");
        await _page.ClickAsync($"a.nav-item[href='{ruta}']");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.EndsWith(ruta, new Uri(_page.Url).AbsolutePath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await _page.Locator("text=An unhandled exception").CountAsync());
    }

    [Fact(DisplayName = "UI-05 Descargar reporte PDF de progreso")]
    public async Task Descargar_ReporteProgreso()
    {
        await LoginAsync("maria@demo.com");
        var descarga = await _page.RunAndWaitForDownloadAsync(() => _page.ClickAsync("#btnReporteProgreso"));
        Assert.EndsWith(".pdf", descarga.SuggestedFilename);
        Assert.StartsWith("Calorix_Progreso_", descarga.SuggestedFilename);
    }

    [Fact(DisplayName = "UI-06 Descargar reporte PDF de planes")]
    public async Task Descargar_ReportePlanes()
    {
        await LoginAsync("maria@demo.com");
        await _page.GotoAsync($"{BaseUrl}/MisPlanes");
        var descarga = await _page.RunAndWaitForDownloadAsync(() => _page.ClickAsync("#btnReportePlanes"));
        Assert.StartsWith("Calorix_Planes_", descarga.SuggestedFilename);
    }

    [Fact(DisplayName = "UI-07 Nutriólogo entra a su panel de pacientes")]
    public async Task Login_Nutriologo()
    {
        await LoginAsync("carlos@demo.com");
        await _page.GotoAsync($"{BaseUrl}/Pacientes");
        Assert.Contains("/Pacientes", _page.Url);
    }

    [Fact(DisplayName = "UI-08 Cerrar sesión regresa al login")]
    public async Task Logout()
    {
        await LoginAsync("maria@demo.com");
        await _page.ClickAsync("a.logout-btn");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/Home/Login", _page.Url);
    }
}
