using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IRetentionPolicyRepository
    {
        // Ek specific policy laane ke liye
        Task<RetentionPolicy?> GetPolicyByIdAsync(Guid policyId, Guid organizationId);

        // Kisi organization ki saari policies laane ke liye
        Task<IEnumerable<RetentionPolicy>> GetAllPoliciesAsync(Guid organizationId);

        // Nayi policy database me save karne ke liye
        Task<RetentionPolicy> AddPolicyAsync(RetentionPolicy policy);

        // Existing policy ko update karne ke liye
        Task<RetentionPolicy> UpdatePolicyAsync(RetentionPolicy policy);

        // Policy ko delete karne ke liye
        Task<bool> DeletePolicyAsync(Guid policyId, Guid organizationId);

        // Background worker ke liye saari active policies laane ka method
        Task<IEnumerable<RetentionPolicy>> GetAllActivePoliciesForJobAsync();
        // Sirf ek org ki active policies
        Task<IEnumerable<RetentionPolicy>> GetActivePoliciesByOrganizationAsync(Guid organizationId);

        // Org-scoped, policy-driven erasure/anonymization returning processed count
        Task<int> ProcessExpiredDataAsync(Guid organizationId, int retentionPeriodInDays, string actionType, string triggerEvent); Task<RetentionMetricsDto> GetDashboardMetricsAsync(Guid organizationId);
        
        // Naya method Erasure Queue ke liye
        Task<IEnumerable<ErasureQueueDto>> GetErasureQueueAsync(Guid organizationId);
    }
}