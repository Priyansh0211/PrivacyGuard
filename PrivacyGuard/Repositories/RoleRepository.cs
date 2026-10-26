using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        public async Task<bool> CreateRoleWithPermissionsAsync(Role role, List<RolePermission> rolePermissions)
        {
            // Transaction शुरू कर रहे हैं ताकि Role और Permissions दोनों एक साथ सेव हों
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Role सेव करो
                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();

                // 2. मैप्ड परमिशन्स सेव करो
                if (rolePermissions.Any())
                {
                    await _context.RolePermissions.AddRangeAsync(rolePermissions);
                    await _context.SaveChangesAsync();
                }

                // सब सही रहा तो Transaction कमिट कर दो
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                // कुछ भी फटा तो रोलबैक कर दो
                await transaction.RollbackAsync();
                return false;
            }
        }

      

        
        public async Task<List<Permission>> GetUserPermissionDetailsAsync(Guid userId)
        {
            return await (from u in _context.Users
                          where u.UserId == userId
                          join rp in _context.RolePermissions on u.RoleId equals rp.RoleId
                          join p in _context.Permissions on rp.PermissionId equals p.Id
                          select p).Distinct().ToListAsync();
        }


        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {

            var permissions = await (from u in _context.Users
                                     where u.UserId == userId
                                     join rp in _context.RolePermissions on u.RoleId equals rp.RoleId
                                     join p in _context.Permissions on rp.PermissionId equals p.Id
                                     select p.SystemName).Distinct().ToListAsync();

            return permissions;
        }
    }
}