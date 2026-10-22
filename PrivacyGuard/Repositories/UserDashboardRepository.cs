using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PrivacyGuard.Repositories
{
    public class UserDashboardRepository : IUserDashboardRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserDashboardRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserDashboardDto> GetUserSummaryAsync(Guid organizationId, Guid userId, string userRole)
        {
            var currentDate = DateTime.UtcNow;
            var userIdStr = userId.ToString();

            // 1. Pending Actions (Campaigns)
            var activeCampaigns = await _dbContext.Campaigns
                .Where(c => c.OrganizationId == organizationId
                         && c.Status == "Active"
                         && c.StartDate <= currentDate
                         && c.EndDate >= currentDate
                         && (c.TargetRole == "All" || c.TargetRole == userRole))
                .Select(c => c.CampaignId)
                .ToListAsync();

            var respondedCampaigns = await _dbContext.CampaignResponses
                .Where(r => r.UserId == userId && activeCampaigns.Contains(r.CampaignId))
                .Select(r => r.CampaignId)
                .ToListAsync();

            var pendingActionsCount = activeCampaigns.Count - respondedCampaigns.Count;

            // 2. Active Consents
            var activeConsentsCount = await _dbContext.ConsentRecords
                .CountAsync(c => c.OrganizationId == organizationId && c.DataPrincipalId == userIdStr && c.ConsentStatus == "Granted");

            // 3. Privacy Health Score Calculation
            int healthScore = 100 - (pendingActionsCount * 15);
            if (healthScore < 20) healthScore = 20;

            // 4. Audit Log (Recent Activity)
            var recentActivityLog = await _dbContext.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(4)
                .Select(a => new MyActivityLogDto
                {
                    Action = $"{a.ActionType} activity on {a.TableName}",
                    Date = a.Timestamp
                })
                .ToListAsync();

            // 5. Total Requests Raised by User
            var totalRequestsCount = await _dbContext.PrivacyRequests
                .CountAsync(p => p.OrganizationId == organizationId && p.DataPrincipalId == userIdStr);

            return new UserDashboardDto
            {
                PrivacyHealthScore = healthScore,
                PendingActionsCount = pendingActionsCount,
                ActiveConsentsCount = activeConsentsCount,
                TotalRequestsRaised = totalRequestsCount,
                RecentActivityLog = recentActivityLog
            };
        }
    }
}
