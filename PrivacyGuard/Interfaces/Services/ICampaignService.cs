using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface ICampaignService
    {
        Task<Campaign> CreateCampaignAsync(CreateCampaignDto dto, Guid organizationId);
        Task<IEnumerable<Campaign>> GetMyActiveCampaignsAsync(Guid organizationId, Guid userId, string userRole);
        Task<CampaignResponse> SubmitCampaignResponseAsync(SubmitCampaignResponseDto dto, Guid userId, Guid organizationId);

        Task<IEnumerable<Campaign>> GetAllCampaignsAsync(Guid organizationId);

        Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId, Guid organizationId);
    }
}
