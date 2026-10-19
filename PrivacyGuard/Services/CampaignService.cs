using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository;

        public CampaignService(ICampaignRepository campaignRepository)
        {
            _campaignRepository = campaignRepository;
        }

        public async Task<Campaign> CreateCampaignAsync(CreateCampaignDto dto, Guid organizationId)
        {
            // 💡 BUSINESS LOGIC 1: Date Validation
            if (dto.EndDate <= dto.StartDate)
            {
                throw new Exception("End date must be greater than Start date.");
            }

            // 💡 BUSINESS LOGIC 2: Consent campaign ke liye DataInventoryId check
            if (dto.CampaignType == "Consent" && dto.DataInventoryId == null)
            {
                throw new Exception("DataInventoryId is required for Consent type campaigns.");
            }

            // DTO ko Database Model (Entity) mein map kar rahe hain
            var campaign = new Campaign
            {
                OrganizationId = organizationId,
                Title = dto.Title,
                Description = dto.Description,
                CampaignType = dto.CampaignType,
                TargetRole = dto.TargetRole,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Active", // By default active rakh rahe hain taaki turant test ho sake
                DataInventoryId = dto.DataInventoryId
            };

            return await _campaignRepository.CreateCampaignAsync(campaign);
        }

        public async Task<IEnumerable<Campaign>> GetMyActiveCampaignsAsync(Guid organizationId, Guid userId, string userRole)
        {
            // 1. Repository se saare active campaigns le aao jo is role ke liye hain
            var allActiveCampaigns = await _campaignRepository.GetActiveCampaignsForUserAsync(organizationId, userRole);

            // 2. 💡 BUSINESS LOGIC 3: Un campaigns ko filter out karo jinka response user pehle hi de chuka hai
            var pendingCampaigns = new List<Campaign>();

            foreach (var campaign in allActiveCampaigns)
            {
                var existingResponse = await _campaignRepository.GetUserResponseAsync(campaign.CampaignId, userId);

                // Agar user ne abhi tak koi response nahi diya hai (null), tabhi list mein add karo
                if (existingResponse == null)
                {
                    pendingCampaigns.Add(campaign);
                }
            }

            return pendingCampaigns; // Ab user ko dashboard par sirf "Pending" tasks dikhenge!
        }

        public async Task<CampaignResponse> SubmitCampaignResponseAsync(SubmitCampaignResponseDto dto, Guid userId, Guid organizationId)
        {
            // 1. Verify karo ki campaign exist karta hai aur isi tenant(org) ka hai
            var campaign = await _campaignRepository.GetCampaignByIdAsync(dto.CampaignId, organizationId);
            if (campaign == null)
            {
                throw new Exception("Campaign not found or does not belong to your organization.");
            }

            // 2. Naya response object banao
            var response = new CampaignResponse
            {
                CampaignId = dto.CampaignId,
                UserId = userId,
                ResponseStatus = dto.ResponseStatus, // "Accepted", "Rejected", ya "Completed"
                RespondedAt = DateTime.UtcNow
            };

            // 3. Database mein save karo
            return await _campaignRepository.SaveResponseAsync(response);
        }

        public async Task<IEnumerable<Campaign>> GetAllCampaignsAsync(Guid organizationId)
        {
            // Repository se seedha saare campaigns mangwa lenge
            return await _campaignRepository.GetAllCampaignsByOrgAsync(organizationId);
        }

        public async Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId, Guid organizationId)
        {
            return await _campaignRepository.GetCampaignStatsAsync(campaignId, organizationId);
        }
    }
}
