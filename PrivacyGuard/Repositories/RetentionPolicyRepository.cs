using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class RetentionPolicyRepository : IRetentionPolicyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RetentionPolicyRepository> _logger;
        private const int UpcomingWindowDays = 7;
        private const int BatchSize = 50;

        public RetentionPolicyRepository(
            ApplicationDbContext context,
            ILogger<RetentionPolicyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        private static DateTime ResolveTriggerBaseDate(string? triggerEvent, DateTime createdAt, DateTime updatedAt)
        {
            return string.Equals(triggerEvent, "LastActivity", StringComparison.OrdinalIgnoreCase)
                ? updatedAt
                : createdAt;
        }

        public async Task<RetentionPolicy?> GetPolicyByIdAsync(Guid policyId, Guid organizationId)
        {
            return await _context.RetentionPolicies
                .FirstOrDefaultAsync(p => p.PolicyId == policyId && p.OrganizationId == organizationId);
        }

        public async Task<IEnumerable<RetentionPolicy>> GetAllPoliciesAsync(Guid organizationId)
        {
            return await _context.RetentionPolicies
                .Where(p => p.OrganizationId == organizationId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<RetentionPolicy> AddPolicyAsync(RetentionPolicy policy)
        {
            await _context.RetentionPolicies.AddAsync(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task<RetentionPolicy> UpdatePolicyAsync(RetentionPolicy policy)
        {
            policy.UpdatedAt = DateTime.UtcNow;
            _context.RetentionPolicies.Update(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task<bool> DeletePolicyAsync(Guid policyId, Guid organizationId)
        {
            var policy = await GetPolicyByIdAsync(policyId, organizationId);
            if (policy == null) return false;

            _context.RetentionPolicies.Remove(policy);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RetentionPolicy>> GetAllActivePoliciesForJobAsync()
        {
            return await _context.RetentionPolicies
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<RetentionPolicy>> GetActivePoliciesByOrganizationAsync(Guid organizationId)
        {
            return await _context.RetentionPolicies
                .Where(p => p.IsActive && p.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<int> ProcessExpiredDataAsync(
            Guid organizationId,
            int retentionPeriodInDays,
            string actionType,
            string triggerEvent)
        {
            bool isLastActivity = string.Equals(triggerEvent, "LastActivity", StringComparison.OrdinalIgnoreCase);
            bool isKnownAction =
                string.Equals(actionType, "Anonymize", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, "Delete", StringComparison.OrdinalIgnoreCase);

            if (!isKnownAction)
            {
                _logger.LogWarning("Unknown ActionType '{ActionType}' for Org {OrganizationId}. Skipping policy execution.", actionType, organizationId);
                return 0;
            }

            var now = DateTime.UtcNow;
            var cutoffDate = now.AddDays(-retentionPeriodInDays);
            int totalProcessed = 0;

            var query = _context.DataPrincipals
                .Where(dp => dp.OrganizationId == organizationId
                             && dp.AnonymizedAt == null
                             && !dp.IsOnLegalHold);

            query = isLastActivity
                ? query.Where(dp => dp.UpdatedAt <= cutoffDate)
                : query.Where(dp => dp.CreatedAt <= cutoffDate);

            var eligibleIds = await query
                .Select(dp => dp.DataPrincipalId)
                .ToListAsync();

            if (!eligibleIds.Any())
                return 0;

            for (int i = 0; i < eligibleIds.Count; i += BatchSize)
            {
                var batchIds = eligibleIds.Skip(i).Take(BatchSize).ToList();

                var batchRecords = await _context.DataPrincipals
                    .Include(dp => dp.Identifiers)
                    .Where(dp => batchIds.Contains(dp.DataPrincipalId))
                    .ToListAsync();

                foreach (var dp in batchRecords)
                {
                    try
                    {
                        DataPrincipalAnonymizer.Anonymize(dp, now);
                        await _context.SaveChangesAsync();
                        totalProcessed++;
                    }
                    catch (Exception ex)
                    {
                        _context.Entry(dp).State = EntityState.Detached;
                        if (dp.Identifiers != null)
                        {
                            foreach (var idItem in dp.Identifiers)
                            {
                                _context.Entry(idItem).State = EntityState.Detached;
                            }
                        }

                        _logger.LogError(ex,
                            "Failed to {ActionType} DataPrincipal {DataPrincipalId} for Org {OrganizationId}. Skipping record.",
                            actionType, dp.DataPrincipalId, organizationId);
                    }
                }
            }

            return totalProcessed;
        }

        public async Task<RetentionMetricsDto> GetDashboardMetricsAsync(Guid organizationId)
        {
            var queueItems = (await GetErasureQueueAsync(organizationId)).ToList();
            var activePoliciesCount = await _context.RetentionPolicies
                .CountAsync(p => p.OrganizationId == organizationId && p.IsActive);

            return new RetentionMetricsDto
            {
                ActivePolicies = activePoliciesCount,
                Overdue = queueItems.Count(q => q.Status == "Overdue"),
                JobsInQueue = queueItems.Count(q => q.Status == "Pending")
            };
        }

        public async Task<IEnumerable<ErasureQueueDto>> GetErasureQueueAsync(Guid organizationId)
        {
            var activePolicies = await _context.RetentionPolicies
                .Where(p => p.OrganizationId == organizationId && p.IsActive)
                .OrderBy(p => p.RetentionPeriodInDays)
                .ToListAsync();

            if (!activePolicies.Any())
                return new List<ErasureQueueDto>();

            var now = DateTime.UtcNow;

            var principals = await _context.DataPrincipals
                .Where(dp => dp.OrganizationId == organizationId
                             && dp.AnonymizedAt == null
                             && !dp.IsOnLegalHold)
                .Select(dp => new
                {
                    dp.DataPrincipalId,
                    dp.ReferenceId,
                    dp.CreatedAt,
                    dp.UpdatedAt
                })
                .ToListAsync();

            var result = new List<ErasureQueueDto>();

            foreach (var dp in principals)
            {
                RetentionPolicy? matchedPolicy = null;
                DateTime earliestDueDate = DateTime.MaxValue;

                foreach (var policy in activePolicies)
                {
                    var baseDate = ResolveTriggerBaseDate(policy.TriggerEvent, dp.CreatedAt, dp.UpdatedAt);
                    var dueDate = baseDate.AddDays(policy.RetentionPeriodInDays);

                    if (dueDate < earliestDueDate)
                    {
                        earliestDueDate = dueDate;
                        matchedPolicy = policy;
                    }
                }

                if (matchedPolicy == null) continue;

                if (earliestDueDate <= now.AddDays(UpcomingWindowDays))
                {
                    result.Add(new ErasureQueueDto
                    {
                        RecordId = dp.DataPrincipalId,
                        ReferenceId = dp.ReferenceId,
                        PolicyName = matchedPolicy.PolicyName,
                        ActionType = matchedPolicy.ActionType,
                        DueDate = earliestDueDate,
                        Status = earliestDueDate < now ? "Overdue" : "Pending"
                    });
                }
            }

            return result.OrderBy(q => q.DueDate).ToList();
        }

        public async Task<bool> SetLegalHoldAsync(Guid dataPrincipalId, Guid organizationId, bool isOnLegalHold, string? reason)
        {
            var dp = await _context.DataPrincipals
                .FirstOrDefaultAsync(x => x.DataPrincipalId == dataPrincipalId && x.OrganizationId == organizationId);

            if (dp == null)
                return false;

            dp.IsOnLegalHold = isOnLegalHold;
            dp.LegalHoldReason = isOnLegalHold ? reason?.Trim() : null;
            dp.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ErasedRecordDto>> GetErasedRecordsAsync(Guid organizationId)
        {
            var policy = await _context.RetentionPolicies
                .Where(p => p.OrganizationId == organizationId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            var defaultPolicyName = policy?.PolicyName ?? "Retention Policy";
            var defaultActionType = string.IsNullOrWhiteSpace(policy?.ActionType) ? "Anonymize" : policy!.ActionType;

            return await _context.DataPrincipals
                .Where(dp => dp.OrganizationId == organizationId && dp.AnonymizedAt != null)
                .OrderByDescending(dp => dp.AnonymizedAt)
                .Select(dp => new ErasedRecordDto
                {
                    RecordId = dp.DataPrincipalId,
                    ReferenceId = dp.ReferenceId,
                    ErasedAt = dp.AnonymizedAt!.Value,
                    ActionType = defaultActionType,
                    PolicyName = defaultPolicyName
                })
                .ToListAsync();
        }
    }
}