using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _dashboardRepository;

        public AdminDashboardService(IAdminDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardSummaryAsync(Guid organizationId)
        {
            return await _dashboardRepository.GetDashboardSummaryAsync(organizationId);
        }
    }
}
