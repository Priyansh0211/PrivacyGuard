using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IAuditRepository
    {
        Task<List<AuditLog>> GetLogsByOrganizationAsync(Guid organizationId);
        Task RecordAuthenticationEventAsync(Guid userId, Guid organizationId, string actorEmail, string actorRole, string? ipAddress, string action);
    }
}
