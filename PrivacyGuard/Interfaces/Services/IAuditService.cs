using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IAuditService
    {
        Task<List<AuditLogDto>> GetOrganizationLogsAsync(Guid organizationId);
    }
}
