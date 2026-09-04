using Microsoft.EntityFrameworkCore;
using StarnetCosmo.Core.Data;

namespace StarnetCosmo.App.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(StarnetDbContext context)
    {
        // Asegura que las tablas existan sin inyectar datos ficticios
        await context.Database.EnsureCreatedAsync();
    }
}
