using System.Text.Json;
using Microsoft.Playwright;

namespace Calorix.UITests;

/// <summary>Pruebas de la API GraphQL en /graphql (GQ-01 … GQ-05). Requiere la app corriendo.</summary>
public class GraphQLTests : IAsyncLifetime
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

    private async Task<JsonElement> GqlAsync(string query)
    {
        var resp = await _api.PostAsync("/graphql", new() { DataObject = new { query } });
        return (await resp.JsonAsync())!.Value;
    }

    private Task LoginAsync() =>
        _api.PostAsync("/Home/Login", new()
        {
            Form = _api.CreateFormData().Set("email", "maria@demo.com").Set("password", "demo123")
        });

    [Fact(DisplayName = "GQ-01 Consulta pública del catálogo de alimentos")]
    public async Task Alimentos_Publico()
    {
        var r = await GqlAsync("{ alimentos { id nombre calorias } }");
        Assert.True(r.GetProperty("data").GetProperty("alimentos").GetArrayLength() > 0);
    }

    [Fact(DisplayName = "GQ-02 Consulta privada sin sesión devuelve NO_AUTENTICADO")]
    public async Task Perfil_SinSesion()
    {
        var r = await GqlAsync("{ perfil { nombre } }");
        var err = r.GetProperty("errors")[0];
        Assert.Equal("NO_AUTENTICADO", err.GetProperty("extensions").GetProperty("code").GetString());
    }

    [Fact(DisplayName = "GQ-03 Con sesión se obtiene perfil, resumen y planes en una sola consulta")]
    public async Task Consulta_Combinada()
    {
        await LoginAsync();
        var data = (await GqlAsync("{ perfil { nombre } resumenDiario { totalCalorias } misPlanes { nombre } }"))
            .GetProperty("data");
        Assert.Equal("María García", data.GetProperty("perfil").GetProperty("nombre").GetString());
        Assert.Equal(JsonValueKind.Number, data.GetProperty("resumenDiario").GetProperty("totalCalorias").ValueKind);
        Assert.Equal(JsonValueKind.Array, data.GetProperty("misPlanes").ValueKind);
    }

    [Fact(DisplayName = "GQ-04 El esquema no expone la contraseña")]
    public async Task Esquema_SinPassword()
    {
        var r = await GqlAsync("{ __type(name:\"PerfilDto\") { fields { name } } }");
        var campos = r.GetProperty("data").GetProperty("__type").GetProperty("fields")
            .EnumerateArray().Select(f => f.GetProperty("name").GetString()).ToList();
        Assert.DoesNotContain("password", campos);
    }

    [Fact(DisplayName = "GQ-05 Mutación registrarComida valida la cantidad y luego registra")]
    public async Task Mutacion_RegistrarComida()
    {
        await LoginAsync();
        var invalida = await GqlAsync("mutation { registrarComida(alimentoId:1, cantidad:0, momento:\"Snack\") { id } }");
        Assert.True(invalida.TryGetProperty("errors", out _));

        var ok = await GqlAsync("mutation { registrarComida(alimentoId:1, cantidad:100, momento:\"Snack\") { id calorias } }");
        var id = ok.GetProperty("data").GetProperty("registrarComida").GetProperty("id").GetInt32();
        Assert.True(id > 0);

        // Limpieza: elimina el registro de prueba
        var del = await GqlAsync($"mutation {{ eliminarRegistro(registroId:{id}) }}");
        Assert.True(del.GetProperty("data").GetProperty("eliminarRegistro").GetBoolean());
    }
}
