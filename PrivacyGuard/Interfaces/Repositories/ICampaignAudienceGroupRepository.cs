using PrivacyGuard.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface ICampaignAudienceGroupRepository
    {
        Task<CampaignAudienceGroupDto> CreateAsync(Guid organizationId, CreateCampaignAudienceGroupDto dto);
        Task<IEnumerable<CampaignAudienceGroupDto>> GetAllAsync(Guid organizationId);
        Task<CampaignAudienceGroupDto?> GetByIdAsync(Guid groupId, Guid organizationId);
        Task<CampaignAudienceGroupDto?> UpdateAsync(Guid groupId, Guid organizationId, UpdateCampaignAudienceGroupDto dto);
        Task<bool> DeleteAsync(Guid groupId, Guid organizationId);
        Task ApplyToCampaignAsync(Guid groupId, Guid campaignId, Guid organizationId);
    }
}
