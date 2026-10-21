using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IAdminDashboardRepository
    {
        Task<AdminDashboardDto> GetDashboardSummaryAsync(Guid organizationId);
    }
}
