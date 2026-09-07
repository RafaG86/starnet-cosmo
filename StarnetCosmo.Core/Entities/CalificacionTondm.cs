using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarnetCosmo.Core.Entities;

/// <summary>
/// Calificación comercial detallada de la iglesia mediante la metodología T-O-N-D-M del manual STARNET.
/// Se diligencia manualmente durante la llamada telefónica de prospección.
/// </summary>
public class CalificacionTondm
{
    public int Id { get; set; }

    [Required]
    public int LeadId { get; set; }

    [ForeignKey("LeadId")]
    public Lead? Lead { get; set; }

    // ==========================================
    // T - TIEMPO (Urgencia y tiempos de adopción)
    // ==========================================
    [MaxLength(100)]
    public string? UrgenciaImplementacion { get; set; } // Inmediato (1-2 semanas), Corto plazo (1 mes), Mediano (2-3 meses), Solo explorando

    [MaxLength(100)]
    public string? FechaTentativaImplementacion { get; set; }

    public bool TieneEventoProximo { get; set; } = false; // ¿Tienen aniversario, convención o evento donde necesiten el software ya?

    // ====================================================
    // O - OPERACIÓN & ORGANIZACIÓN (Procesos actuales)
    // ====================================================
    public int? CantidadMiembros { get; set; }
    public int NumeroSedes { get; set; } = 1;
    public bool TieneCelulas { get; set; } = false;
    public int? CantidadCelulas { get; set; }
    public bool TieneEscuelaFormacion { get; set; } = false;

    [MaxLength(150)]
    public string? HerramientaActual { get; set; } // Excel, Cuaderno / Papel, WhatsApp, Planning Center, IglesiaTech, etc.

    [MaxLength(1000)]
    public string? ProcesosPorFuera { get; set; } // Qué procesos quedan desconectados (ej. asistencia, seguimiento, ofrendas)

    // ==========================================
    // N - NECESIDAD & DOLOR (Pain points reales)
    // ==========================================
    [MaxLength(300)]
    public string? DolorPrincipal { get; set; } // Pérdida de personas, desorden en alabanza, falta de consolidación, reportes lentos

    [MaxLength(50)]
    public string? NivelDolor { get; set; } // Crítico / Alto, Medio, Bajo / Confort

    [MaxLength(1500)]
    public string? DetalleNecesidad { get; set; } // Relato exacto de lo que el líder o pastor expresó en la llamada

    // ==========================================
    // D - DECISOR (Quién toma la decisión final)
    // ==========================================
    [MaxLength(150)]
    public string? NombreDecisor { get; set; }

    [MaxLength(100)]
    public string? CargoDecisor { get; set; } // Pastor Principal, Pastor Ejecutivo, Junta Directiva, Administrador

    public bool DecisorPresenteEnLlamada { get; set; } = false;

    [MaxLength(300)]
    public string? ProcesoDecision { get; set; } // Decide el pastor solo, decide junta en reunión mensual, decide comité de tecnología

    // ==========================================
    // M - MONTO & PRESUPUESTO (Capacidad financiera)
    // ==========================================
    public decimal? PresupuestoEstimado { get; set; }

    [MaxLength(20)]
    public string? Moneda { get; set; } = "COP"; // COP, USD, MXN, ARS

    [MaxLength(50)]
    public string? DisposicionInversion { get; set; } // Alta (prioridad con presupuesto), Media (depende de la demo), Baja (buscan gratis)

    // ==========================================
    // AUDITORÍA & ESTADO DE LA LLAMADA
    // ==========================================
    public DateTime FechaLlamada { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CalificadoPor { get; set; } = "Comercial STARNET";

    [MaxLength(50)]
    public string? ResultadoLlamada { get; set; } // Demo Agendada, Interesado (Seguimiento), Volver a Llamar, No Califica

    [MaxLength(2000)]
    public string? NotasLlamada { get; set; }

    public bool CalificacionCompletada { get; set; } = true;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
