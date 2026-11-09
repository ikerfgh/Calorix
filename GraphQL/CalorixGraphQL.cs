using Calorix.Models;
using Calorix.Patterns.Facade;
using HotChocolate;

namespace Calorix.GraphQL;

/// <summary>
/// API GraphQL de Calorix (HotChocolate), expuesta en /graphql.
/// Reutiliza la misma fachada que los controladores MVC, así que respeta las mismas reglas de negocio.
/// Las consultas personales usan la sesión del usuario que inició sesión en la app.
/// </summary>
public class Query
{
    /// <summary>Catálogo de alimentos (público). Filtra por nombre o categoría.</summary>
    public List<Alimento> Alimentos(string? buscar, [Service] ICalorixFacade facade) =>
        facade.BuscarAlimentos(buscar ?? "");

    public Alimento? Alimento(int id, [Service] ICalorixFacade facade) => facade.ObtenerAlimento(id);

    /// <summary>Perfil del usuario en sesión (sin datos sensibles).</summary>
    public PerfilDto Perfil([Service] ICalorixFacade facade, [Service] IHttpContextAccessor http)
    {
        var u = facade.ObtenerPerfil(Sesion.UsuarioId(http))!;
        return new PerfilDto(u.Id, u.Nombre, u.Email, u.Edad, u.Peso, u.Altura, u.Genero, u.NivelActividad, u.MetaCalorica);
    }

    /// <summary>Resumen nutricional de un día (por defecto hoy).</summary>
    public ResumenDiario ResumenDiario(DateTime? fecha, [Service] ICalorixFacade facade, [Service] IHttpContextAccessor http) =>
        facade.ObtenerResumenDiario(Sesion.UsuarioId(http), (fecha ?? DateTime.Today).Date);

    public MacrosRecomendados MacrosRecomendados(string formula, [Service] ICalorixFacade facade, [Service] IHttpContextAccessor http) =>
        facade.ObtenerMacrosRecomendados(Sesion.UsuarioId(http), formula);

    public List<RegistroPeso> HistorialPeso(int dias, [Service] ICalorixFacade facade, [Service] IHttpContextAccessor http) =>
        facade.ObtenerHistorialPeso(Sesion.UsuarioId(http), Math.Clamp(dias, 1, 365));

    public List<PlanAlimenticio> MisPlanes([Service] ICalorixFacade facade, [Service] IHttpContextAccessor http) =>
        facade.ObtenerPlanesPaciente(Sesion.UsuarioId(http));
}

public class Mutation
{
    /// <summary>Registra una comida del catálogo (cantidad en gramos).</summary>
    public RegistroComida RegistrarComida(int alimentoId, float cantidad, string momento,
        [Service] ICalorixFacade facade, [Service] IHttpContextAccessor http)
    {
        if (cantidad <= 0 || cantidad > 5000)
            throw new GraphQLException("La cantidad debe estar entre 1 y 5000 g.");
        if (facade.ObtenerAlimento(alimentoId) is null)
            throw new GraphQLException("El alimento no existe.");
        return facade.RegistrarComida(Sesion.UsuarioId(http), alimentoId, cantidad, momento);
    }

    public bool EliminarRegistro(int registroId, [Service] ICalorixFacade facade, [Service] IHttpContextAccessor http) =>
        facade.EliminarRegistro(registroId, Sesion.UsuarioId(http));

    public RegistroPeso RegistrarPeso(float peso, string? notas,
        [Service] ICalorixFacade facade, [Service] IHttpContextAccessor http)
    {
        if (peso is < 20 or > 400) throw new GraphQLException("Peso fuera de rango (20–400 kg).");
        return facade.RegistrarPeso(Sesion.UsuarioId(http), peso, notas);
    }
}

public record PerfilDto(int Id, string Nombre, string Email, int Edad, float Peso, float Altura,
    string Genero, string NivelActividad, float MetaCalorica);

internal static class Sesion
{
    /// <summary>Obtiene el Id del paciente en sesión o lanza un error GraphQL.</summary>
    public static int UsuarioId(IHttpContextAccessor http)
    {
        var s = http.HttpContext?.Session;
        if (s?.GetString("UserRole") == "Usuario" && int.TryParse(s.GetString("UserId"), out var id))
            return id;
        throw new GraphQLException(ErrorBuilder.New()
            .SetMessage("No autenticado: inicia sesión como usuario en Calorix.")
            .SetCode("NO_AUTENTICADO").Build());
    }
}
