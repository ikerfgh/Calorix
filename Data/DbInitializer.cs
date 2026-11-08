using Calorix.Models;
using Calorix.Services;
using Microsoft.EntityFrameworkCore;

namespace Calorix.Data;

public static class DbInitializer
{
    public static void Seed(CalorixDbContext db)
    {
        db.Database.EnsureCreated();

        // Asegura tablas nuevas en BDs ya creadas (EnsureCreated no las añade)
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Solicitudes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NutriologoId INTEGER NOT NULL,
                UsuarioId INTEGER NOT NULL,
                Estado TEXT NOT NULL DEFAULT 'pendiente',
                Fecha TEXT NOT NULL,
                FechaRespuesta TEXT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_Solicitudes_UsuarioId_Estado ON Solicitudes(UsuarioId, Estado);
            CREATE INDEX IF NOT EXISTS IX_Solicitudes_NutriologoId_UsuarioId ON Solicitudes(NutriologoId, UsuarioId);

            CREATE TABLE IF NOT EXISTS DocumentosNutriologo (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NutriologoId INTEGER NOT NULL,
                NombreOriginal TEXT NOT NULL,
                RutaRelativa TEXT NOT NULL,
                ContentType TEXT NOT NULL DEFAULT '',
                Tamano INTEGER NOT NULL DEFAULT 0,
                FechaSubida TEXT NOT NULL,
                Estado TEXT NOT NULL DEFAULT 'pendiente'
            );
            CREATE INDEX IF NOT EXISTS IX_DocumentosNutriologo_NutriologoId ON DocumentosNutriologo(NutriologoId);
        ");

        // Añade columnas nuevas a tablas existentes (idempotente: ignora error si ya existen)
        TryAddColumn(db, "Nutriologos", "EsAdmin", "INTEGER NOT NULL DEFAULT 0");
        TryAddColumn(db, "Nutriologos", "Rechazado", "INTEGER NOT NULL DEFAULT 0");
        TryAddColumn(db, "Recetas", "CreadaPorNutriologoId", "INTEGER NULL");
        TryAddColumn(db, "ComidaPlan", "RecetaId", "INTEGER NULL");

        if (db.Alimentos.Any())
        {
            EnsureAdminNutriologo(db);
            HashearPasswordsPlanas(db);
            return;
        }

        db.Alimentos.AddRange(
            new Alimento { Nombre = "Pechuga de Pollo",    Calorias = 165,  Proteinas = 31f,  Carbohidratos = 0f,   Grasas = 3.6f, Categoria = "Proteínas" },
            new Alimento { Nombre = "Arroz Blanco Cocido", Calorias = 130,  Proteinas = 2.7f, Carbohidratos = 28f,  Grasas = 0.3f, Categoria = "Cereales" },
            new Alimento { Nombre = "Huevo Entero",        Calorias = 155,  Proteinas = 13f,  Carbohidratos = 1.1f, Grasas = 11f,  Categoria = "Proteínas" },
            new Alimento { Nombre = "Atún en Agua",        Calorias = 116,  Proteinas = 26f,  Carbohidratos = 0f,   Grasas = 1f,   Categoria = "Proteínas" },
            new Alimento { Nombre = "Leche Entera",        Calorias = 61,   Proteinas = 3.2f, Carbohidratos = 4.8f, Grasas = 3.3f, Categoria = "Lácteos" },
            new Alimento { Nombre = "Plátano",             Calorias = 89,   Proteinas = 1.1f, Carbohidratos = 23f,  Grasas = 0.3f, Categoria = "Frutas" },
            new Alimento { Nombre = "Avena",               Calorias = 389,  Proteinas = 17f,  Carbohidratos = 66f,  Grasas = 7f,   Categoria = "Cereales" },
            new Alimento { Nombre = "Brócoli",             Calorias = 34,   Proteinas = 2.8f, Carbohidratos = 7f,   Grasas = 0.4f, Categoria = "Verduras" },
            new Alimento { Nombre = "Salmón",              Calorias = 208,  Proteinas = 20f,  Carbohidratos = 0f,   Grasas = 13f,  Categoria = "Proteínas" },
            new Alimento { Nombre = "Aguacate",            Calorias = 160,  Proteinas = 2f,   Carbohidratos = 9f,   Grasas = 15f,  Categoria = "Grasas Saludables" }
        );

        db.Usuarios.AddRange(
            new Usuario { Nombre = "María García",   Email = "maria@demo.com",  Password = "demo123",
                          Edad = 28, Peso = 65,  Altura = 165, Genero = "F", NivelActividad = "Moderado",  MetaCalorica = 1875 },
            new Usuario { Nombre = "Juan López",     Email = "juan@demo.com",   Password = "demo123",
                          Edad = 35, Peso = 80,  Altura = 178, Genero = "M", NivelActividad = "Activo",    MetaCalorica = 2680 },
            new Usuario { Nombre = "Ana Martínez",   Email = "ana@demo.com",    Password = "demo123",
                          Edad = 25, Peso = 58,  Altura = 162, Genero = "F", NivelActividad = "Ligero",    MetaCalorica = 1620 }
        );

        db.SaveChanges();

        var maria = db.Usuarios.First(u => u.Email == "maria@demo.com");
        var juan = db.Usuarios.First(u => u.Email == "juan@demo.com");
        var ana = db.Usuarios.First(u => u.Email == "ana@demo.com");

        db.Nutriologos.AddRange(
            new Nutriologo { Nombre = "Dr. Carlos Mendoza", Email = "carlos@demo.com", Password = "demo123",
                             Cedula = "NUT-001234", Especialidad = "Nutrición Clínica",   AniosExp = 10, Validado = true,
                             PacienteIds = new List<int> { maria.Id } },
            new Nutriologo { Nombre = "Dra. Laura Sánchez", Email = "laura@demo.com",  Password = "demo123",
                             Cedula = "NUT-005678", Especialidad = "Nutrición Deportiva", AniosExp = 7,  Validado = true,
                             PacienteIds = new List<int> { juan.Id } },
            new Nutriologo { Nombre = "Admin Calorix",      Email = "admin@calorix.com", Password = "admin123",
                             Cedula = "ADMIN",      Especialidad = "Administración",      AniosExp = 0, Validado = true, EsAdmin = true,
                             PacienteIds = new List<int>() }
        );

        var pollo = db.Alimentos.First(a => a.Nombre == "Pechuga de Pollo");
        var arroz = db.Alimentos.First(a => a.Nombre == "Arroz Blanco Cocido");
        var huevo = db.Alimentos.First(a => a.Nombre == "Huevo Entero");
        var leche = db.Alimentos.First(a => a.Nombre == "Leche Entera");
        var platano = db.Alimentos.First(a => a.Nombre == "Plátano");
        var avena = db.Alimentos.First(a => a.Nombre == "Avena");
        var brocoli = db.Alimentos.First(a => a.Nombre == "Brócoli");

        db.Recetas.AddRange(
            new Receta {
                Nombre = "Ensalada Proteica",
                Descripcion = "Bowl alto en proteínas con pollo, brócoli y huevo",
                Ingredientes = new() {
                    new IngredienteReceta { AlimentoId = pollo.Id, Cantidad = 150 },
                    new IngredienteReceta { AlimentoId = brocoli.Id, Cantidad = 100 },
                    new IngredienteReceta { AlimentoId = huevo.Id, Cantidad = 60 }
                },
                CaloriasTotal = MathF.Round(165*1.5f + 34*1f + 155*0.6f),
                ProteinasTotal = MathF.Round(31*1.5f + 2.8f*1f + 13*0.6f),
                CarbosTotal = MathF.Round(0*1.5f + 7*1f + 1.1f*0.6f),
                GrasasTotal = MathF.Round(3.6f*1.5f + 0.4f*1f + 11*0.6f),
                UsuarioId = maria.Id, FechaCreacion = DateTime.Now.AddDays(-7)
            },
            new Receta {
                Nombre = "Bowl de Avena con Fruta",
                Descripcion = "Desayuno energético con avena, plátano y leche",
                Ingredientes = new() {
                    new IngredienteReceta { AlimentoId = avena.Id, Cantidad = 80 },
                    new IngredienteReceta { AlimentoId = platano.Id, Cantidad = 120 },
                    new IngredienteReceta { AlimentoId = leche.Id, Cantidad = 200 }
                },
                CaloriasTotal = MathF.Round(389*0.8f + 89*1.2f + 61*2f),
                ProteinasTotal = MathF.Round(17*0.8f + 1.1f*1.2f + 3.2f*2f),
                CarbosTotal = MathF.Round(66*0.8f + 23*1.2f + 4.8f*2f),
                GrasasTotal = MathF.Round(7*0.8f + 0.3f*1.2f + 3.3f*2f),
                UsuarioId = maria.Id, FechaCreacion = DateTime.Now.AddDays(-3)
            }
        );

        var hoy = DateTime.Today;
        db.RegistrosComida.AddRange(
            new RegistroComida { UsuarioId = maria.Id, AlimentoId = huevo.Id, NombreAlimento = "Huevo Entero",
                                 Cantidad = 120, Momento = "Desayuno", Fecha = hoy,
                                 Calorias = MathF.Round(155*1.2f), Proteinas = MathF.Round(13*1.2f), Carbohidratos = MathF.Round(1.1f*1.2f), Grasas = MathF.Round(11*1.2f) },
            new RegistroComida { UsuarioId = maria.Id, AlimentoId = avena.Id, NombreAlimento = "Avena",
                                 Cantidad = 60, Momento = "Desayuno", Fecha = hoy,
                                 Calorias = MathF.Round(389*0.6f), Proteinas = MathF.Round(17*0.6f), Carbohidratos = MathF.Round(66*0.6f), Grasas = MathF.Round(7*0.6f) },
            new RegistroComida { UsuarioId = maria.Id, AlimentoId = pollo.Id, NombreAlimento = "Pechuga de Pollo",
                                 Cantidad = 200, Momento = "Comida", Fecha = hoy,
                                 Calorias = MathF.Round(165*2f), Proteinas = MathF.Round(31*2f), Carbohidratos = 0, Grasas = MathF.Round(3.6f*2f) },
            new RegistroComida { UsuarioId = maria.Id, AlimentoId = arroz.Id, NombreAlimento = "Arroz Blanco Cocido",
                                 Cantidad = 150, Momento = "Comida", Fecha = hoy,
                                 Calorias = MathF.Round(130*1.5f), Proteinas = MathF.Round(2.7f*1.5f), Carbohidratos = MathF.Round(28*1.5f), Grasas = MathF.Round(0.3f*1.5f) }
        );

        db.SaveChanges();
        HashearPasswordsPlanas(db);
    }

