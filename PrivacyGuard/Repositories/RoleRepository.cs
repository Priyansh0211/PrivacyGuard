using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;
using static PrivacyGuard.DTOs.RolePermissionDto;

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

        public async Task<RolePermissionResponseDto?> GetRoleForEditAsync(Guid roleId, Guid organizationId)
        {
            // 1. Check karo ki Role exist karta hai aur usi organization ka hai
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.OrganizationId == organizationId);

            if (role == null) return null;

            // 2. System ki saari permissions nikal lo (1 se 18 tak)
            var allPermissions = await _context.Permissions.ToListAsync();

            // 3. Is specific role ko jo permissions mili hui hain, unki IDs nikal lo
            var assignedPermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            // 4. Dono ko map kardo
            var permissionCheckboxes = allPermissions.Select(p => new PermissionCheckboxDto
            {
                Id = p.Id,
                ModuleName = p.ModuleName,
                ActionName = p.ActionName,
                StstemName = p.SystemName,
                
                IsAssigned = assignedPermissionIds.Contains(p.Id)
            }).ToList();

            // 5. Final response bana kar bhej do
            return new RolePermissionResponseDto
            {
                RoleId = role.Id,
                Name = role.Name,
                Description = role.Description,
                Permissions = permissionCheckboxes
            };
        }

        public async Task<bool> UpdateRoleAndPermissionsAsync(Guid roleId, Guid organizationId, string name, string description, List<int> permissionIds)
        {
            // Transaction shuru kar rahe hain, taaki agar error aaye toh aadha data save na ho
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Check karo ki Role exist karta hai aur current Organization ka hi hai
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == roleId && r.OrganizationId == organizationId);

                if (role == null) return false;

                // 2. Role ka Name aur Description update karo
                role.Name = name;
                role.Description = description;
                _context.Roles.Update(role);

                // 3. Purani saari permissions ko DB se hata do (Taaki nayi fresh lag sakein)
                var existingPermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();

                if (existingPermissions.Any())
                {
                    _context.RolePermissions.RemoveRange(existingPermissions);
                }

                // 4. Nayi permissions insert karo (Jo DTO me aayi hain)
                if (permissionIds != null && permissionIds.Any())
                {
                    var newRolePermissions = permissionIds.Select(pId => new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = roleId,
                        PermissionId = pId
                    }).ToList();

                    _context.RolePermissions.AddRange(newRolePermissions);
                }

                // 5. Sab kuch ek sath save karke transaction commit kar do
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); // Kuch bhi fail hua toh undo ho jayega
                throw;
            }
        }
    }
}