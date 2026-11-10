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

        public RetentionPolicyService(IRetentionPolicyRepository repository)
        {
            _repository = repository;
        }

        public async Task<RetentionPolicyDto?> GetPolicyByIdAsync(Guid policyId, Guid organizationId)
        {
            var policy = await _repository.GetPolicyByIdAsync(policyId, organizationId);
            return policy == null ? null : MapToDto(policy);
        }

        public async Task<IEnumerable<RetentionPolicyDto>> GetAllPoliciesAsync(Guid organizationId)
        {
            var policies = await _repository.GetAllPoliciesAsync(organizationId);
            return policies.Select(MapToDto).ToList();
        }

        public async Task<RetentionPolicyDto> CreatePolicyAsync(CreateRetentionPolicyDto dto, Guid organizationId)
        {
            var newPolicy = new RetentionPolicy
            {
                PolicyId = Guid.NewGuid(),
                OrganizationId = organizationId,
                PolicyName = dto.PolicyName,
                Description = dto.Description,
                RetentionPeriodInDays = dto.RetentionPeriodInDays,
                TriggerEvent = dto.TriggerEvent,
                ActionType = dto.ActionType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var savedPolicy = await _repository.AddPolicyAsync(newPolicy);
            return MapToDto(savedPolicy);
        }

        public async Task<RetentionPolicyDto?> UpdatePolicyAsync(Guid policyId, UpdateRetentionPolicyDto dto, Guid organizationId)
        {
            var existingPolicy = await _repository.GetPolicyByIdAsync(policyId, organizationId);
            if (existingPolicy == null) return null;

            existingPolicy.PolicyName = dto.PolicyName;
            existingPolicy.Description = dto.Description;
            existingPolicy.RetentionPeriodInDays = dto.RetentionPeriodInDays;
            existingPolicy.IsActive = dto.IsActive;

            var updatedPolicy = await _repository.UpdatePolicyAsync(existingPolicy);
            return MapToDto(updatedPolicy);
        }

        public async Task<bool> DeletePolicyAsync(Guid policyId, Guid organizationId)
        {
            return await _repository.DeletePolicyAsync(policyId, organizationId);
        }

        public async Task ExecuteDailyErasureJobAsync()
        {
            var activePolicies = await _repository.GetAllActivePoliciesForJobAsync();

            foreach (var policy in activePolicies)
            {
                var targetExpiryDate = DateTime.UtcNow.AddDays(-policy.RetentionPeriodInDays);
                await _repository.ProcessExpiredDataAsync(policy.ActionType, targetExpiryDate);
            }
        }
        // ==========================================
        public async Task<RetentionMetricsDto> GetDashboardMetricsAsync(Guid organizationId)
        {
            // Repo ko bol kar dashboard ka data mangwaya hai
            return await _repository.GetDashboardMetricsAsync(organizationId);
        }

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
    }
}