using System.ComponentModel.DataAnnotations;

namespace StarnetCosmo.Core.Entities;

public class Interaccion
{
    public int Id { get; set; }

    public int LeadId { get; set; }
    public Lead? Lead { get; set; }

    public InteractionType Tipo { get; set; } = InteractionType.WhatsApp;

    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Detalle { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MetodoEVPRC_Fase { get; set; } // Escuchar, Validar, Preguntar, Responder, Confirmar

    public bool Exitoso { get; set; } = true;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
