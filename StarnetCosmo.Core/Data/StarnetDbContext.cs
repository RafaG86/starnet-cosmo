using Microsoft.EntityFrameworkCore;
using StarnetCosmo.Core.Entities;

namespace StarnetCosmo.Core.Data;

public class StarnetDbContext : DbContext
{
    public StarnetDbContext(DbContextOptions<StarnetDbContext> options) : base(options)
    {
    }

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Interaccion> Interacciones => Set<Interaccion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Índices de alto rendimiento para búsqueda rápida y deduplicación
            entity.HasIndex(e => e.Telefono)
                  .HasDatabaseName("IX_Leads_Telefono");

            entity.HasIndex(e => new { e.NombreIglesia, e.Ciudad })
                  .HasDatabaseName("IX_Leads_Nombre_Ciudad");

            entity.HasIndex(e => e.EstadoPipeline)
                  .HasDatabaseName("IX_Leads_EstadoPipeline");

            entity.HasIndex(e => e.FechaCreacion)
                  .HasDatabaseName("IX_Leads_FechaCreacion");

            entity.HasMany(e => e.Interacciones)
                  .WithOne(i => i.Lead)
                  .HasForeignKey(i => i.LeadId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Interaccion>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.LeadId);
            entity.HasIndex(i => i.Fecha);
        });
    }
}
