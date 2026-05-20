using Calorix.Data;
using Microsoft.EntityFrameworkCore;

namespace Calorix.Patterns.Singleton;

public sealed class DatabaseConnection
{
    private static DatabaseConnection? _instance;
    private static readonly object _lock = new();

    public string ConnectionString { get; }

    private DatabaseConnection(string connectionString) => ConnectionString = connectionString;

    public static DatabaseConnection Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException(
                    "DatabaseConnection no inicializada. Llama a Initialize() en Program.cs antes de usarla.");
            return _instance;
        }
    }

    public static void Initialize(string connectionString)
    {
        if (_instance != null) return;
        lock (_lock)
        {
            _instance ??= new DatabaseConnection(connectionString);
        }
    }

    public CalorixDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CalorixDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        return new CalorixDbContext(options);
    }
}
