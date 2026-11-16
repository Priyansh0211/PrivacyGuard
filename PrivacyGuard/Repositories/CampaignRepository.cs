using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;
using PrivacyGuard.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Repositories
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public CampaignRepository(ApplicationDbContext dbContext) { _dbContext = dbContext; }

        public async Task<Campaign> CreateCampaignAsync(Campaign campaign)
        {
            await _dbContext.Campaigns.AddAsync(campaign);
            await _dbContext.SaveChangesAsync();
            return campaign;
        }

        public async Task<CampaignAuditReportDto> GetCampaignAuditReportAsync(Guid campaignId, Guid organizationId)
        {
            var campaign = await _dbContext.Campaigns
                .Include(c => c.DataInventory)
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.OrganizationId == organizationId);

            if (campaign == null) throw new Exception("Campaign not found.");

            var responses = await _dbContext.CampaignResponses
                .Include(r => r.User)
                .Include(r => r.DataPrincipal)
                .Where(r => r.CampaignId == campaignId)
                .OrderByDescending(r => r.RespondedAt ?? r.CreatedAt)
                .ToListAsync();

            var accepted = responses.Count(r => r.ResponseStatus == "Accepted");
            var rejected = responses.Count(r => r.ResponseStatus == "Rejected");
            var completed = responses.Count(r => r.ResponseStatus == "Completed");
            var total = responses.Count;
            var checks = new List<string>();

            var hasValidDates = campaign.EndDate > campaign.StartDate;
            checks.Add(hasValidDates ? "Campaign dates are valid" : "Campaign end date must be after start date");

            var hasPurposeLink = !string.Equals(campaign.CampaignType, "Consent", StringComparison.OrdinalIgnoreCase) || campaign.DataInventoryId.HasValue;
            checks.Add(hasPurposeLink ? "Purpose/data inventory requirement is satisfied" : "Consent campaign has no linked data inventory");

            var hasDescription = !string.IsNullOrWhiteSpace(campaign.Description);
            checks.Add(hasDescription ? "Campaign description is available" : "Campaign description is missing");

            var hasValidStatus = campaign.Status is "Draft" or "Active" or "Completed" or "Cancelled";
            checks.Add(hasValidStatus ? "Campaign status is recognized" : "Campaign has an unrecognized status");

            var isCompliant = hasValidDates && hasPurposeLink && hasDescription && hasValidStatus;

            return new CampaignAuditReportDto
            {
                CampaignId = campaign.CampaignId,
                Title = campaign.Title,
                Description = campaign.Description ?? string.Empty,
                CampaignType = campaign.CampaignType,
                TargetAudience = campaign.TargetAudience.ToString(),
                TargetRole = campaign.TargetRole,
                CampaignStatus = campaign.Status,
                LinkedDataInventory = campaign.DataInventory?.Name ?? "Not linked",
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                TotalResponses = total,
                AcceptedCount = accepted,
                RejectedCount = rejected,
                CompletedCount = completed,
                AcceptanceRate = ToPercentage(accepted, total),
                RejectionRate = ToPercentage(rejected, total),
                CompletionRate = ToPercentage(completed, total),
                AuditResult = isCompliant ? "Compliant" : "Attention Required",
                AuditRemarks = isCompliant ? "No campaign configuration gaps were found." : "Review the failed compliance checks before relying on this campaign audit.",
                ComplianceChecks = checks,
                Responses = responses.Select(response => new CampaignResponseAuditDto
                {
                    ResponseReference = response.ResponseId.ToString("N")[..8].ToUpperInvariant(),
                    ActorType = response.UserId.HasValue ? "Internal User" : "Data Principal",
                    MaskedEmail = PiiMaskingHelper.MaskEmail(response.User?.Email ?? response.DataPrincipal?.Email),
                    ResponseStatus = response.ResponseStatus,
                    CreatedAt = response.CreatedAt,
                    RespondedAt = response.RespondedAt
                }).ToList()
            };
        }

        private static decimal ToPercentage(int value, int total)
        {
            return total == 0 ? 0 : Math.Round(value * 100m / total, 2);
        }

        public async Task<Campaign?> GetCampaignByIdAsync(Guid campaignId, Guid organizationId)
        {
            return await _dbContext.Campaigns.FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.OrganizationId == organizationId);
        }

        public async Task<IEnumerable<Campaign>> GetAllCampaignsByOrgAsync(Guid organizationId)
        {
            return await _dbContext.Campaigns.Include(c => c.DataInventory)
                .Where(c => c.OrganizationId == organizationId).OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetActiveCampaignsForUserAsync(Guid organizationId, Guid actorId, string userRole, bool isInternalUser)
        {
            var currentDate = DateTime.UtcNow;

            return await _dbContext.Campaigns.Include(c => c.DataInventory)
                .Where(c => c.OrganizationId == organizationId && c.Status == "Active" && c.StartDate <= currentDate && c.EndDate >= currentDate
                         && (
                             c.TargetAudience == AudienceType.Everyone ||
                             (!isInternalUser && c.TargetAudience == AudienceType.AllDataPrincipals) ||
                             (isInternalUser && c.TargetAudience == AudienceType.AllInternalUsers && (c.TargetRole == "All" || c.TargetRole == userRole)) ||
                             // 🚨 TICK BOX LOGIC
                             (c.TargetAudience == AudienceType.Specific &&
                                ((!isInternalUser && _dbContext.CampaignAudienceMappings.Any(m => m.CampaignId == c.CampaignId && m.DataPrincipalId == actorId)) ||
                                 (isInternalUser && _dbContext.CampaignAudienceMappings.Any(m => m.CampaignId == c.CampaignId && m.UserId == actorId)))
                             )
                         ))
                .OrderBy(c => c.EndDate).ToListAsync();
        }

        public async Task<CampaignResponse?> GetUserResponseAsync(Guid campaignId, Guid actorId, bool isInternalUser)
        {
            // Proper Table Check based on Role
            if (isInternalUser)
                return await _dbContext.CampaignResponses.FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.UserId == actorId);
            else
                return await _dbContext.CampaignResponses.FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.DataPrincipalId == actorId);
        }

        public async Task<CampaignResponse> SaveResponseAsync(CampaignResponse response, bool isInternalUser)
        {
            if (isInternalUser)
            {
                if (!response.UserId.HasValue ||
                    !await _dbContext.Users.AnyAsync(u => u.UserId == response.UserId.Value))
                {
                    throw new InvalidOperationException("The logged-in internal user does not exist.");
                }
            }
            else
            {
                if (!response.DataPrincipalId.HasValue ||
                    !await _dbContext.DataPrincipals.AnyAsync(dp => dp.DataPrincipalId == response.DataPrincipalId.Value))
                {
                    throw new InvalidOperationException(
                        "The token contains an invalid DataPrincipalId. Log out, log in again, and use the new token.");
                }
            }

            Guid actorId = isInternalUser ? response.UserId!.Value : response.DataPrincipalId!.Value;
            var existing = await GetUserResponseAsync(response.CampaignId, actorId, isInternalUser);

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
            var campaign = await _dbContext.Campaigns.FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.OrganizationId == organizationId);
            if (campaign == null) throw new Exception("Campaign not found.");
            var responses = await _dbContext.CampaignResponses.Where(r => r.CampaignId == campaignId).ToListAsync();

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

        //  GET AUDIENCE LIST (Both Types Support)
        public async Task<List<AudienceMemberDto>> GetAudienceListAsync(Guid orgId, string type)
        {
            if (type.ToLower() == "internal")
            {
                return await _dbContext.Users.Where(u => u.OrganizationId == orgId && u.IsActive)
                    .Select(u => new AudienceMemberDto { Id = u.UserId, FullName = u.FullName, Email = u.Email, UserType = "InternalUser" }).ToListAsync();
            }
            else
            {
                return await _dbContext.DataPrincipals.Where(dp => dp.OrganizationId == orgId)
                    .Select(dp => new AudienceMemberDto { Id = dp.DataPrincipalId, FullName = dp.FullName, Email = dp.Email, UserType = "DataPrincipal" }).ToListAsync();
            }
        }

        // LINK AUDIENCE TO CAMPAIGN (Smart Delete & Insert)
        public async Task LinkAudienceToCampaignAsync(Guid campaignId, List<Guid> selectedIds, bool isInternalUserMapping)
        {
            // Sirf wahi records delete karo jo is type (staff ya customer) se related hain
            var existingMappings = await _dbContext.CampaignAudienceMappings
                .Where(m => m.CampaignId == campaignId && (isInternalUserMapping ? m.UserId != null : m.DataPrincipalId != null))
                .ToListAsync();

            _dbContext.CampaignAudienceMappings.RemoveRange(existingMappings);

            var newMappings = selectedIds.Select(id => new CampaignAudienceMapping
            {
                CampaignId = campaignId,
                UserId = isInternalUserMapping ? id : null,
                DataPrincipalId = !isInternalUserMapping ? id : null
            }).ToList();

            await _dbContext.CampaignAudienceMappings.AddRangeAsync(newMappings);
            await _dbContext.SaveChangesAsync();
        }
    }
}