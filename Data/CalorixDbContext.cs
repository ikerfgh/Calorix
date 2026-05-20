using Calorix.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Calorix.Data;

public class CalorixDbContext : DbContext
{
    public CalorixDbContext(DbContextOptions<CalorixDbContext> options) : base(options) { }

    public DbSet<Alimento> Alimentos => Set<Alimento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Nutriologo> Nutriologos => Set<Nutriologo>();
    public DbSet<RegistroComida> RegistrosComida => Set<RegistroComida>();
    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<PlanAlimenticio> Planes => Set<PlanAlimenticio>();
    public DbSet<RegistroPeso> RegistrosPeso => Set<RegistroPeso>();
    public DbSet<Observacion> Observaciones => Set<Observacion>();
    public DbSet<SolicitudVinculacion> Solicitudes => Set<SolicitudVinculacion>();
    public DbSet<DocumentoNutriologo> DocumentosNutriologo => Set<DocumentoNutriologo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonOpts = new JsonSerializerOptions();

        modelBuilder.Entity<Nutriologo>()
            .Property(n => n.PacienteIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOpts),
                v => JsonSerializer.Deserialize<List<int>>(v, jsonOpts) ?? new List<int>())
            .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<int>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                v => v.ToList()));

        modelBuilder.Entity<Receta>()
            .HasMany(r => r.Ingredientes)
            .WithOne()
            .HasForeignKey(i => i.RecetaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlanAlimenticio>()
            .HasMany(p => p.Comidas)
            .WithOne()
            .HasForeignKey(c => c.PlanAlimenticioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComidaPlan>()
            .Property(c => c.AlimentoIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOpts),
                v => JsonSerializer.Deserialize<List<int>>(v, jsonOpts) ?? new List<int>())
            .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<int>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                v => v.ToList()));

        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Nutriologo>().HasIndex(n => n.Email).IsUnique();

        modelBuilder.Entity<RegistroPeso>().HasIndex(r => new { r.UsuarioId, r.Fecha });
        modelBuilder.Entity<Observacion>().HasIndex(o => o.UsuarioId);
        modelBuilder.Entity<Observacion>().HasIndex(o => o.NutriologoId);
        modelBuilder.Entity<SolicitudVinculacion>().HasIndex(s => new { s.UsuarioId, s.Estado });
        modelBuilder.Entity<SolicitudVinculacion>().HasIndex(s => new { s.NutriologoId, s.UsuarioId });
        modelBuilder.Entity<DocumentoNutriologo>().HasIndex(d => d.NutriologoId);
    }
}
