using System.ComponentModel.DataAnnotations;

namespace StarnetCosmo.Core.Entities;

public class Lead
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string NombreIglesia { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Pais { get; set; } = "Colombia";

    [MaxLength(300)]
    public string? Direccion { get; set; }

    [MaxLength(250)]
    public string? SitioWeb { get; set; }

    [MaxLength(300)]
    public string? RedesSociales { get; set; }

    // Contacto
    [MaxLength(150)]
    public string? NombreContacto { get; set; }

    [MaxLength(100)]
    public string? CargoContacto { get; set; } // Pastor Principal, Administrador, Líder de Alabanza, etc.

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    public bool EsDecisor { get; set; } = false;

    // Metodología T-O-N-D-M (Calificación)
    public int? CantidadMiembros { get; set; }
    public int NumeroSedes { get; set; } = 1;
    public bool TieneCelulas { get; set; } = false;
    public bool TieneEscuelaFormacion { get; set; } = false;

    [MaxLength(150)]
    public string? HerramientaActual { get; set; } // Excel, WhatsApp, Planning Center, Papel, etc.

    [MaxLength(500)]
    public string? ProcesosPorFuera { get; set; }

    [MaxLength(300)]
    public string? DolorPrincipal { get; set; } // Cuidado de miembros, asistencia, formación, etc.

    [MaxLength(50)]
    public string? Prioridad { get; set; } // Alta, Media, Baja

    [MaxLength(150)]
    public string? QuienDecide { get; set; } // Pastor, Junta, Equipo administrativo

    [MaxLength(100)]
    public string? FechaTentativaImplementacion { get; set; } // Inmediato, 1 mes, 3 meses, Solo conociendo

    public decimal? PresupuestoEstimado { get; set; }

    // Interés y Pipeline Comercial STARNET
    public ProductInterest ProductoInteres { get; set; } = ProductInterest.EkklesiApp;
    public PipelineStage EstadoPipeline { get; set; } = PipelineStage.Nuevo;
    public LeadSource Origen { get; set; } = LeadSource.Manual;

    [MaxLength(2000)]
    public string? Notas { get; set; }

    [MaxLength(500)]
    public string? ObjecionPrincipal { get; set; } // Registrada según método E-V-P-R-C

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    public DateTime? UltimaActividad { get; set; }

    // Relaciones
    public List<Interaccion> Interacciones { get; set; } = new();
}
