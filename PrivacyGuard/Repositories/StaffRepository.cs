using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly ApplicationDbContext _context;

        public StaffRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> StaffExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> AddStaffAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return user;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<List<User>> GetStaffByOrganizationIdAsync(Guid organizationId)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.OrgUnit)   // Branch name ke liye
                .Include(u => u.UserRole)  // Role name ke liye (pehle missing tha)
                .Where(u => u.OrganizationId == organizationId
                            && (u.UserRole == null || u.UserRole.Name != "DataPrincipal"))
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
        public async Task<Role?> GetRoleInOrganizationAsync(Guid roleId, Guid organizationId)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roleId && r.OrganizationId == organizationId);
        }

        public async Task<OrgUnit?> GetActiveOrgUnitInOrganizationAsync(Guid orgUnitId, Guid organizationId)
        {
            return await _context.OrgUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orgUnitId && o.OrganizationId == organizationId && o.IsActive);
        }
    }
}