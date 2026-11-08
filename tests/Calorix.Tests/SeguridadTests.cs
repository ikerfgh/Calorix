using Calorix.Patterns.Facade;
using Calorix.Services;

namespace Calorix.Tests;

/// <summary>Pruebas de seguridad: contraseñas con hash (TC-21 … TC-25).</summary>
public class SeguridadTests : IDisposable
{
    private readonly TestDb _t = new();
    private readonly CalorixFacade _f;

    public SeguridadTests() => _f = new CalorixFacade(_t.Db);
    public void Dispose() => _t.Dispose();

    [Fact(DisplayName = "TC-21 El hash no contiene la contraseña y usa sal aleatoria")]
    public void Hash_ConSal()
    {
        var h1 = PasswordService.Hash("demo123");
        var h2 = PasswordService.Hash("demo123");
        Assert.DoesNotContain("demo123", h1);
        Assert.NotEqual(h1, h2);
        Assert.True(PasswordService.IsHashed(h1));
        Assert.False(PasswordService.IsHashed("demo123"));
    }

    [Fact(DisplayName = "TC-22 Verify acepta la correcta y rechaza la incorrecta")]
    public void Verify()
    {
        var h = PasswordService.Hash("S3guro!");
        Assert.True(PasswordService.Verify(h, "S3guro!"));
        Assert.False(PasswordService.Verify(h, "s3guro!"));
        Assert.False(PasswordService.Verify("texto-plano", "texto-plano"));
    }

    [Fact(DisplayName = "TC-23 Registro de usuario guarda la contraseña con hash")]
    public void Registro_GuardaHash()
    {
        var (u, _) = _f.RegistrarUsuario("Luis", "luis@test.com", "MiClave1", 25, 70, 175, "M", "Ligero");
        Assert.NotEqual("MiClave1", u!.Password);
        Assert.True(PasswordService.IsHashed(u.Password));
        Assert.NotNull(_f.Login("luis@test.com", "MiClave1"));
    }

    [Fact(DisplayName = "TC-24 Registro de nutriólogo guarda la contraseña con hash")]
    public void RegistroNutriologo_GuardaHash()
    {
        var (n, _) = _f.RegistrarNutriologo("Dra. Sofía", "sofia@test.com", "Clave9", "1112223", "Clínica", 2);
        Assert.True(PasswordService.IsHashed(n!.Password));
        Assert.Equal("Nutriologo", _f.Login("sofia@test.com", "Clave9")!.Value.role);
    }

    [Theory(DisplayName = "TC-25 Login normaliza el correo (mayúsculas/espacios)")]
    [InlineData("MARIA@test.com")]
    [InlineData("  maria@test.com ")]
    public void Login_NormalizaEmail(string email) =>
        Assert.NotNull(_f.Login(email, "demo123"));
}
