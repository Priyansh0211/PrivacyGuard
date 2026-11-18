using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Models;
using PrivacyGuard.Interfaces.Repositories;
// Note: Replace with your actual DbContext namespace if different
using PrivacyGuard.Data;

namespace PrivacyGuard.Repositories
{
    public class RuleAndActRepository : IRuleAndActRepository
    {
        private readonly ApplicationDbContext _context;

        public RuleAndActRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RuleAndAct>> GetAllByOrganizationIdAsync(Guid orgId)
        {
            return await _context.RulesAndActs
                .Where(r => r.OrganizationId == orgId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<RuleAndAct?> GetByIdAsync(Guid id)
        {
            return await _context.RulesAndActs
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
        }

        public async Task<RuleAndAct> AddAsync(RuleAndAct rule)
        {
            _context.RulesAndActs.Add(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<RuleAndAct> UpdateAsync(RuleAndAct rule)
        {
            rule.UpdatedAt = DateTime.UtcNow;
            _context.RulesAndActs.Update(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task DeleteAsync(Guid id)
        {
            var rule = await _context.RulesAndActs.FindAsync(id);
            if (rule != null)
            {
                rule.IsActive = false; // Soft delete
                rule.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}