    private static void TryAddColumn(CalorixDbContext db, string table, string column, string definition)
    {
        try
        {
            db.Database.ExecuteSqlRaw($"ALTER TABLE {table} ADD COLUMN {column} {definition};");
        }
        catch
        {
            // La columna ya existe — operación idempotente.
        }
    }

    /// <summary>
    /// Migración: convierte a hash PBKDF2 cualquier contraseña que siga en texto plano
    /// (cuentas demo y registros anteriores a esta versión). Es idempotente.
    /// </summary>
    private static void HashearPasswordsPlanas(CalorixDbContext db)
    {
        int cambios = 0;
        foreach (var u in db.Usuarios.ToList().Where(u => !PasswordService.IsHashed(u.Password)))
        { u.Password = PasswordService.Hash(u.Password); cambios++; }
        foreach (var n in db.Nutriologos.ToList().Where(n => !PasswordService.IsHashed(n.Password)))
        { n.Password = PasswordService.Hash(n.Password); cambios++; }
        if (cambios > 0) db.SaveChanges();
    }

    private static void EnsureAdminNutriologo(CalorixDbContext db)
    {
        const string adminEmail = "admin@calorix.com";
        var admin = db.Nutriologos.FirstOrDefault(n => n.Email == adminEmail);
        if (admin == null)
        {
            db.Nutriologos.Add(new Nutriologo
            {
                Nombre = "Admin Calorix", Email = adminEmail, Password = "admin123",
                Cedula = "ADMIN", Especialidad = "Administración", AniosExp = 0,
                Validado = true, EsAdmin = true, PacienteIds = new List<int>()
            });
            db.SaveChanges();
        }
        else if (!admin.EsAdmin)
        {
            admin.EsAdmin = true;
            db.SaveChanges();
        }
    }
}
