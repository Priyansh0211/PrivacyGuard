using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface ICampaignService
    {
        Task<Campaign> CreateCampaignAsync(CreateCampaignDto dto, Guid organizationId);
        Task<IEnumerable<Campaign>> GetMyActiveCampaignsAsync(Guid organizationId, Guid actorId, string userRole, bool isInternalUser);
        Task<CampaignResponse> SubmitCampaignResponseAsync(SubmitCampaignResponseDto dto, Guid actorId, Guid organizationId, bool isInternalUser);
        Task<IEnumerable<Campaign>> GetAllCampaignsAsync(Guid organizationId);
        Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId, Guid organizationId);

        Task<List<AudienceMemberDto>> GetAudienceListAsync(Guid orgId, string type);
        Task<bool> AssignAudienceAsync(LinkAudienceRequestDto request);
    }
}