using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        { 
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<List<User>> GetUsersByOrganizationIdAsync(Guid organizationId)
        {
            return await _context.Users
                 .AsNoTracking()
                 .Include(u => u.UserRole)
                 .Include(u => u.OrgUnit)
                 // Role-less staff bhi dikhne chahiye; sirf legacy DataPrincipal-role wale hide
                 .Where(u => u.OrganizationId == organizationId
                             && (u.UserRole == null || u.UserRole.Name != "DataPrincipal"))
                 .ToListAsync();
        }
        public async Task<List<DataPrincipal>> GetDataPrincipalsByOrganizationIdAsync(Guid organizationId)
        {
            return await _context.DataPrincipals
                .AsNoTracking()
                .Where(dp => dp.OrganizationId == organizationId && dp.AnonymizedAt == null)
                .ToListAsync();
        }
        public async Task<User?> GetUserByIdAndOrgAsync(Guid userId, Guid organizationId)
        {
            return await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.OrganizationId == organizationId);
        }
        public async Task<Role?> GetRoleInOrganizationAsync(Guid roleId, Guid organizationId)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roleId && r.OrganizationId == organizationId);
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task<OrgUnit?> GetActiveOrgUnitInOrganizationAsync(Guid orgUnitId, Guid organizationId)
        {
            return await _context.OrgUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orgUnitId && o.OrganizationId == organizationId && o.IsActive);
        }
    }
}