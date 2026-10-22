using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces
{
    public interface IUserDashboardRepository
    {
        Task<UserDashboardDto> GetUserSummaryAsync(Guid organizationId, Guid userId, string userRole);
    }
}