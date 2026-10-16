using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IAuditRepository
    {
        Task<List<AuditLog>> GetLogsByOrganizationAsync(Guid organizationId);
    }
}
