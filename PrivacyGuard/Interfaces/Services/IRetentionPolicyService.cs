using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IRetentionPolicyService
    {
        // Ek policy ki details DTO format mein laane ke liye
        Task<RetentionPolicyDto?> GetPolicyByIdAsync(Guid policyId, Guid organizationId);

        // Frontend grid/list ke liye saari policies laane ke liye
        Task<IEnumerable<RetentionPolicyDto>> GetAllPoliciesAsync(Guid organizationId);

        // Nayi policy create karke uska response DTO mein bhejne ke liye
        Task<RetentionPolicyDto> CreatePolicyAsync(CreateRetentionPolicyDto dto, Guid organizationId);

        // Existing policy ko update karne ke liye
        Task<RetentionPolicyDto?> UpdatePolicyAsync(Guid policyId, UpdateRetentionPolicyDto dto, Guid organizationId);

        // Policy ko soft/hard delete karne ke liye
        Task<bool> DeletePolicyAsync(Guid policyId, Guid organizationId);
        // Background job is method ko daily call karegi
        Task ExecuteDailyErasureJobAsync();
    }
}