using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IRuleAndActService
    {
        Task<IEnumerable<RuleResponseDto>> GetRulesAsync(Guid orgId);
        Task<RuleResponseDto?> GetRuleByIdAsync(Guid id);
        Task<RuleResponseDto> CreateRuleAsync(CreateRuleDto dto);
        Task<bool> DeleteRuleAsync(Guid id);
    }
}