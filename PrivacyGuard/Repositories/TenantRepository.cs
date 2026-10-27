using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _context;

        public TenantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(Organization org, User admin)> CreateTenantWithAdminAsync(Organization org, User admin)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                
                _context.Organizations.Add(org);

               
                if (admin.UserRole != null)
                {
                    _context.Roles.Add(admin.UserRole);

                   
                    var allPermissions = await _context.Permissions.ToListAsync();

                   
                    var rolePermissions = allPermissions.Select(p => new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = admin.UserRole.Id,
                        PermissionId = p.Id
                    }).ToList();

                    _context.RolePermissions.AddRange(rolePermissions);
                }

                admin.OrganizationId = org.OrganizationId;
                _context.Users.Add(admin);

                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (org, admin);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

      

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.Include(u => u.UserRole).FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetPermissionsByRoleIdAsync(Guid? roleId)
        {
            if (roleId == null || roleId == Guid.Empty)
            {
                return new List<string>();
            }

            return await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId && rp.Permission != null)
                .Select(rp => rp.Permission!.SystemName)
                .ToListAsync();
        }

        public async Task<List<RoleDropdownDto>> GetRolesForDropdownAsync(Guid organizationId)
        {
            return await _context.Roles
                .Where(r => r.OrganizationId == organizationId)
                .Select(r => new RoleDropdownDto
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();
        }
    }
}