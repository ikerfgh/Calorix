namespace Calorix.Models;

public class Alimento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public float Calorias { get; set; }
    public float Proteinas { get; set; }
    public float Carbohidratos { get; set; }
    public float Grasas { get; set; }
    public string Categoria { get; set; } = "";
}

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public int Edad { get; set; }
    public float Peso { get; set; }
    public float Altura { get; set; }
    public string Genero { get; set; } = "";
    public string NivelActividad { get; set; } = "";
    public float MetaCalorica { get; set; }
}

public class Nutriologo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Cedula { get; set; } = "";
    public string Especialidad { get; set; } = "";
    public int AniosExp { get; set; }
    public bool Validado { get; set; }
    public List<int> PacienteIds { get; set; } = new();
}

public class RegistroComida
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int AlimentoId { get; set; }
    public string NombreAlimento { get; set; } = "";
    public float Cantidad { get; set; }
    public string UnidadCantidad { get; set; } = "g";
    public string Momento { get; set; } = "";
    public DateTime Fecha { get; set; }
    public float Calorias { get; set; }
    public float Proteinas { get; set; }
    public float Carbohidratos { get; set; }
    public float Grasas { get; set; }
}

public class IngredienteReceta
{
    public int AlimentoId { get; set; }
    public float Cantidad { get; set; }
}

public class Receta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public List<IngredienteReceta> Ingredientes { get; set; } = new();
    public float CaloriasTotal { get; set; }
    public float ProteinasTotal { get; set; }
    public float CarbosTotal { get; set; }
    public float GrasasTotal { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class ComidaPlan
{
    public string Momento { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public float Calorias { get; set; }
    public List<int> AlimentoIds { get; set; } = new();
}

public class PlanAlimenticio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int NutriologoId { get; set; }
    public int PacienteId { get; set; }
    public List<ComidaPlan> Comidas { get; set; } = new();
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}

public class ResumenDiario
{
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
    public List<RegistroComida> Registros { get; set; } = new();
    public float TotalCalorias { get; set; }
    public float TotalProteinas { get; set; }
    public float TotalCarbos { get; set; }
    public float TotalGrasas { get; set; }
    public float MetaCalorica { get; set; }
    public float PorcentajeCumplimiento { get; set; }
}

public class EstadisticasSemana
{
    public Dictionary<string, float> DatosPorDia { get; set; } = new();
    public float PromedioCaloriasDia { get; set; }
    public float TotalSemana { get; set; }
}

public class MacrosRecomendados
{
    public float CaloriasMeta { get; set; }
    public float ProteinasGramos { get; set; }
    public float CarbosGramos { get; set; }
    public float GrasasGramos { get; set; }
    public string Formula { get; set; } = "";
}
