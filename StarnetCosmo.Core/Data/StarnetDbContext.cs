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
    public DbSet<CalificacionTondm> CalificacionesTondm => Set<CalificacionTondm>();

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

            entity.HasIndex(e => new { e.Pais, e.Ciudad })
                  .HasDatabaseName("IX_Leads_Pais_Ciudad");

            entity.HasMany(e => e.Interacciones)
                  .WithOne(i => i.Lead)
                  .HasForeignKey(i => i.LeadId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CalificacionTondm)
                  .WithOne(c => c.Lead)
                  .HasForeignKey<CalificacionTondm>(c => c.LeadId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Interaccion>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.LeadId);
            entity.HasIndex(i => i.Fecha);
        });

        modelBuilder.Entity<CalificacionTondm>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.LeadId).IsUnique();
            entity.HasIndex(c => c.FechaLlamada);
        });
    }
}
