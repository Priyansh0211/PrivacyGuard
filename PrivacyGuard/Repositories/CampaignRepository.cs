using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CampaignRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Campaign> CreateCampaignAsync(Campaign campaign)
        {
            await _dbContext.Campaigns.AddAsync(campaign);
            await _dbContext.SaveChangesAsync();
            return campaign;
        }

        public async Task<Campaign?> GetCampaignByIdAsync(Guid campaignId, Guid organizationId)
        {
            // OrganizationId check zaroori hai Tenant Isolation ke liye
            return await _dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.OrganizationId == organizationId);
        }

        public async Task<IEnumerable<Campaign>> GetAllCampaignsByOrgAsync(Guid organizationId)
        {

            return await _dbContext.Campaigns
                .Include(c => c.DataInventory)
                .Where(c => c.OrganizationId == organizationId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetActiveCampaignsForUserAsync(Guid organizationId, string userRole)
        {
            var currentDate = DateTime.UtcNow;

            // Sirf wahi campaigns layenge jo Active hain, date match karti hai, aur Role ya toh 'All' hai ya User ka Role hai
            return await _dbContext.Campaigns
                .Include(c => c.DataInventory)
                .Where(c => c.OrganizationId == organizationId
                         && c.Status == "Active"
                         && c.StartDate <= currentDate
                         && c.EndDate >= currentDate
                         && (c.TargetRole == "All" || c.TargetRole == userRole))
                .OrderBy(c => c.EndDate) // Jo pehle khatam hone wala hai wo upar aayega
                .ToListAsync();
        }

        public async Task<CampaignResponse?> GetUserResponseAsync(Guid campaignId, Guid userId)
        {
            return await _dbContext.CampaignResponses
                .FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.UserId == userId);
        }

        public async Task<CampaignResponse> SaveResponseAsync(CampaignResponse response)
        {
            // Check karte hain ki pehle se record hai ya nahi
            var existing = await GetUserResponseAsync(response.CampaignId, response.UserId);

            if (existing != null)
            {
                existing.ResponseStatus = response.ResponseStatus;
                existing.RespondedAt = response.RespondedAt;
                _dbContext.CampaignResponses.Update(existing);
            }
            else
            {
                await _dbContext.CampaignResponses.AddAsync(response);
            }

            await _dbContext.SaveChangesAsync();
            return response;
        }

        public async Task<CampaignStatsDto> GetCampaignStatsAsync(Guid campaignId, Guid organizationId)
        {
            // 1. Campaign check karo ki wo is admin ki organization ka hai ya nahi
            var campaign = await _dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.OrganizationId == organizationId);

            if (campaign == null) throw new Exception("Campaign not found.");

            // 2. Us campaign ke saare responses nikal lo
            var responses = await _dbContext.CampaignResponses
                .Where(r => r.CampaignId == campaignId)
                .ToListAsync();

            // 3. Stats calculate karke DTO mein daal do
            return new CampaignStatsDto
            {
                CampaignId = campaign.CampaignId,
                Title = campaign.Title,
                TotalResponses = responses.Count,
                AcceptedCount = responses.Count(r => r.ResponseStatus == "Accepted"),
                RejectedCount = responses.Count(r => r.ResponseStatus == "Rejected"),
                CompletedCount = responses.Count(r => r.ResponseStatus == "Completed")
            };
        }
    }
}
