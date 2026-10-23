using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class OrgUnitRepository : IOrgUnitRepository
    {
        private readonly ApplicationDbContext _context;

        public OrgUnitRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrgUnit> AddAsync(OrgUnit orgUnit)
        {
            await _context.OrgUnits.AddAsync(orgUnit);
            return orgUnit;
        }

        public async Task<OrgUnit?> GetByIdAsync(Guid id, Guid organizationId)
        {
            // Security check: ID aur OrganizationId dono match hone chahiye
            return await _context.OrgUnits
                .FirstOrDefaultAsync(o => o.Id == id && o.OrganizationId == organizationId);
        }

        public async Task<IEnumerable<OrgUnit>> GetAllByOrgIdAsync(Guid organizationId)
        {
            // Sirf ussi company ka data fetch karega
            return await _context.OrgUnits
                .Where(o => o.OrganizationId == organizationId)
                .AsNoTracking()
                .ToListAsync();
        }

        public void Update(OrgUnit orgUnit)
        {
            _context.OrgUnits.Update(orgUnit);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}