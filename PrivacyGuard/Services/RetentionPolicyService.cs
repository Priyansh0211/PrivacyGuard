using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class RetentionPolicyService : IRetentionPolicyService
    {
        private readonly IRetentionPolicyRepository _repository;
        private readonly ILogger<RetentionPolicyService> _logger;

        private static readonly HashSet<string> AllowedTriggerEvents = new(StringComparer.OrdinalIgnoreCase)
        {
            "DataCreatedDate",
            "LastActivity"
        };

        private static readonly HashSet<string> AllowedActionTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Anonymize",
            "Delete"
        };

        public RetentionPolicyService(
            IRetentionPolicyRepository repository,
            ILogger<RetentionPolicyService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        private static string NormalizeTriggerEvent(string? triggerEvent)
        {
            var trimmed = triggerEvent?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                return "DataCreatedDate";

            if (!AllowedTriggerEvents.Contains(trimmed))
                throw new ArgumentException("Unsupported trigger event. Allowed values are 'DataCreatedDate' or 'LastActivity'.");

            return string.Equals(trimmed, "LastActivity", StringComparison.OrdinalIgnoreCase)
                ? "LastActivity"
                : "DataCreatedDate";
        }

        private static string NormalizeActionType(string? actionType)
        {
            var trimmed = actionType?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || !AllowedActionTypes.Contains(trimmed))
                throw new ArgumentException("Unsupported action type. Allowed values are 'Anonymize' or 'Delete'.");

            return string.Equals(trimmed, "Delete", StringComparison.OrdinalIgnoreCase)
                ? "Delete"
                : "Anonymize";
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
            var normalizedTrigger = NormalizeTriggerEvent(dto.TriggerEvent);
            var normalizedAction = NormalizeActionType(dto.ActionType);

            var newPolicy = new RetentionPolicy
            {
                PolicyId = Guid.NewGuid(),
                OrganizationId = organizationId,
                PolicyName = dto.PolicyName.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                RetentionPeriodInDays = dto.RetentionPeriodInDays,
                TriggerEvent = normalizedTrigger,
                ActionType = normalizedAction,
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

            var normalizedTrigger = NormalizeTriggerEvent(dto.TriggerEvent);

            existingPolicy.PolicyName = dto.PolicyName.Trim();
            existingPolicy.Description = dto.Description?.Trim() ?? string.Empty;
            existingPolicy.RetentionPeriodInDays = dto.RetentionPeriodInDays;
            existingPolicy.TriggerEvent = normalizedTrigger;
            existingPolicy.IsActive = dto.IsActive;

            var updatedPolicy = await _repository.UpdatePolicyAsync(existingPolicy);
            return MapToDto(updatedPolicy);
        }

        public async Task<bool> DeletePolicyAsync(Guid policyId, Guid organizationId)
        {
            return await _repository.DeletePolicyAsync(policyId, organizationId);
        }

        public async Task<int> ExecuteDailyErasureJobAsync()
        {
            var activePolicies = (await _repository.GetAllActivePoliciesForJobAsync()).ToList();
            return await RunPoliciesAsync(activePolicies);
        }

        public async Task<int> ExecuteErasureJobForOrganizationAsync(Guid organizationId)
        {
            var activePolicies = (await _repository.GetActivePoliciesByOrganizationAsync(organizationId)).ToList();
            return await RunPoliciesAsync(activePolicies);
        }

        private async Task<int> RunPoliciesAsync(List<RetentionPolicy> policies)
        {
            int totalProcessed = 0;

            foreach (var policy in policies)
            {
                try
                {
                    var normalizedTrigger = NormalizeTriggerEvent(policy.TriggerEvent);
                    var normalizedAction = NormalizeActionType(policy.ActionType);

                    var processed = await _repository.ProcessExpiredDataAsync(
                        policy.OrganizationId,
                        policy.RetentionPeriodInDays,
                        normalizedAction,
                        normalizedTrigger);

                    totalProcessed += processed;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error executing retention policy {PolicyId} ({PolicyName}) for Org {OrganizationId}",
                        policy.PolicyId, policy.PolicyName, policy.OrganizationId);
                }
            }

            return totalProcessed;
        }

        public async Task<RetentionMetricsDto> GetDashboardMetricsAsync(Guid organizationId)
        {
            return await _repository.GetDashboardMetricsAsync(organizationId);
        }

        public async Task<IEnumerable<ErasureQueueDto>> GetErasureQueueAsync(Guid organizationId)
        {
            return await _repository.GetErasureQueueAsync(organizationId);
        }

        public async Task<bool> SetLegalHoldAsync(Guid dataPrincipalId, Guid organizationId, LegalHoldRequestDto dto)
        {
            return await _repository.SetLegalHoldAsync(
                dataPrincipalId,
                organizationId,
                dto.IsOnLegalHold,
                dto.Reason);
        }

        public async Task<IEnumerable<ErasedRecordDto>> GetErasedRecordsAsync(Guid organizationId)
        {
            return await _repository.GetErasedRecordsAsync(organizationId);
        }

        private static RetentionPolicyDto MapToDto(RetentionPolicy policy)
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