using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;

namespace PrivacyGuard.Services
{
    public class UserDashboardService : IUserDashboardService
    {
        private readonly IUserDashboardRepository _userRepo;

        public UserDashboardService(IUserDashboardRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<UserDashboardDto> GetUserSummaryAsync(Guid organizationId, Guid userId, string userRole)
        {
            
            return await _userRepo.GetUserSummaryAsync(organizationId, userId, userRole);
        }
    }
}
