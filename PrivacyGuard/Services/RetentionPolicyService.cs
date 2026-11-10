using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class RetentionPolicyService : IRetentionPolicyService
    {
        private readonly IRetentionPolicyRepository _repository;

        // Repository inject kar rahe hain taaki DB se baat kar sakein
        public RetentionPolicyService(IRetentionPolicyRepository repository)
        {
            _repository = repository;
        }

        public async Task<RetentionPolicyDto?> GetPolicyByIdAsync(Guid policyId, Guid organizationId)
        {
            // Repo se DB model mangwaya
            var policy = await _repository.GetPolicyByIdAsync(policyId, organizationId);
            if (policy == null) return null;

            // Model ko DTO mein map karke return kiya
            return MapToDto(policy);
        }

        public async Task<IEnumerable<RetentionPolicyDto>> GetAllPoliciesAsync(Guid organizationId)
        {
            // Repo se saari policies aayi
            var policies = await _repository.GetAllPoliciesAsync(organizationId);
            
            // LINQ Select use karke list of Models ko list of DTOs mein convert kar diya
            return policies.Select(MapToDto).ToList();
        }

        public async Task<RetentionPolicyDto> CreatePolicyAsync(CreateRetentionPolicyDto dto, Guid organizationId)
        {
            // Frontend ke DTO se naya Database Model banaya
            var newPolicy = new RetentionPolicy
            {
                PolicyId = Guid.NewGuid(), // Nayi unique ID generate ki
                OrganizationId = organizationId,
                PolicyName = dto.PolicyName,
                Description = dto.Description,
                RetentionPeriodInDays = dto.RetentionPeriodInDays,
                TriggerEvent = dto.TriggerEvent,
                ActionType = dto.ActionType,
                IsActive = true, // By default naya rule active rahega
                CreatedAt = DateTime.UtcNow
            };

            // Repo ko bol kar DB mein save karwaya
            var savedPolicy = await _repository.AddPolicyAsync(newPolicy);

            // Response mein DTO bana kar bhej diya
            return MapToDto(savedPolicy);
        }

        public async Task<RetentionPolicyDto?> UpdatePolicyAsync(Guid policyId, UpdateRetentionPolicyDto dto, Guid organizationId)
        {
            // Pehle check kiya ki wo policy actually exist karti bhi hai ya nahi
            var existingPolicy = await _repository.GetPolicyByIdAsync(policyId, organizationId);
            if (existingPolicy == null) return null;

            // Existing model ke purane data ko naye DTO data se overwrite kiya
            existingPolicy.PolicyName = dto.PolicyName;
            existingPolicy.Description = dto.Description;
            existingPolicy.RetentionPeriodInDays = dto.RetentionPeriodInDays;
            existingPolicy.IsActive = dto.IsActive;
            
            // Repo ko dekar update save karwaya (Repo UpdateAt property automatically set kar dega)
            var updatedPolicy = await _repository.UpdatePolicyAsync(existingPolicy);

            return MapToDto(updatedPolicy);
        }

        public async Task<bool> DeletePolicyAsync(Guid policyId, Guid organizationId)
        {
            // Seedha Repo ka delete method call kiya
            return await _repository.DeletePolicyAsync(policyId, organizationId);
        }

        // ==========================================
        // HELPER METHOD (Taki code baar baar na likhna pade)
        // ==========================================
        private RetentionPolicyDto MapToDto(RetentionPolicy policy)
        {
            return new RetentionPolicyDto
            {
                PolicyId = policy.PolicyId,
                PolicyName = policy.PolicyName,
                Description = policy.Description,
                RetentionPeriodInDays = policy.RetentionPeriodInDays,
                TriggerEvent = policy.TriggerEvent,
                ActionType = policy.ActionType,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt
            };
        }
        public async Task ExecuteDailyErasureJobAsync()
        {
            // 1. Repo se saari active policies mangwao (bina _context error ke)
            var activePolicies = await _repository.GetAllActivePoliciesForJobAsync();

            foreach (var policy in activePolicies)
            {
                // 2. Target date calculate karo
                var targetExpiryDate = DateTime.UtcNow.AddDays(-policy.RetentionPeriodInDays);

                // 3. Repo ko bol do ki is date se purana data utha aur policy ke hisaab se action le le"
                await _repository.ProcessExpiredDataAsync(policy.ActionType, targetExpiryDate);
            }
        }
    }
}