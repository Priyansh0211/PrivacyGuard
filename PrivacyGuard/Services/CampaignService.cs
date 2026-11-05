using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        public CampaignService(ICampaignRepository campaignRepository) { _campaignRepository = campaignRepository; }

        public async Task<Campaign> CreateCampaignAsync(CreateCampaignDto dto, Guid organizationId)
        {
            if (dto.EndDate <= dto.StartDate) throw new Exception("End date must be greater than Start date.");
            if (dto.CampaignType == "Consent" && dto.DataInventoryId == null) throw new Exception("DataInventoryId required for Consent type.");

            var campaign = new Campaign
            {
                OrganizationId = organizationId,
                Title = dto.Title,
                Description = dto.Description,
                CampaignType = dto.CampaignType,
                TargetAudience = dto.TargetAudience,
                TargetRole = dto.TargetRole,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Active",
                DataInventoryId = dto.DataInventoryId
            };
            return await _campaignRepository.CreateCampaignAsync(campaign);
        }

        public async Task<IEnumerable<Campaign>> GetMyActiveCampaignsAsync(Guid organizationId, Guid actorId, string userRole, bool isInternalUser)
        {
            var allActiveCampaigns = await _campaignRepository.GetActiveCampaignsForUserAsync(organizationId, actorId, userRole, isInternalUser);
            var pendingCampaigns = new List<Campaign>();

            foreach (var campaign in allActiveCampaigns)
            {
                var existingResponse = await _campaignRepository.GetUserResponseAsync(campaign.CampaignId, actorId, isInternalUser);
                if (existingResponse == null) pendingCampaigns.Add(campaign);
            }
            return pendingCampaigns;
        }

        public async Task<CampaignResponse> SubmitCampaignResponseAsync(SubmitCampaignResponseDto dto, Guid actorId, Guid organizationId, bool isInternalUser)
        {
            var campaign = await _campaignRepository.GetCampaignByIdAsync(dto.CampaignId, organizationId);
            if (campaign == null) throw new Exception("Campaign not found.");

            var response = new CampaignResponse
            {
                CampaignId = dto.CampaignId,
                ResponseStatus = dto.ResponseStatus,
                RespondedAt = DateTime.UtcNow,
                UserId = isInternalUser ? actorId : null,
                DataPrincipalId = !isInternalUser ? actorId : null
            };
            return await _campaignRepository.SaveResponseAsync(response, isInternalUser);
        }

        public async Task<IEnumerable<Campaign>> GetAllCampaignsAsync(Guid organizationId) => await _campaignRepository.GetAllCampaignsByOrgAsync(organizationId);
        public async Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId, Guid organizationId) => await _campaignRepository.GetCampaignStatsAsync(campaignId, organizationId);
        public async Task<List<AudienceMemberDto>> GetAudienceListAsync(Guid orgId, string type) => await _campaignRepository.GetAudienceListAsync(orgId, type);

        public async Task<bool> AssignAudienceAsync(LinkAudienceRequestDto request)
        {
            if (request.SelectedIds == null || request.SelectedIds.Count == 0) return false;
            await _campaignRepository.LinkAudienceToCampaignAsync(request.CampaignId, request.SelectedIds, request.IsInternalUserMapping);
            return true;
        }
    }
}