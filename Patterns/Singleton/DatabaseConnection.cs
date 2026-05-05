using Calorix.Models;

namespace Calorix.Patterns.Singleton;

public sealed class DatabaseConnection
{
    private static DatabaseConnection? _instance;
    private static readonly object _lock = new();

    public List<Alimento> Alimentos { get; private set; } = new();
    public List<Usuario> Usuarios { get; private set; } = new();
    public List<Nutriologo> Nutriologos { get; private set; } = new();
    public List<RegistroComida> RegistrosComida { get; private set; } = new();
    public List<Receta> Recetas { get; private set; } = new();
    public List<PlanAlimenticio> Planes { get; private set; } = new();

    private int _nextRegistroId = 1;
    private int _nextRecetaId = 1;
    private int _nextPlanId = 1;
    private int _nextNutriologoId = 1;

    private DatabaseConnection() => SeedData();

    public static DatabaseConnection Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new DatabaseConnection();
                }
            }
            return _instance;
        }
    }

    public int GetNextRegistroId() => _nextRegistroId++;
    public int GetNextRecetaId() => _nextRecetaId++;
    public int GetNextPlanId() => _nextPlanId++;
    public int GetNextNutriologoId() => _nextNutriologoId++;

    private void SeedData()
    {
        Alimentos = new List<Alimento>
        {
            new() { Id = 1,  Nombre = "Pechuga de Pollo",    Calorias = 165,  Proteinas = 31f,  Carbohidratos = 0f,   Grasas = 3.6f, Categoria = "Proteínas" },
            new() { Id = 2,  Nombre = "Arroz Blanco Cocido", Calorias = 130,  Proteinas = 2.7f, Carbohidratos = 28f,  Grasas = 0.3f, Categoria = "Cereales" },
            new() { Id = 3,  Nombre = "Huevo Entero",        Calorias = 155,  Proteinas = 13f,  Carbohidratos = 1.1f, Grasas = 11f,  Categoria = "Proteínas" },
            new() { Id = 4,  Nombre = "Atún en Agua",        Calorias = 116,  Proteinas = 26f,  Carbohidratos = 0f,   Grasas = 1f,   Categoria = "Proteínas" },
            new() { Id = 5,  Nombre = "Leche Entera",        Calorias = 61,   Proteinas = 3.2f, Carbohidratos = 4.8f, Grasas = 3.3f, Categoria = "Lácteos" },
            new() { Id = 6,  Nombre = "Plátano",             Calorias = 89,   Proteinas = 1.1f, Carbohidratos = 23f,  Grasas = 0.3f, Categoria = "Frutas" },
            new() { Id = 7,  Nombre = "Avena",               Calorias = 389,  Proteinas = 17f,  Carbohidratos = 66f,  Grasas = 7f,   Categoria = "Cereales" },
            new() { Id = 8,  Nombre = "Brócoli",             Calorias = 34,   Proteinas = 2.8f, Carbohidratos = 7f,   Grasas = 0.4f, Categoria = "Verduras" },
            new() { Id = 9,  Nombre = "Salmón",              Calorias = 208,  Proteinas = 20f,  Carbohidratos = 0f,   Grasas = 13f,  Categoria = "Proteínas" },
            new() { Id = 10, Nombre = "Aguacate",            Calorias = 160,  Proteinas = 2f,   Carbohidratos = 9f,   Grasas = 15f,  Categoria = "Grasas Saludables" },
        };

        Usuarios = new List<Usuario>
        {
            new() { Id = 1, Nombre = "María García",   Email = "maria@demo.com",  Password = "demo123",
                    Edad = 28, Peso = 65,  Altura = 165, Genero = "F", NivelActividad = "Moderado",  MetaCalorica = 1875 },
            new() { Id = 2, Nombre = "Juan López",     Email = "juan@demo.com",   Password = "demo123",
                    Edad = 35, Peso = 80,  Altura = 178, Genero = "M", NivelActividad = "Activo",    MetaCalorica = 2680 },
            new() { Id = 3, Nombre = "Ana Martínez",   Email = "ana@demo.com",    Password = "demo123",
                    Edad = 25, Peso = 58,  Altura = 162, Genero = "F", NivelActividad = "Ligero",    MetaCalorica = 1620 },
        };

        _nextNutriologoId = 3;
        Nutriologos = new List<Nutriologo>
        {
            new() { Id = 1, Nombre = "Dr. Carlos Mendoza", Email = "carlos@demo.com", Password = "demo123",
                    Cedula = "NUT-001234", Especialidad = "Nutrición Clínica",   AniosExp = 10, Validado = true,
                    PacienteIds = new List<int> { 1, 3 } },
            new() { Id = 2, Nombre = "Dra. Laura Sánchez", Email = "laura@demo.com",  Password = "demo123",
                    Cedula = "NUT-005678", Especialidad = "Nutrición Deportiva", AniosExp = 7,  Validado = true,
                    PacienteIds = new List<int> { 2 } },
        };

        _nextRecetaId = 3;
        Recetas = new List<Receta>
        {
            new() {
                Id = 1,
                Nombre = "Ensalada Proteica",
                Descripcion = "Bowl alto en proteínas con pollo, brócoli y huevo",
                Ingredientes = new() { new() { AlimentoId = 1, Cantidad = 150 }, new() { AlimentoId = 8, Cantidad = 100 }, new() { AlimentoId = 3, Cantidad = 60 } },
                CaloriasTotal = MathF.Round(165*1.5f + 34*1f + 155*0.6f),
                ProteinasTotal = MathF.Round(31*1.5f + 2.8f*1f + 13*0.6f),
                CarbosTotal = MathF.Round(0*1.5f + 7*1f + 1.1f*0.6f),
                GrasasTotal = MathF.Round(3.6f*1.5f + 0.4f*1f + 11*0.6f),
                UsuarioId = 1, FechaCreacion = DateTime.Now.AddDays(-7)
            },
            new() {
                Id = 2,
                Nombre = "Bowl de Avena con Fruta",
                Descripcion = "Desayuno energético con avena, plátano y leche",
                Ingredientes = new() { new() { AlimentoId = 7, Cantidad = 80 }, new() { AlimentoId = 6, Cantidad = 120 }, new() { AlimentoId = 5, Cantidad = 200 } },
                CaloriasTotal = MathF.Round(389*0.8f + 89*1.2f + 61*2f),
                ProteinasTotal = MathF.Round(17*0.8f + 1.1f*1.2f + 3.2f*2f),
                CarbosTotal = MathF.Round(66*0.8f + 23*1.2f + 4.8f*2f),
                GrasasTotal = MathF.Round(7*0.8f + 0.3f*1.2f + 3.3f*2f),
                UsuarioId = 1, FechaCreacion = DateTime.Now.AddDays(-3)
            },
        };

        var hoy = DateTime.Today;
        _nextRegistroId = 5;
        RegistrosComida = new List<RegistroComida>
        {
            new() { Id = 1, UsuarioId = 1, AlimentoId = 3, NombreAlimento = "Huevo Entero",
                    Cantidad = 120, Momento = "Desayuno", Fecha = hoy,
                    Calorias = MathF.Round(155*1.2f), Proteinas = MathF.Round(13*1.2f), Carbohidratos = MathF.Round(1.1f*1.2f), Grasas = MathF.Round(11*1.2f) },
            new() { Id = 2, UsuarioId = 1, AlimentoId = 7, NombreAlimento = "Avena",
                    Cantidad = 60, Momento = "Desayuno", Fecha = hoy,
                    Calorias = MathF.Round(389*0.6f), Proteinas = MathF.Round(17*0.6f), Carbohidratos = MathF.Round(66*0.6f), Grasas = MathF.Round(7*0.6f) },
            new() { Id = 3, UsuarioId = 1, AlimentoId = 1, NombreAlimento = "Pechuga de Pollo",
                    Cantidad = 200, Momento = "Comida", Fecha = hoy,
                    Calorias = MathF.Round(165*2f), Proteinas = MathF.Round(31*2f), Carbohidratos = 0, Grasas = MathF.Round(3.6f*2f) },
            new() { Id = 4, UsuarioId = 1, AlimentoId = 2, NombreAlimento = "Arroz Blanco Cocido",
                    Cantidad = 150, Momento = "Comida", Fecha = hoy,
                    Calorias = MathF.Round(130*1.5f), Proteinas = MathF.Round(2.7f*1.5f), Carbohidratos = MathF.Round(28*1.5f), Grasas = MathF.Round(0.3f*1.5f) },
        };

        _nextPlanId = 1;
    }
}
