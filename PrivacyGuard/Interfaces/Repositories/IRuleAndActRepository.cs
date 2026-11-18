using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IRuleAndActRepository
    {
        Task<IEnumerable<RuleAndAct>> GetAllByOrganizationIdAsync(Guid orgId);
        Task<RuleAndAct?> GetByIdAsync(Guid id);
        Task<RuleAndAct> AddAsync(RuleAndAct rule);
        Task<RuleAndAct> UpdateAsync(RuleAndAct rule);
        Task DeleteAsync(Guid id);
    }
}