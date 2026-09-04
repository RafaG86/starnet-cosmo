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
    Task<Interaccion?> AddInteractionAsync(int leadId, InteraccionCreateDto dto);
    Task<LeadStatsDto> GetStatsAsync();
    Task<bool> DeleteLeadAsync(int id);
    string GenerateWhatsAppLink(Lead lead, string tipoScript);
    string GenerateStarnetHubHandoff(Lead lead);
}
