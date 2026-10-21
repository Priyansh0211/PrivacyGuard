using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface ICampaignRepository
    {

        // Admin operations
        Task<Campaign> CreateCampaignAsync(Campaign campaign);
        Task<Campaign?> GetCampaignByIdAsync(Guid campaignId, Guid organizationId);
        Task<IEnumerable<Campaign>> GetAllCampaignsByOrgAsync(Guid organizationId);

        // User operations
        Task<IEnumerable<Campaign>> GetActiveCampaignsForUserAsync(Guid organizationId, string userRole);

        // Tracking/Response operations
        Task<CampaignResponse?> GetUserResponseAsync(Guid campaignId, Guid userId);
        Task<CampaignResponse> SaveResponseAsync(CampaignResponse response);

        Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId, Guid organizationId);

    }
}
