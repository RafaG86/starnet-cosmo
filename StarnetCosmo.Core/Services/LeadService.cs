using Microsoft.EntityFrameworkCore;
using StarnetCosmo.Core.Data;
using StarnetCosmo.Core.DTOs;
using StarnetCosmo.Core.Entities;
using System.Text;
using System.Text.RegularExpressions;

namespace StarnetCosmo.Core.Services;

public class LeadService : ILeadService
{
    private readonly StarnetDbContext _context;

    public LeadService(StarnetDbContext context)
    {
        _context = context;
    }

    public async Task<List<Lead>> GetLeadsAsync(LeadFilterDto filter)
    {
        var query = _context.Leads
            .Include(l => l.Interacciones)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var term = filter.Query.Trim().ToLower();
            query = query.Where(l =>
                l.NombreIglesia.ToLower().Contains(term) ||
                (l.NombreContacto != null && l.NombreContacto.ToLower().Contains(term)) ||
                (l.Ciudad != null && l.Ciudad.ToLower().Contains(term)) ||
                (l.Telefono != null && l.Telefono.Contains(term)) ||
                (l.Email != null && l.Email.ToLower().Contains(term)));
        }

        if (filter.Estado.HasValue)
        {
            query = query.Where(l => l.EstadoPipeline == filter.Estado.Value);
        }

        if (filter.Producto.HasValue)
        {
            query = query.Where(l => l.ProductoInteres == filter.Producto.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Ciudad))
        {
            query = query.Where(l => l.Ciudad.ToLower() == filter.Ciudad.Trim().ToLower());
        }

