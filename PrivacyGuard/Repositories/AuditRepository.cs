using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetLogsByOrganizationAsync(Guid organizationId)
        {
            return await _context.AuditLogs
                .Where(log => log.OrganizationId == organizationId)
                .OrderByDescending(log => log.Timestamp) // Latest logs top par aayenge
                .ToListAsync();
        }
    }
}
