using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminDashboardRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AdminDashboardDto> GetDashboardSummaryAsync(Guid organizationId)
        {
            var currentDate = DateTime.UtcNow;

            // 1. Basic Counts
            var totalUsers = await _dbContext.Users.CountAsync(u => u.OrganizationId == organizationId);

            // PERMANENT FIX: Ab hum child table (ConsentDetails) mein check kar rahe hain 
            // ki kisi bhi granular purpose ka status "Granted" hai ya nahi.
            var activeConsents = await _dbContext.ConsentRecords.CountAsync(c =>
                c.OrganizationId == organizationId &&
                c.ConsentDetails.Any(d => d.ConsentStatus == "Granted"));

            // 2. SLA Risk Logic
            // DPDP rule: Request 30 din mein poori honi chahiye. Hum 25 din pe 'Risk' dikhayenge.
            var riskThresholdDate = currentDate.AddDays(-25);

            var allPendingRequests = await _dbContext.PrivacyRequests
                .Where(p => p.OrganizationId == organizationId && p.Status == "Pending")
                .ToListAsync();

            var pendingRequestsCount = allPendingRequests.Count;
            var slaRiskRequestsCount = allPendingRequests.Count(p => p.CreatedAt <= riskThresholdDate);

            var totalBreaches = await _dbContext.DataBreachIncidents.CountAsync(b => b.OrganizationId == organizationId);

            // 3. Campaign Performance
            var activeCampaigns = await _dbContext.Campaigns
                .Where(c => c.OrganizationId == organizationId && c.Status == "Active")
                .ToListAsync();

            var campaignPerformances = new List<CampaignStatsDto>();

            foreach (var campaign in activeCampaigns)
            {
                var responses = await _dbContext.CampaignResponses
                    .Where(r => r.CampaignId == campaign.CampaignId)
                    .ToListAsync();

                campaignPerformances.Add(new CampaignStatsDto
                {
                    CampaignId = campaign.CampaignId,
                    Title = campaign.Title,
                    TotalResponses = responses.Count,
                    AcceptedCount = responses.Count(r => r.ResponseStatus == "Accepted"),
                    RejectedCount = responses.Count(r => r.ResponseStatus == "Rejected"),
                    CompletedCount = responses.Count(r => r.ResponseStatus == "Completed")
                });
            }

            // 4. Recent Breaches (Aakhri 5 incidents)
            var recentBreaches = await _dbContext.DataBreachIncidents
                .Where(b => b.OrganizationId == organizationId)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new RecentBreachDto
                {
                    Title = "Data Breach Incident",
                    Severity = b.Severity ?? "High",
                    IncidentDate = b.CreatedAt
                })
                .ToListAsync();

            // 5. Calculate Compliance Health Score (Smart Algorithm)
            // SYNTAX FIX: Yahan math operators gayab the, unhe add kar diya hai
            int healthScore = 100 - (pendingRequestsCount * 1) - (slaRiskRequestsCount * 5) - (totalBreaches * 10);

            if (healthScore < 0) healthScore = 0;
            if (healthScore > 100) healthScore = 100;

            return new AdminDashboardDto
            {
                ComplianceHealthScore = healthScore,
                TotalUsers = totalUsers,
                ActiveConsents = activeConsents,
                PendingRequests = pendingRequestsCount,
                SlaRiskRequests = slaRiskRequestsCount,
                TotalDataBreaches = totalBreaches,
                CampaignPerformances = campaignPerformances,
                RecentBreaches = recentBreaches
            };
        }
    }
}