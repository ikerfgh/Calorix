using Calorix.Models;
using Calorix.Patterns.Facade;
using Microsoft.AspNetCore.Mvc;

namespace Calorix.Controllers.Api;

/// <summary>
/// API REST de Calorix (/api). Reutiliza la misma fachada que las vistas MVC y la API GraphQL.
/// Los recursos personales usan la sesión del usuario (cookie) y devuelven 401 si no hay sesión.
/// Documentación interactiva: /swagger
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class CalorixApiController : ControllerBase
{
    private readonly ICalorixFacade _facade;

    public CalorixApiController(ICalorixFacade facade) => _facade = facade;

    // ---------- Alimentos (público) ----------

    /// <summary>Lista el catálogo de alimentos; filtra por nombre o categoría.</summary>
    [HttpGet("alimentos")]
    public ActionResult<List<Alimento>> GetAlimentos([FromQuery] string? buscar) =>
        _facade.BuscarAlimentos(buscar ?? "");

    /// <summary>Obtiene un alimento por su id.</summary>
    [HttpGet("alimentos/{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Alimento> GetAlimento(int id) =>
        _facade.ObtenerAlimento(id) is { } a ? a : NotFound(Error("Alimento no encontrado."));

    // ---------- Recursos del usuario en sesión ----------

    /// <summary>Perfil del usuario en sesión (sin contraseña).</summary>
    [HttpGet("perfil")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<PerfilDto> GetPerfil()
    {
        if (!Sesion(out var uid)) return Unauthorized(Error("Inicia sesión como usuario."));
        var u = _facade.ObtenerPerfil(uid)!;
        return new PerfilDto(u.Id, u.Nombre, u.Email, u.Edad, u.Peso, u.Altura, u.Genero, u.NivelActividad, u.MetaCalorica);
    }

    /// <summary>Resumen nutricional del día (por defecto hoy).</summary>
    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<ResumenDiario> GetResumen([FromQuery] DateTime? fecha)
    {
        if (!Sesion(out var uid)) return Unauthorized(Error("Inicia sesión como usuario."));
        return _facade.ObtenerResumenDiario(uid, (fecha ?? DateTime.Today).Date);
    }

    /// <summary>Registros de comida del día (por defecto hoy).</summary>
    [HttpGet("registros")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<List<RegistroComida>> GetRegistros([FromQuery] DateTime? fecha)
    {
        if (!Sesion(out var uid)) return Unauthorized(Error("Inicia sesión como usuario."));
        return _facade.ObtenerRegistrosDia(uid, (fecha ?? DateTime.Today).Date);
    }

    /// <summary>Registra una comida del catálogo (cantidad en gramos).</summary>
    [HttpPost("registros")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<RegistroComida> PostRegistro([FromBody] NuevoRegistroDto dto)
    {
        if (!Sesion(out var uid)) return Unauthorized(Error("Inicia sesión como usuario."));
        if (dto.Cantidad <= 0 || dto.Cantidad > 5000) return BadRequest(Error("La cantidad debe estar entre 1 y 5000 g."));
        if (_facade.ObtenerAlimento(dto.AlimentoId) is null) return BadRequest(Error("El alimento no existe."));
        var reg = _facade.RegistrarComida(uid, dto.AlimentoId, dto.Cantidad, dto.Momento);
        return Created($"/api/registros/{reg.Id}", reg);
    }

    /// <summary>Elimina un registro propio.</summary>
    [HttpDelete("registros/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult DeleteRegistro(int id)
    {
        if (!Sesion(out var uid)) return Unauthorized(Error("Inicia sesión como usuario."));
        return _facade.EliminarRegistro(id, uid) ? NoContent() : NotFound(Error("Registro no encontrado."));
    }

    /// <summary>Planes alimenticios asignados al usuario.</summary>
    [HttpGet("planes")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<List<PlanAlimenticio>> GetPlanes()
    {
        if (!Sesion(out var uid)) return Unauthorized(Error("Inicia sesión como usuario."));
        return _facade.ObtenerPlanesPaciente(uid);
    }

    /// <summary>Historial de peso de los últimos N días (1–365).</summary>
    [HttpGet("peso")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<List<RegistroPeso>> GetPeso([FromQuery] int dias = 30)
    {
        if (!Sesion(out var uid)) return Unauthorized(Error("Inicia sesión como usuario."));
        return _facade.ObtenerHistorialPeso(uid, Math.Clamp(dias, 1, 365));
    }

    /// <summary>Registra el peso actual (20–400 kg).</summary>
    [HttpPost("peso")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<RegistroPeso> PostPeso([FromBody] NuevoPesoDto dto)
    {
        if (!Sesion(out var uid)) return Unauthorized(Error("Inicia sesión como usuario."));
        if (dto.Peso is < 20 or > 400) return BadRequest(Error("Peso fuera de rango (20–400 kg)."));
        var r = _facade.RegistrarPeso(uid, dto.Peso, dto.Notas);
        return Created($"/api/peso/{r.Id}", r);
    }

    private bool Sesion(out int userId)
    {
        userId = 0;
        var s = HttpContext.Session;
        return s.GetString("UserRole") == "Usuario" && int.TryParse(s.GetString("UserId"), out userId);
    }

    private static object Error(string mensaje) => new { error = mensaje };
}

public record PerfilDto(int Id, string Nombre, string Email, int Edad, float Peso, float Altura,
    string Genero, string NivelActividad, float MetaCalorica);
public record NuevoRegistroDto(int AlimentoId, float Cantidad, string Momento);
public record NuevoPesoDto(float Peso, string? Notas);
