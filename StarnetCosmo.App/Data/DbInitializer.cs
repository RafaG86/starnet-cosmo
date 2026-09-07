using Microsoft.EntityFrameworkCore;
using StarnetCosmo.Core.Data;

namespace StarnetCosmo.App.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(StarnetDbContext context)
    {
        // Asegura que la base de datos y tablas base existan
        await context.Database.EnsureCreatedAsync();

        // Asegurar que la tabla CalificacionesTondm e índices optimizados existan
        const string createTondmTableSql = @"
CREATE TABLE IF NOT EXISTS ""CalificacionesTondm"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_CalificacionesTondm"" PRIMARY KEY AUTOINCREMENT,
    ""LeadId"" INTEGER NOT NULL,
    ""UrgenciaImplementacion"" TEXT NULL,
    ""FechaTentativaImplementacion"" TEXT NULL,
    ""TieneEventoProximo"" INTEGER NOT NULL DEFAULT 0,
    ""CantidadMiembros"" INTEGER NULL,
    ""NumeroSedes"" INTEGER NOT NULL DEFAULT 1,
    ""TieneCelulas"" INTEGER NOT NULL DEFAULT 0,
    ""CantidadCelulas"" INTEGER NULL,
    ""TieneEscuelaFormacion"" INTEGER NOT NULL DEFAULT 0,
    ""HerramientaActual"" TEXT NULL,
    ""ProcesosPorFuera"" TEXT NULL,
    ""DolorPrincipal"" TEXT NULL,
    ""NivelDolor"" TEXT NULL,
    ""DetalleNecesidad"" TEXT NULL,
    ""NombreDecisor"" TEXT NULL,
    ""CargoDecisor"" TEXT NULL,
    ""DecisorPresenteEnLlamada"" INTEGER NOT NULL DEFAULT 0,
    ""ProcesoDecision"" TEXT NULL,
    ""PresupuestoEstimado"" TEXT NULL,
    ""Moneda"" TEXT NULL DEFAULT 'COP',
    ""DisposicionInversion"" TEXT NULL,
    ""FechaLlamada"" TEXT NOT NULL,
    ""CalificadoPor"" TEXT NULL,
    ""ResultadoLlamada"" TEXT NULL,
    ""NotasLlamada"" TEXT NULL,
    ""CalificacionCompletada"" INTEGER NOT NULL DEFAULT 1,
    ""FechaActualizacion"" TEXT NOT NULL,
    CONSTRAINT ""FK_CalificacionesTondm_Leads_LeadId"" FOREIGN KEY (""LeadId"") REFERENCES ""Leads"" (""Id"") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CalificacionesTondm_LeadId"" ON ""CalificacionesTondm"" (""LeadId"");
CREATE INDEX IF NOT EXISTS ""IX_CalificacionesTondm_FechaLlamada"" ON ""CalificacionesTondm"" (""FechaLlamada"");
CREATE INDEX IF NOT EXISTS ""IX_Leads_Pais_Ciudad"" ON ""Leads"" (""Pais"", ""Ciudad"");
";

        await context.Database.ExecuteSqlRawAsync(createTondmTableSql);
    }
}
