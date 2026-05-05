using Calorix.Models;

namespace Calorix.Patterns.Strategy;

public interface ICalculoNutricionalStrategy
{
    float CalcularCaloriasBase(Usuario usuario);
    float CalcularCaloriasMeta(Usuario usuario);
    string ObtenerDescripcion();
}

public class MifflinStJeorStrategy : ICalculoNutricionalStrategy
{
    public float CalcularCaloriasBase(Usuario u)
    {
        if (u.Genero == "M")
            return 10 * u.Peso + 6.25f * u.Altura - 5 * u.Edad + 5;
        return 10 * u.Peso + 6.25f * u.Altura - 5 * u.Edad - 161;
    }

    public float CalcularCaloriasMeta(Usuario u) =>
        CalcularCaloriasBase(u) * GetActivityFactor(u.NivelActividad);

    public string ObtenerDescripcion() => "Mifflin-St Jeor (recomendada)";

    private static float GetActivityFactor(string nivel) => nivel switch
    {
        "Sedentario" => 1.2f,
        "Ligero" => 1.375f,
        "Moderado" => 1.55f,
        "Activo" => 1.725f,
        "MuyActivo" => 1.9f,
        _ => 1.55f
    };
}

public class HarrisBenedictStrategy : ICalculoNutricionalStrategy
{
    public float CalcularCaloriasBase(Usuario u)
    {
        if (u.Genero == "M")
            return 88.362f + 13.397f * u.Peso + 4.799f * u.Altura - 5.677f * u.Edad;
        return 447.593f + 9.247f * u.Peso + 3.098f * u.Altura - 4.330f * u.Edad;
    }

    public float CalcularCaloriasMeta(Usuario u) =>
        CalcularCaloriasBase(u) * GetActivityFactor(u.NivelActividad);

    public string ObtenerDescripcion() => "Harris-Benedict (clásica)";

    private static float GetActivityFactor(string nivel) => nivel switch
    {
        "Sedentario" => 1.2f,
        "Ligero" => 1.375f,
        "Moderado" => 1.55f,
        "Activo" => 1.725f,
        "MuyActivo" => 1.9f,
        _ => 1.55f
    };
}

public class CaloriaDeportivaStrategy : ICalculoNutricionalStrategy
{
    public float CalcularCaloriasBase(Usuario u)
    {
        float bodyFatPct = u.Genero == "M" ? 0.18f : 0.25f;
        float lbm = u.Peso * (1 - bodyFatPct);
        return 370 + 21.6f * lbm;
    }

    public float CalcularCaloriasMeta(Usuario u) =>
        CalcularCaloriasBase(u) * GetActivityFactor(u.NivelActividad);

    public string ObtenerDescripcion() => "Katch-McArdle Deportiva (atletas)";

    private static float GetActivityFactor(string nivel) => nivel switch
    {
        "Sedentario" => 1.2f,
        "Ligero" => 1.375f,
        "Moderado" => 1.55f,
        "Activo" => 1.725f,
        "MuyActivo" => 1.9f,
        _ => 1.55f
    };
}

public class CalculadorNutricional
{
    private ICalculoNutricionalStrategy _strategy;

    public CalculadorNutricional(ICalculoNutricionalStrategy strategy)
    {
        _strategy = strategy;
    }

    public void CambiarEstrategia(ICalculoNutricionalStrategy strategy) => _strategy = strategy;

    public float CalcularMeta(Usuario usuario) => _strategy.CalcularCaloriasMeta(usuario);

    public MacrosRecomendados CalcularMacros(Usuario usuario)
    {
        var meta = _strategy.CalcularCaloriasMeta(usuario);
        return new MacrosRecomendados
        {
            CaloriasMeta = MathF.Round(meta),
            ProteinasGramos = MathF.Round(meta * 0.30f / 4),
            CarbosGramos = MathF.Round(meta * 0.40f / 4),
            GrasasGramos = MathF.Round(meta * 0.30f / 9),
            Formula = _strategy.ObtenerDescripcion()
        };
    }

    public string ObtenerDescripcion() => _strategy.ObtenerDescripcion();
}
