using Microsoft.EntityFrameworkCore;
using StarnetCosmo.Core.Data;
using StarnetCosmo.Core.Entities;

namespace StarnetCosmo.App.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(StarnetDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Leads.AnyAsync())
        {
            return; // Ya existen datos
        }

        // Semillero con prospectos representativos alineados al manual comercial STARNET
        var seedLeads = new List<Lead>
        {
            new()
            {
                NombreIglesia = "Comunidad Cristiana El Redentor",
                Ciudad = "Bogotá",
                Pais = "Colombia",
                Direccion = "Calle 140 # 15-20, Cedritos",
                Telefono = "3105551234",
                Email = "contacto@elredentor.co",
                SitioWeb = "https://elredentor.co",
                NombreContacto = "Carlos Gómez",
                CargoContacto = "Pastor Principal",
                EsDecisor = true,
                CantidadMiembros = 320,
                NumeroSedes = 2,
                TieneCelulas = true,
                TieneEscuelaFormacion = true,
                HerramientaActual = "Excel y varios grupos de WhatsApp",
                ProcesosPorFuera = "Control de asistencia en hojas de cálculo y registros en papel",
                DolorPrincipal = "Dificultad para saber quién deja de asistir y hacer seguimiento pastoral oportuno",
                Prioridad = "Alta",
                QuienDecide = "Pastor Carlos Gómez y consejo pastoral",
                FechaTentativaImplementacion = "Próximo mes",
                PresupuestoEstimado = 450000,
                ProductoInteres = ProductInterest.EkklesiApp,
                EstadoPipeline = PipelineStage.DemoAgendada,
                Origen = LeadSource.GoogleMaps,
                Notas = "Interesado en centralizar miembros y células. Agendada demo para el martes 10:00 AM.",
                FechaCreacion = DateTime.UtcNow.AddDays(-5),
                FechaActualizacion = DateTime.UtcNow.AddHours(-3),
                UltimaActividad = DateTime.UtcNow.AddHours(-3),
                Interacciones = new List<Interaccion>
                {
                    new()
                    {
                        Tipo = InteractionType.WhatsApp,
                        Titulo = "Primer contacto WhatsApp",
                        Detalle = "Se envió script de lead frío. El pastor respondió interesado en ordenar la información de los 320 miembros.",
                        Fecha = DateTime.UtcNow.AddDays(-5)
                    },
                    new()
                    {
                        Tipo = InteractionType.Llamada,
                        Titulo = "Calificación T-O-N-D-M",
                        Detalle = "Calificado: 2 sedes, células activas, deciden pastor y consejo.",
                        Fecha = DateTime.UtcNow.AddDays(-3)
                    }
                }
            },
            new()
            {
                NombreIglesia = "Iglesia Vida & Gracia",
                Ciudad = "Medellín",
                Pais = "Colombia",
                Direccion = "Carrera 43A # 1-50, El Poblado",
                Telefono = "3004445678",
                Email = "pastoral@vidaygracia.org",
                NombreContacto = "David Valencia",
                CargoContacto = "Pastor Ejecutivo",
                EsDecisor = true,
                CantidadMiembros = 650,
                NumeroSedes = 1,
                TieneCelulas = true,
                TieneEscuelaFormacion = true,
                HerramientaActual = "Planning Center (muy costoso en dólares y en inglés)",
                ProcesosPorFuera = "Seguimiento pastoral en chats",
                DolorPrincipal = "Buscan plataforma 100% en español con soporte en Colombia y precio en COP",
                Prioridad = "Alta",
                QuienDecide = "Junta directiva",
                FechaTentativaImplementacion = "Inmediato",
                PresupuestoEstimado = 750000,
                ProductoInteres = ProductInterest.SuiteCompleta,
                EstadoPipeline = PipelineStage.Cotizacion,
                Origen = LeadSource.Referido,
                Notas = "Quieren EkklesiApp + ChordSync para su equipo de alabanza.",
                FechaCreacion = DateTime.UtcNow.AddDays(-8),
                FechaActualizacion = DateTime.UtcNow.AddDays(-1),
                UltimaActividad = DateTime.UtcNow.AddDays(-1),
                Interacciones = new List<Interaccion>
                {
                    new()
                    {
                        Tipo = InteractionType.Demo,
                        Titulo = "Demo consultiva realizada (25 min)",
                        Detalle = "Se mostró Cuidado Base e historia de una persona. Quedaron muy conformes.",
                        Fecha = DateTime.UtcNow.AddDays(-2)
                    }
                }
            },
            new()
            {
                NombreIglesia = "Centro Bíblico Manantial",
                Ciudad = "Cali",
                Pais = "Colombia",
                Direccion = "Av. Roosevelt # 34-12",
                Telefono = "3158889900",
                Email = "admin@manantialcali.org",
                NombreContacto = "Andrés Morales",
                CargoContacto = "Director de Alabanza",
                EsDecisor = false,
                CantidadMiembros = 180,
                NumeroSedes = 1,
                HerramientaActual = "Hojas de papel y carpetas de acordes",
                DolorPrincipal = "Ensayos desordenados, cada músico lleva acordes en tonos distintos",
                Prioridad = "Media",
                QuienDecide = "Pastor Principal (Héctor Suárez)",
                FechaTentativaImplementacion = "Este trimestre",
                ProductoInteres = ProductInterest.ChordSync,
                EstadoPipeline = PipelineStage.Contactado,
                Origen = LeadSource.Instagram,
                Notas = "Contacto iniciado con director de alabanza para ChordSync, luego conectar al pastor.",
                FechaCreacion = DateTime.UtcNow.AddDays(-2),
                FechaActualizacion = DateTime.UtcNow.AddHours(-12),
                UltimaActividad = DateTime.UtcNow.AddHours(-12)
            },
            new()
            {
                NombreIglesia = "Iglesia Manantial de Vida Eterna",
                Ciudad = "Barranquilla",
                Pais = "Colombia",
                Telefono = "3127773322",
                Email = "contacto@manantialbaq.org",
                NombreContacto = "Felipe Rueda",
                CargoContacto = "Pastor Principal",
                EsDecisor = true,
                CantidadMiembros = 95,
                NumeroSedes = 1,
                HerramientaActual = "Excel",
                DolorPrincipal = "Organizar consolidación de personas nuevas",
                Prioridad = "Baja",
                ProductoInteres = ProductInterest.EkklesiApp,
                EstadoPipeline = PipelineStage.Nuevo,
                Origen = LeadSource.GoogleMaps,
                Notas = "Lead nuevo capturado por scraper de Google Maps.",
                FechaCreacion = DateTime.UtcNow.AddHours(-5),
                FechaActualizacion = DateTime.UtcNow.AddHours(-5),
                UltimaActividad = DateTime.UtcNow.AddHours(-5)
            }
        };

        context.Leads.AddRange(seedLeads);
        await context.SaveChangesAsync();
    }
}
