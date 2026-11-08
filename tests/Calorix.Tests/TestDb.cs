using Calorix.Data;
using Calorix.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Calorix.Tests;

/// <summary>
/// Base de datos SQLite en memoria, aislada por prueba, con datos mínimos conocidos.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _conn;
    public CalorixDbContext Db { get; }

    public Alimento Manzana { get; }
    public Usuario Maria { get; }
    public Nutriologo Carlos { get; }

    public TestDb()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        Db = new CalorixDbContext(new DbContextOptionsBuilder<CalorixDbContext>().UseSqlite(_conn).Options);
        Db.Database.EnsureCreated();

        Manzana = new Alimento { Nombre = "Manzana", Calorias = 52, Proteinas = 0.3f, Carbohidratos = 14, Grasas = 0.2f, Categoria = "Frutas" };
        Maria = new Usuario
        {
            Nombre = "María", Email = "maria@test.com", Password = Calorix.Services.PasswordService.Hash("demo123"),
            Edad = 28, Peso = 65, Altura = 165, Genero = "F", NivelActividad = "Moderado", MetaCalorica = 2000
        };
        Carlos = new Nutriologo
        {
            Nombre = "Dr. Carlos", Email = "carlos@test.com", Password = Calorix.Services.PasswordService.Hash("demo123"),
            Cedula = "1234567", Especialidad = "Clínica", AniosExp = 5, Validado = true
        };
        Db.Alimentos.Add(Manzana);
        Db.Usuarios.Add(Maria);
        Db.Nutriologos.Add(Carlos);
        Db.SaveChanges();
    }

    public void Dispose()
    {
        Db.Dispose();
        _conn.Dispose();
    }
}
