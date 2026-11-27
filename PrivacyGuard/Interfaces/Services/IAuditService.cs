using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IAuditService
    {
        Task<List<AuditLogDto>> GetOrganizationLogsAsync(Guid organizationId);
        Task RecordAuthenticationEventAsync(Guid userId, Guid organizationId, string actorEmail, string actorRole, string? ipAddress, string action);
    }
}
