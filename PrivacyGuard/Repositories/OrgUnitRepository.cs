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

        // NAYA METHOD: Interface ko satisfy karne aur Controller se Delete call karne ke liye
        public void Delete(OrgUnit orgUnit)
        {
            _context.OrgUnits.Remove(orgUnit);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Users/Principals count dynamically nikalne ke liye
        public async Task<int> GetPrincipalsCountAsync(Guid orgUnitId)
        {
            // As per PDF: group Users by OrgUnitId and calculate the count
            return await _context.Users.CountAsync(u => u.OrgUnitId == orgUnitId);
        }

        public async Task<bool> IsUnitCodeUniqueAsync(string unitCode, Guid organizationId, Guid? excludeId = null)
        {
            var query = _context.OrgUnits.Where(o => o.OrganizationId == organizationId && o.UnitCode == unitCode);
            if (excludeId.HasValue) query = query.Where(o => o.Id != excludeId.Value);
            return !await query.AnyAsync();
        }

        // Deletion check ke liye
        public async Task<bool> HasChildrenAsync(Guid orgUnitId)
        {
            return await _context.OrgUnits.AnyAsync(o => o.ParentId == orgUnitId);
        }

        public async Task<bool> HasAssignedUsersAsync(Guid orgUnitId)
        {
            return await _context.Users.AnyAsync(u => u.OrgUnitId == orgUnitId);
        }
    }
}