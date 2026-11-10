using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
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
                .OrderByDescending(p => p.CreatedAt) // Latest upar dikhegi
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
            if (policy == null)
            {
                return false;
            }

            _context.RetentionPolicies.Remove(policy);
            await _context.SaveChangesAsync();
            return true;
        }
        // RetentionPolicyRepository.cs mein class ke andar ye methods add kar do:

        public async Task<IEnumerable<RetentionPolicy>> GetAllActivePoliciesForJobAsync()
        {
            // Background job ke liye saari active policies nikal rahe hain (saari organizations ki)
            return await _context.RetentionPolicies
                .Where(p => p.IsActive == true)
                .ToListAsync();
        }

        public async Task ProcessExpiredDataAsync(string actionType, DateTime targetExpiryDate)
        {
            // DB se wo saare DataPrincipals nikalo jinki date expire ho chuki hai
            var expiredDataPrincipals = await _context.DataPrincipals
                .Where(dp => dp.CreatedAt <= targetExpiryDate)
                .ToListAsync();

            if (expiredDataPrincipals.Any())
            {
                foreach (var dp in expiredDataPrincipals)
                {
                    if (actionType == "Anonymize")
                    {
                        // Data ko mask/anonymize kar do
                        dp.FullName = "Anonymized_User";
                        dp.Email = "deleted@privacyguard.com";
                        dp.Mobile = "0000000000";
                        dp.ReferenceId = "DELETED-" + Guid.NewGuid().ToString().Substring(0, 8);
                        // dp.IsDeleted = true; // Agar tumhare DataPrincipal model mein soft delete property hai toh isko uncomment kar lena
                    }
                    else if (actionType == "Delete")
                    {
                        // Hard delete
                        _context.DataPrincipals.Remove(dp);
                    }
                }

                // Ek hi baar mein saare changes save kar lo
                await _context.SaveChangesAsync();
            }
        }
    }
}