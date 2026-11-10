using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class RetentionPolicyRepository : IRetentionPolicyRepository
    {
        private readonly ApplicationDbContext _context;

        public RetentionPolicyRepository(ApplicationDbContext context)
        {
            _context = context;
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
                .Where(p => p.IsActive == true)
                .ToListAsync();
        }

        public async Task ProcessExpiredDataAsync(string actionType, DateTime targetExpiryDate)
        {
            var expiredDataPrincipals = await _context.DataPrincipals
                .Where(dp => dp.CreatedAt <= targetExpiryDate)
                .ToListAsync();

            if (expiredDataPrincipals.Any())
            {
                foreach (var dp in expiredDataPrincipals)
                {
                    if (actionType == "Anonymize")
                    {
                        dp.FullName = "Anonymized_User";
                        dp.Email = "deleted@privacyguard.com";
                        dp.Mobile = "0000000000";
                        dp.ReferenceId = "DELETED-" + Guid.NewGuid().ToString().Substring(0, 8);
                    }
                    else if (actionType == "Delete")
                    {
                        _context.DataPrincipals.Remove(dp);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        // ==========================================
        // YAHAN HAIN DASHBOARD WALI QUERIES
        // ==========================================
        public async Task<RetentionMetricsDto> GetDashboardMetricsAsync(Guid organizationId)
        {
            var activePoliciesCount = await _context.RetentionPolicies
                .CountAsync(p => p.OrganizationId == organizationId && p.IsActive == true);

            var overdueCount = await _context.DataPrincipals
                .CountAsync(dp => dp.OrganizationId == organizationId && dp.CreatedAt < DateTime.UtcNow.AddDays(-30));

            var queueCount = await _context.DataPrincipals
                .CountAsync(dp => dp.OrganizationId == organizationId && dp.CreatedAt >= DateTime.UtcNow.AddDays(-30) && dp.CreatedAt < DateTime.UtcNow.AddDays(-23));

            return new RetentionMetricsDto
            {
                ActivePolicies = activePoliciesCount,
                Overdue = overdueCount,
                JobsInQueue = queueCount
            };
        }
    }
}