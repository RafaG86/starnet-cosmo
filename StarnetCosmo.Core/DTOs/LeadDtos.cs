using StarnetCosmo.Core.Entities;

namespace StarnetCosmo.Core.DTOs;

public class LeadCreateDto
{
    public string NombreIglesia { get; set; } = string.Empty;
    public string Ciudad { get; set; } = "Colombia";
    public string Pais { get; set; } = "Colombia";
    public string? Direccion { get; set; }
    public string? SitioWeb { get; set; }
    public string? RedesSociales { get; set; }

    public string? NombreContacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool EsDecisor { get; set; } = false;

    // T-O-N-D-M
    public int? CantidadMiembros { get; set; }
    public int NumeroSedes { get; set; } = 1;
    public bool TieneCelulas { get; set; } = false;
    public bool TieneEscuelaFormacion { get; set; } = false;
    public string? HerramientaActual { get; set; }
    public string? ProcesosPorFuera { get; set; }
    public string? DolorPrincipal { get; set; }
    public string? Prioridad { get; set; }
    public string? QuienDecide { get; set; }
    public string? FechaTentativaImplementacion { get; set; }
    public decimal? PresupuestoEstimado { get; set; }

    public ProductInterest ProductoInteres { get; set; } = ProductInterest.EkklesiApp;
    public PipelineStage EstadoPipeline { get; set; } = PipelineStage.Nuevo;
    public LeadSource Origen { get; set; } = LeadSource.Manual;
    public string? Notas { get; set; }
}

public class LeadBulkItemDto
{
    public string NombreIglesia { get; set; } = string.Empty;
    public string? Ciudad { get; set; }
    public string? Pais { get; set; } = "Colombia";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? NombreContacto { get; set; }
    public string? SitioWeb { get; set; }
    public string? RedesSociales { get; set; }
    public int? CantidadMiembros { get; set; }
    public ProductInterest ProductoInteres { get; set; } = ProductInterest.EkklesiApp;
    public LeadSource Origen { get; set; } = LeadSource.WebScraper;
    public string? Notas { get; set; }
}

public class BulkImportResultDto
{
    public int TotalProcesados { get; set; }
    public int TotalInsertados { get; set; }
    public int TotalDuplicadosOmitidos { get; set; }
    public List<string> Mensajes { get; set; } = new();
}

public class LeadUpdateStageDto
{
    public PipelineStage NuevoEstado { get; set; }
    public string? NotaCambio { get; set; }
}

public class LeadFilterDto
{
    public string? Query { get; set; }
    public PipelineStage? Estado { get; set; }
    public ProductInterest? Producto { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public int Pagina { get; set; } = 1;
    public int CantidadPorPagina { get; set; } = 50;
}

public class LeadStatsDto
{
    public int TotalLeads { get; set; }
    public Dictionary<string, int> LeadsPorEtapa { get; set; } = new();
    public Dictionary<string, int> LeadsPorProducto { get; set; } = new();
    public int TotalGanados { get; set; }
    public int TotalDemosAgendadasORealizadas { get; set; }
    public double TasaConversion { get; set; }
}

public class InteraccionCreateDto
{
    public InteractionType Tipo { get; set; } = InteractionType.WhatsApp;
    public string Titulo { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public string? MetodoEVPRC_Fase { get; set; }
    public bool Exitoso { get; set; } = true;
}

public class LeadUpdateDto
{
    public string NombreIglesia { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Pais { get; set; } = "Colombia";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? SitioWeb { get; set; }
    public string? RedesSociales { get; set; }
    public string? NombreContacto { get; set; }
    public string? CargoContacto { get; set; }
    public bool EsDecisor { get; set; } = false;
    public ProductInterest ProductoInteres { get; set; } = ProductInterest.EkklesiApp;
    public PipelineStage EstadoPipeline { get; set; } = PipelineStage.Nuevo;
    public LeadSource Origen { get; set; } = LeadSource.Manual;
    public string? Prioridad { get; set; }
    public string? Notas { get; set; }
    public string? ObjecionPrincipal { get; set; }
}

public class TondmUpsertDto
{
    // T - Tiempo
    public string? UrgenciaImplementacion { get; set; }
    public string? FechaTentativaImplementacion { get; set; }
    public bool TieneEventoProximo { get; set; } = false;

    // O - Operación & Organización
    public int? CantidadMiembros { get; set; }
    public int NumeroSedes { get; set; } = 1;
    public bool TieneCelulas { get; set; } = false;
    public int? CantidadCelulas { get; set; }
    public bool TieneEscuelaFormacion { get; set; } = false;
    public string? HerramientaActual { get; set; }
    public string? ProcesosPorFuera { get; set; }

    // N - Necesidad & Dolor
    public string? DolorPrincipal { get; set; }
    public string? NivelDolor { get; set; }
    public string? DetalleNecesidad { get; set; }

    // D - Decisor
    public string? NombreDecisor { get; set; }
    public string? CargoDecisor { get; set; }
    public bool DecisorPresenteEnLlamada { get; set; } = false;
    public string? ProcesoDecision { get; set; }

    // M - Monto & Presupuesto
    public decimal? PresupuestoEstimado { get; set; }
    public string? Moneda { get; set; } = "COP";
    public string? DisposicionInversion { get; set; }

    // Auditoría & Resultado de la llamada
    public string? CalificadoPor { get; set; }
    public string? ResultadoLlamada { get; set; }
    public string? NotasLlamada { get; set; }
    public bool CalificacionCompletada { get; set; } = true;
}

