using StarnetCosmo.Core.DTOs;
using StarnetCosmo.Core.Entities;

namespace StarnetCosmo.Core.Services;

public interface ILeadService
{
    Task<List<Lead>> GetLeadsAsync(LeadFilterDto filter);
    Task<Lead?> GetLeadByIdAsync(int id);
    Task<(bool Success, string Message, Lead? Lead)> CreateLeadAsync(LeadCreateDto dto);
    Task<BulkImportResultDto> ImportBulkLeadsAsync(List<LeadBulkItemDto> leads);
    Task<bool> UpdateStageAsync(int id, LeadUpdateStageDto dto);
    Task<bool> UpdateLeadAsync(int id, LeadUpdateDto dto);
    Task<bool> DeleteLeadAsync(int id);
    Task<CalificacionTondm?> GetTondmAsync(int leadId);
    Task<CalificacionTondm> SaveTondmAsync(int leadId, TondmUpsertDto dto);
    Task<Interaccion?> AddInteractionAsync(int leadId, InteraccionCreateDto dto);
    Task<LeadStatsDto> GetStatsAsync();
    string GenerateWhatsAppLink(Lead lead, string tipoScript);
    string GenerateStarnetHubHandoff(Lead lead);
}