        return await query
            .OrderByDescending(l => l.FechaActualizacion)
            .Skip((filter.Pagina - 1) * filter.CantidadPorPagina)
            .Take(filter.CantidadPorPagina)
            .ToListAsync();
    }

    public async Task<Lead?> GetLeadByIdAsync(int id)
    {
        return await _context.Leads
            .Include(l => l.Interacciones.OrderByDescending(i => i.Fecha))
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<(bool Success, string Message, Lead? Lead)> CreateLeadAsync(LeadCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreIglesia))
        {
            return (false, "El nombre de la iglesia es obligatorio.", null);
        }

        string cleanPhone = CleanPhoneNumber(dto.Telefono);

        // Validación anti-duplicados por teléfono
        if (!string.IsNullOrEmpty(cleanPhone))
        {
            var existsPhone = await _context.Leads
                .AnyAsync(l => l.Telefono != null && l.Telefono.Contains(cleanPhone));

            if (existsPhone)
            {
                return (false, $"Ya existe un lead registrado con el teléfono o WhatsApp {cleanPhone}.", null);
            }
        }

        // Validación anti-duplicados por nombre y ciudad
        var churchTrim = dto.NombreIglesia.Trim().ToLower();
        var cityTrim = dto.Ciudad.Trim().ToLower();
        var existsName = await _context.Leads
            .AnyAsync(l => l.NombreIglesia.ToLower() == churchTrim && l.Ciudad.ToLower() == cityTrim);

        if (existsName)
        {
            return (false, $"Ya existe la iglesia '{dto.NombreIglesia}' en la ciudad '{dto.Ciudad}'.", null);
        }

        var lead = new Lead
        {
            NombreIglesia = dto.NombreIglesia.Trim(),
            Ciudad = string.IsNullOrWhiteSpace(dto.Ciudad) ? "Colombia" : dto.Ciudad.Trim(),
            Pais = string.IsNullOrWhiteSpace(dto.Pais) ? "Colombia" : dto.Pais.Trim(),
            Direccion = dto.Direccion?.Trim(),
            SitioWeb = dto.SitioWeb?.Trim(),
            RedesSociales = dto.RedesSociales?.Trim(),
            NombreContacto = dto.NombreContacto?.Trim(),
            CargoContacto = dto.CargoContacto?.Trim() ?? "Pastor",
            Telefono = cleanPhone,
            Email = dto.Email?.Trim().ToLower(),
            EsDecisor = dto.EsDecisor,
            CantidadMiembros = dto.CantidadMiembros,
            NumeroSedes = dto.NumeroSedes < 1 ? 1 : dto.NumeroSedes,
            TieneCelulas = dto.TieneCelulas,
            TieneEscuelaFormacion = dto.TieneEscuelaFormacion,
            HerramientaActual = dto.HerramientaActual?.Trim(),
            ProcesosPorFuera = dto.ProcesosPorFuera?.Trim(),
            DolorPrincipal = dto.DolorPrincipal?.Trim(),
            Prioridad = dto.Prioridad ?? "Media",
            QuienDecide = dto.QuienDecide?.Trim(),
            FechaTentativaImplementacion = dto.FechaTentativaImplementacion?.Trim(),
            PresupuestoEstimado = dto.PresupuestoEstimado,
            ProductoInteres = dto.ProductoInteres,
            EstadoPipeline = dto.EstadoPipeline,
            Origen = dto.Origen,
            Notas = dto.Notas?.Trim(),
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow,
            UltimaActividad = DateTime.UtcNow
        };

        lead.Interacciones.Add(new Interaccion
        {
            Tipo = InteractionType.Otro,
            Titulo = "Lead creado",
            Detalle = $"Lead ingresado en el sistema vía {dto.Origen}.",
            Fecha = DateTime.UtcNow
        });

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        return (true, "Lead creado correctamente.", lead);
    }

    public async Task<BulkImportResultDto> ImportBulkLeadsAsync(List<LeadBulkItemDto> leads)
    {
        var result = new BulkImportResultDto { TotalProcesados = leads.Count };

        // Pre-cargar teléfonos y nombres existentes para evitar N+1 queries
        var existingPhones = await _context.Leads
            .Where(l => !string.IsNullOrEmpty(l.Telefono))
            .Select(l => l.Telefono!)
            .ToListAsync();

        var existingNames = await _context.Leads
            .Select(l => (l.NombreIglesia.ToLower() + "|" + l.Ciudad.ToLower()))
            .ToListAsync();

        var phoneSet = new HashSet<string>(existingPhones.Select(CleanPhoneNumber));
        var nameSet = new HashSet<string>(existingNames);

        var newLeads = new List<Lead>();

        foreach (var item in leads)
        {
            if (string.IsNullOrWhiteSpace(item.NombreIglesia))
            {
                result.TotalDuplicadosOmitidos++;
                continue;
            }

            var cleanPhone = CleanPhoneNumber(item.Telefono);
            var city = string.IsNullOrWhiteSpace(item.Ciudad) ? "Colombia" : item.Ciudad.Trim();
            var churchKey = (item.NombreIglesia.Trim().ToLower() + "|" + city.ToLower());

            if (!string.IsNullOrEmpty(cleanPhone) && phoneSet.Contains(cleanPhone))
            {
                result.TotalDuplicadosOmitidos++;
                continue;
            }

            if (nameSet.Contains(churchKey))
            {
                result.TotalDuplicadosOmitidos++;
                continue;
            }

            var lead = new Lead
            {
                NombreIglesia = item.NombreIglesia.Trim(),
                Ciudad = city,
                Direccion = item.Direccion?.Trim(),
                Telefono = cleanPhone,
                NombreContacto = item.NombreContacto?.Trim(),
                SitioWeb = item.SitioWeb?.Trim(),
                RedesSociales = item.RedesSociales?.Trim(),
                CantidadMiembros = item.CantidadMiembros,
                ProductoInteres = item.ProductoInteres,
                Origen = item.Origen,
                Notas = item.Notas,
                EstadoPipeline = PipelineStage.Nuevo,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow,
                UltimaActividad = DateTime.UtcNow
            };

            lead.Interacciones.Add(new Interaccion
            {
                Tipo = InteractionType.Otro,
                Titulo = "Ingesta masiva / Scraper",
                Detalle = $"Importado automáticamente vía {item.Origen}.",
                Fecha = DateTime.UtcNow
            });

            newLeads.Add(lead);

            if (!string.IsNullOrEmpty(cleanPhone))
                phoneSet.Add(cleanPhone);
            nameSet.Add(churchKey);
        }

        if (newLeads.Count > 0)
        {
            _context.Leads.AddRange(newLeads);
            await _context.SaveChangesAsync();
            result.TotalInsertados = newLeads.Count;
            result.Mensajes.Add($"Se insertaron {newLeads.Count} leads nuevos satisfactoriamente.");
        }

        return result;
    }

    public async Task<bool> UpdateStageAsync(int id, LeadUpdateStageDto dto)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return false;

        var estadoAnterior = lead.EstadoPipeline;
        lead.EstadoPipeline = dto.NuevoEstado;
        lead.FechaActualizacion = DateTime.UtcNow;
        lead.UltimaActividad = DateTime.UtcNow;

        lead.Interacciones.Add(new Interaccion
        {
            Tipo = InteractionType.Otro,
            Titulo = $"Cambio de etapa: {estadoAnterior} → {dto.NuevoEstado}",
            Detalle = string.IsNullOrWhiteSpace(dto.NotaCambio) ? "Actualización de estado en el embudo comercial." : dto.NotaCambio,
            Fecha = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Interaccion?> AddInteractionAsync(int leadId, InteraccionCreateDto dto)
    {
        var lead = await _context.Leads.FindAsync(leadId);
        if (lead == null) return null;

        var interaccion = new Interaccion
        {
            LeadId = leadId,
            Tipo = dto.Tipo,
            Titulo = dto.Titulo,
            Detalle = dto.Detalle,
            MetodoEVPRC_Fase = dto.MetodoEVPRC_Fase,
            Exitoso = dto.Exitoso,
            Fecha = DateTime.UtcNow
        };

        lead.FechaActualizacion = DateTime.UtcNow;
        lead.UltimaActividad = DateTime.UtcNow;

        _context.Interacciones.Add(interaccion);
        await _context.SaveChangesAsync();

        return interaccion;
    }

    public async Task<LeadStatsDto> GetStatsAsync()
    {
        var leads = await _context.Leads.AsNoTracking().ToListAsync();

        var stats = new LeadStatsDto
        {
            TotalLeads = leads.Count,
            TotalGanados = leads.Count(l => l.EstadoPipeline == PipelineStage.Ganado),
            TotalDemosAgendadasORealizadas = leads.Count(l =>
                l.EstadoPipeline == PipelineStage.DemoAgendada ||
                l.EstadoPipeline == PipelineStage.DemoRealizada ||
                l.EstadoPipeline == PipelineStage.Cotizacion ||
                l.EstadoPipeline == PipelineStage.Negociacion ||
                l.EstadoPipeline == PipelineStage.Ganado)
        };

        foreach (PipelineStage stage in Enum.GetValues<PipelineStage>())
        {
            stats.LeadsPorEtapa[stage.ToString()] = leads.Count(l => l.EstadoPipeline == stage);
        }

        foreach (ProductInterest product in Enum.GetValues<ProductInterest>())
        {
            stats.LeadsPorProducto[product.ToString()] = leads.Count(l => l.ProductoInteres == product);
        }

        stats.TasaConversion = stats.TotalLeads > 0
            ? Math.Round(((double)stats.TotalGanados / stats.TotalLeads) * 100, 2)
            : 0;

        return stats;
    }

    public async Task<bool> DeleteLeadAsync(int id)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return false;

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync();
        return true;
    }

    public string GenerateWhatsAppLink(Lead lead, string tipoScript)
    {
        if (string.IsNullOrWhiteSpace(lead.Telefono))
            return string.Empty;

        string contactName = string.IsNullOrWhiteSpace(lead.NombreContacto) ? "Pastor" : lead.NombreContacto.Trim();
        string churchName = lead.NombreIglesia.Trim();
        string product = lead.ProductoInteres == ProductInterest.ChordSync ? "ChordSync" :
                         lead.ProductoInteres == ProductInterest.SermonSync ? "SermonSync" :
                         "EkklesiApp";

        string mensaje = tipoScript.ToLower() switch
        {
            "conocido" =>
                $"Hola Pastor {contactName}, ¿cómo está? Quería contarle que estoy trabajando con STARNET en {product}, una plataforma diseñada para ayudar a las iglesias a organizar mejor sus procesos y el cuidado de las personas. Antes de contarle mucho, quería preguntarle: ¿ustedes en {churchName} actualmente utilizan algún sistema para manejar miembros, asistencia y procesos pastorales?",

            "referido" =>
                $"Buenos días, Pastor {contactName}. Hago parte del equipo comercial de STARNET. Nos compartieron el contacto de {churchName} para presentarles {product}. Antes de enviarle información, quisiera saber: ¿cómo manejan actualmente estos procesos en su congregación?",

            "inbound" =>
                $"Hola Pastor {contactName}, mucho gusto. Recibimos su solicitud para conocer {product} de STARNET. Para orientarlo mejor y mostrarle lo que realmente puede ser útil para {churchName}, ¿podríamos hacerle un par de preguntas muy breves?",

            "followup1" =>
                $"Hola Pastor {contactName}, espero que esté muy bien. Le escribo de STARNET. ¿Tuvieron oportunidad de revisar la propuesta de {product} para {churchName} o surgió alguna duda en la que podamos apoyarles?",

            "cierre_cordial" =>
                $"Pastor {contactName}, espero que esté muy bien. Cierro por ahora este contacto para no incomodarlo. Si más adelante en {churchName} desean conocer nuestras herramientas para organización y cuidado pastoral, quedo con mucho gusto a su entera disposición.",

            _ => // "frio" por defecto
                $"Buenos días, Pastor {contactName}. Hago parte del equipo comercial de STARNET. Trabajamos con iglesias como {churchName} ayudándoles a organizar procesos como membresía, asistencia y cuidado pastoral con {product}. Quisiera hacerle una pregunta breve: ¿actualmente utilizan alguna plataforma para administrar este tipo de información?"
        };

        string rawPhone = Regex.Replace(lead.Telefono, @"[^\d]", "");
        if (rawPhone.Length == 10 && rawPhone.StartsWith("3"))
        {
            rawPhone = "57" + rawPhone; // Código Colombia
        }

        return $"https://wa.me/{rawPhone}?text={Uri.EscapeDataString(mensaje)}";
    }

    public string GenerateStarnetHubHandoff(Lead lead)
    {
        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════════════════");
        sb.AppendLine("📋 EXPEDIENTE DE ENTREGA COMERCIAL - STARNET HUB");
        sb.AppendLine("══════════════════════════════════════════════");
        sb.AppendLine($"• Iglesia / Organización: {lead.NombreIglesia}");
        sb.AppendLine($"• Ciudad / País: {lead.Ciudad}, {lead.Pais}");
        sb.AppendLine($"• Contacto Principal: {lead.NombreContacto ?? "N/D"} ({lead.CargoContacto ?? "Pastor"})");
        sb.AppendLine($"• Teléfono / WhatsApp: {lead.Telefono ?? "N/D"}");
        sb.AppendLine($"• Correo Electrónico: {lead.Email ?? "N/D"}");
        sb.AppendLine($"• Es Decisor: {(lead.EsDecisor ? "Sí" : "No (Decide: " + (lead.QuienDecide ?? "Junta/Otro") + ")")}");
        sb.AppendLine("──────────────────────────────────────────────");
        sb.AppendLine("📊 CALIFICACIÓN T-O-N-D-M:");
        sb.AppendLine($"• Tamaño: {lead.CantidadMiembros?.ToString() ?? "N/D"} miembros | {lead.NumeroSedes} sedes");
        sb.AppendLine($"• Estructura: Células: {(lead.TieneCelulas ? "Sí" : "No")} | Escuela Formación: {(lead.TieneEscuelaFormacion ? "Sí" : "No")}");
        sb.AppendLine($"• Operación Actual: {lead.HerramientaActual ?? "Excel / Manual"}");
        sb.AppendLine($"• Necesidad / Dolor: {lead.DolorPrincipal ?? "Centralizar información y asistencia"}");
        sb.AppendLine($"• Momento: {lead.FechaTentativaImplementacion ?? "Inmediato / En evaluación"}");
        sb.AppendLine("──────────────────────────────────────────────");
        sb.AppendLine("💼 CONDICIÓN COMERCIAL:");
        sb.AppendLine($"• Producto: {lead.ProductoInteres}");
        sb.AppendLine($"• Estado Actual: {lead.EstadoPipeline}");
        sb.AppendLine($"• Presupuesto Estimado: {(lead.PresupuestoEstimado.HasValue ? "$" + lead.PresupuestoEstimado.Value.ToString("N0") + " COP" : "Por definir")}");
        sb.AppendLine($"• Origen: {lead.Origen}");
        sb.AppendLine($"• Objeción / Observación: {lead.ObjecionPrincipal ?? "Ninguna registrada"}");
        sb.AppendLine($"• Notas de Venta: {lead.Notas ?? "Ninguna"}");
        sb.AppendLine("══════════════════════════════════════════════");
        return sb.ToString();
    }

    private static string CleanPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = Regex.Replace(phone, @"[^\d]", "");
        return digits;
    }
}
