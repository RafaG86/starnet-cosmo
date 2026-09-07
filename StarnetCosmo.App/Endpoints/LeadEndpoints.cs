using Microsoft.AspNetCore.Mvc;
using StarnetCosmo.Core.DTOs;
using StarnetCosmo.Core.Entities;
using StarnetCosmo.Core.Services;

namespace StarnetCosmo.App.Endpoints;

public static class LeadEndpoints
{
    public static void MapLeadEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/leads").WithTags("Leads");

        // Listar leads con filtros
        group.MapGet("/", async (
            [FromQuery] string? query,
            [FromQuery] PipelineStage? estado,
            [FromQuery] ProductInterest? producto,
            [FromQuery] string? ciudad,
            [FromQuery] string? pais,
            [FromQuery] int? pagina,
            [FromQuery] int? cantidadPorPagina,
            [FromServices] ILeadService leadService) =>
        {
            var filter = new LeadFilterDto
            {
                Query = query,
                Estado = estado,
                Producto = producto,
                Ciudad = ciudad,
                Pais = pais,
                Pagina = !pagina.HasValue || pagina.Value <= 0 ? 1 : pagina.Value,
                CantidadPorPagina = !cantidadPorPagina.HasValue || cantidadPorPagina.Value <= 0 ? 100 : cantidadPorPagina.Value
            };

            var leads = await leadService.GetLeadsAsync(filter);
            return Results.Ok(leads);
        })
        .WithName("GetLeads")
        .WithSummary("Consulta y filtra leads con búsqueda textual, estado y producto");

        // Obtener un lead por Id
        group.MapGet("/{id:int}", async (int id, [FromServices] ILeadService leadService) =>
        {
            var lead = await leadService.GetLeadByIdAsync(id);
            return lead != null ? Results.Ok(lead) : Results.NotFound(new { message = $"Lead con ID {id} no encontrado." });
        })
        .WithName("GetLeadById")
        .WithSummary("Obtiene el detalle completo de un lead y su historial");

        // Crear un lead individual
        group.MapPost("/", async ([FromBody] LeadCreateDto dto, [FromServices] ILeadService leadService) =>
        {
            var result = await leadService.CreateLeadAsync(dto);
            if (!result.Success)
            {
                return Results.BadRequest(new { message = result.Message });
            }
            return Results.Created($"/api/leads/{result.Lead!.Id}", result.Lead);
        })
        .WithName("CreateLead")
        .WithSummary("Registra un lead individual con validación anti-duplicados");

        // Ingesta masiva desde scrapers / automatizaciones
        group.MapPost("/bulk", async ([FromBody] List<LeadBulkItemDto> leads, [FromServices] ILeadService leadService) =>
        {
            if (leads == null || leads.Count == 0)
            {
                return Results.BadRequest(new { message = "La lista de prospectos no puede estar vacía." });
            }

            var result = await leadService.ImportBulkLeadsAsync(leads);
            return Results.Ok(result);
        })
        .WithName("ImportBulkLeads")
        .WithSummary("Ingesta masiva para scrapers (Google Maps, redes sociales) con deduplicación automática");

        // Actualizar etapa del pipeline
        group.MapPut("/{id:int}/stage", async (int id, [FromBody] LeadUpdateStageDto dto, [FromServices] ILeadService leadService) =>
        {
            var updated = await leadService.UpdateStageAsync(id, dto);
            return updated ? Results.Ok(new { message = "Etapa actualizada correctamente." }) : Results.NotFound();
        })
        .WithName("UpdateLeadStage")
        .WithSummary("Avanza o retrocede la etapa del lead en el pipeline comercial");

        // Registrar interacción
        group.MapPost("/{id:int}/interactions", async (int id, [FromBody] InteraccionCreateDto dto, [FromServices] ILeadService leadService) =>
        {
            var interaccion = await leadService.AddInteractionAsync(id, dto);
            return interaccion != null ? Results.Ok(interaccion) : Results.NotFound();
        })
        .WithName("AddLeadInteraction")
        .WithSummary("Registra una llamada, mensaje o nota de objeción");

        // Obtener enlace directo a WhatsApp con script del manual
        group.MapGet("/{id:int}/whatsapp-link", async (int id, [FromQuery] string? script, [FromServices] ILeadService leadService) =>
        {
            var lead = await leadService.GetLeadByIdAsync(id);
            if (lead == null) return Results.NotFound();

            var link = leadService.GenerateWhatsAppLink(lead, script ?? "frio");
            return Results.Ok(new { url = link, telefono = lead.Telefono });
        })
        .WithName("GetWhatsAppLink")
        .WithSummary("Genera la URL directa a wa.me con el script del manual comercial de STARNET inyectado");

        // Obtener expediente STARNET HUB para Handoff
        group.MapGet("/{id:int}/starnet-hub-export", async (int id, [FromServices] ILeadService leadService) =>
        {
            var lead = await leadService.GetLeadByIdAsync(id);
            if (lead == null) return Results.NotFound();

            var handoffText = leadService.GenerateStarnetHubHandoff(lead);
            return Results.Ok(new { handoff = handoffText, iglesia = lead.NombreIglesia });
        })
        .WithName("GetStarnetHubExport")
        .WithSummary("Genera la ficha formal de Handoff para pegar en STARNET HUB");

        // Actualizar datos de un lead (CRUD)
        group.MapPut("/{id:int}", async (int id, [FromBody] LeadUpdateDto dto, [FromServices] ILeadService leadService) =>
        {
            var updated = await leadService.UpdateLeadAsync(id, dto);
            return updated ? Results.Ok(new { message = "Iglesia actualizada correctamente." }) : Results.NotFound();
        })
        .WithName("UpdateLead")
        .WithSummary("Actualiza los datos de la iglesia");

        // Obtener calificación T-O-N-D-M
        group.MapGet("/{id:int}/tondm", async (int id, [FromServices] ILeadService leadService) =>
        {
            var tondm = await leadService.GetTondmAsync(id);
            return tondm != null ? Results.Ok(tondm) : Results.NotFound(new { message = "No hay calificación TONDM registrada para este lead." });
        })
        .WithName("GetTondm")
        .WithSummary("Obtiene los detalles de la calificación TONDM de la llamada telefónica");

        // Guardar calificación T-O-N-D-M
        group.MapPut("/{id:int}/tondm", async (int id, [FromBody] TondmUpsertDto dto, [FromServices] ILeadService leadService) =>
        {
            try
            {
                var result = await leadService.SaveTondmAsync(id, dto);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("SaveTondm")
        .WithSummary("Guarda o actualiza la calificación TONDM realizada durante la llamada de prospección");

        // Eliminar lead
        group.MapDelete("/{id:int}", async (int id, [FromServices] ILeadService leadService) =>
        {
            var deleted = await leadService.DeleteLeadAsync(id);
            return deleted ? Results.Ok(new { message = "Lead eliminado correctamente." }) : Results.NotFound();
        })
        .WithName("DeleteLead")
        .WithSummary("Elimina un lead del sistema");

        // Métricas de resumen
        app.MapGet("/api/metrics/summary", async ([FromServices] ILeadService leadService) =>
        {
            var stats = await leadService.GetStatsAsync();
            return Results.Ok(stats);
        })
        .WithTags("Metrics")
        .WithName("GetMetricsSummary")
        .WithSummary("Devuelve el resumen de métricas y tasa de conversión del pipeline");
    }
}
