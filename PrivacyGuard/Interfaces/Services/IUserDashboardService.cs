using PrivacyGuard.DTOs;

public interface IUserDashboardService
{
    Task<UserDashboardDto> GetUserSummaryAsync(Guid organizationId, Guid userId, string userRole);
}