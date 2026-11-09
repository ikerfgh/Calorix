using System.Text.Json;
using Microsoft.Playwright;

namespace Calorix.UITests;

/// <summary>Pruebas de la API REST en /api (RS-01 … RS-07). Requiere la app corriendo.</summary>
public class RestApiTests : IAsyncLifetime
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("CALORIX_URL") ?? "http://localhost:5080";

    private IPlaywright _pw = null!;
    private IAPIRequestContext _api = null!;

    public async Task InitializeAsync()
    {
        _pw = await Playwright.CreateAsync();
        _api = await _pw.APIRequest.NewContextAsync(new() { BaseURL = BaseUrl });
    }

    public async Task DisposeAsync()
    {
        await _api.DisposeAsync();
        _pw.Dispose();
    }

    private Task LoginAsync() =>
        _api.PostAsync("/Home/Login", new()
        {
            Form = _api.CreateFormData().Set("email", "maria@demo.com").Set("password", "demo123")
        });

    [Fact(DisplayName = "RS-01 GET /api/alimentos es público y devuelve el catálogo")]
    public async Task Alimentos()
    {
        var r = await _api.GetAsync("/api/alimentos");
        Assert.Equal(200, r.Status);
        Assert.True((await r.JsonAsync())!.Value.GetArrayLength() > 0);
    }

    [Fact(DisplayName = "RS-02 GET /api/alimentos/{id} inexistente devuelve 404")]
    public async Task Alimento_NoExiste()
    {
        var r = await _api.GetAsync("/api/alimentos/999999");
        Assert.Equal(404, r.Status);
    }

    [Fact(DisplayName = "RS-03 Recurso personal sin sesión devuelve 401")]
    public async Task SinSesion_401()
    {
        Assert.Equal(401, (await _api.GetAsync("/api/perfil")).Status);
        Assert.Equal(401, (await _api.GetAsync("/api/resumen")).Status);
    }

    [Fact(DisplayName = "RS-04 Con sesión GET /api/perfil devuelve el perfil sin contraseña")]
    public async Task Perfil()
    {
        await LoginAsync();
        var r = await _api.GetAsync("/api/perfil");
        Assert.Equal(200, r.Status);
        var json = (await r.JsonAsync())!.Value;
        Assert.Equal("María García", json.GetProperty("nombre").GetString());
        Assert.False(json.TryGetProperty("password", out _));
    }

    [Fact(DisplayName = "RS-05 POST /api/registros con cantidad inválida devuelve 400")]
    public async Task Registro_Invalido()
    {
        await LoginAsync();
        var r = await _api.PostAsync("/api/registros", new() { DataObject = new { alimentoId = 1, cantidad = 0, momento = "Snack" } });
        Assert.Equal(400, r.Status);
    }

    [Fact(DisplayName = "RS-06 Crear y borrar un registro: 201 y luego 204")]
    public async Task Registro_CrearYBorrar()
    {
        await LoginAsync();
        var crear = await _api.PostAsync("/api/registros", new() { DataObject = new { alimentoId = 1, cantidad = 100, momento = "Snack" } });
        Assert.Equal(201, crear.Status);
        var id = (await crear.JsonAsync())!.Value.GetProperty("id").GetInt32();

        var lista = (await (await _api.GetAsync("/api/registros")).JsonAsync())!.Value;
        Assert.Contains(lista.EnumerateArray(), x => x.GetProperty("id").GetInt32() == id);

        Assert.Equal(204, (await _api.DeleteAsync($"/api/registros/{id}")).Status);
        Assert.Equal(404, (await _api.DeleteAsync($"/api/registros/{id}")).Status);
    }

    [Fact(DisplayName = "RS-07 Swagger publica la especificación OpenAPI")]
    public async Task Swagger()
    {
        var r = await _api.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(200, r.Status);
        var paths = (await r.JsonAsync())!.Value.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/alimentos", out _));
        Assert.True(paths.TryGetProperty("/api/registros", out _));
    }
}
