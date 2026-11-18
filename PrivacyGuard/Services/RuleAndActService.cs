using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Services
{
    public class RuleAndActService : IRuleAndActService
    {
        private readonly IRuleAndActRepository _repository;

        public RuleAndActService(IRuleAndActRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RuleResponseDto>> GetRulesAsync(Guid orgId)
        {
            var rules = await _repository.GetAllByOrganizationIdAsync(orgId);

            // Demo Hack: Return dummy data if DB is empty to prevent UI console errors
            if (!rules.Any())
            {
                return new List<RuleResponseDto> {
                    new RuleResponseDto {
                        Id = Guid.NewGuid(),
                        OrganizationId = orgId,
                        Title = "DPDP Act 2023 Guidelines",
                        Description = "Official Government DPDP framework.",
                        Category = "DPDP_Act",
                        DocumentUrl = "https://www.meity.gov.in/writereaddata/files/Digital%20Personal%20Data%20Protection%20Act%202023.pdf",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    }
                };
            }

            return rules.Select(r => new RuleResponseDto
            {
                Id = r.Id,
                OrganizationId = r.OrganizationId,
                Title = r.Title,
                Description = r.Description,
                Category = r.Category,
                DocumentUrl = r.DocumentUrl,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsActive = r.IsActive
            });
        }

        public async Task<RuleResponseDto?> GetRuleByIdAsync(Guid id)
        {
            var rule = await _repository.GetByIdAsync(id);
            if (rule == null) return null;

            return new RuleResponseDto { Id = rule.Id, OrganizationId = rule.OrganizationId, Title = rule.Title, Description = rule.Description, Category = rule.Category, DocumentUrl = rule.DocumentUrl, CreatedAt = rule.CreatedAt, UpdatedAt = rule.UpdatedAt, IsActive = rule.IsActive };
        }

        public async Task<RuleResponseDto> CreateRuleAsync(CreateRuleDto dto)
        {
            var currentTime = DateTime.UtcNow;
            var rule = new RuleAndAct
            {
                Id = Guid.NewGuid(),
                OrganizationId = dto.OrganizationId,
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                DocumentUrl = dto.DocumentUrl,
                CreatedAt = currentTime,
                UpdatedAt = currentTime,
                IsActive = true
            };

            var savedRule = await _repository.AddAsync(rule);

            return new RuleResponseDto { Id = savedRule.Id, OrganizationId = savedRule.OrganizationId, Title = savedRule.Title, Description = savedRule.Description, Category = savedRule.Category, DocumentUrl = savedRule.DocumentUrl, CreatedAt = savedRule.CreatedAt, UpdatedAt = savedRule.UpdatedAt, IsActive = savedRule.IsActive };
        }

        public async Task<bool> DeleteRuleAsync(Guid id)
        {
            var rule = await _repository.GetByIdAsync(id);
            if (rule == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}