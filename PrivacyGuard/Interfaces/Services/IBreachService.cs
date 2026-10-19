using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IBreachService
    {
        Task<(bool IsSuccess, string ErrorMessage)> ReportIncidentAsync(Guid userId, ReportBreachDto dto);
        Task<List<BreachResponseDto>> GetAllIncidentsAsync(Guid organizationId);

        Task<(bool IsSuccess, string ErrorMessage)> AssignIncidentAsync(Guid incidentId, Guid organizationId, Guid adminUserId);
        Task<(bool IsSuccess, string ErrorMessage)> UpdateIncidentStatusAsync(Guid incidentId, Guid organizationId, Guid adminUserId, UpdateBreachStatusDto dto);
    }
}
