using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface ICampaignRepository
    {
        Task<Campaign> CreateCampaignAsync(Campaign campaign);
        Task<Campaign?> GetCampaignByIdAsync(Guid campaignId, Guid organizationId);
        Task<IEnumerable<Campaign>> GetAllCampaignsByOrgAsync(Guid organizationId);

        Task<IEnumerable<Campaign>> GetActiveCampaignsForUserAsync(Guid organizationId, Guid actorId, string userRole, bool isInternalUser);

        Task<CampaignResponse?> GetUserResponseAsync(Guid campaignId, Guid actorId, bool isInternalUser);
        Task<CampaignResponse> SaveResponseAsync(CampaignResponse response, bool isInternalUser);

        Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId, Guid organizationId);

        // 💡 Audience Mapping
        Task<List<AudienceMemberDto>> GetAudienceListAsync(Guid orgId, string type);
        Task LinkAudienceToCampaignAsync(Guid campaignId, List<Guid> selectedIds, bool isInternalUserMapping);
    }
}