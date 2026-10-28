using Microsoft.EntityFrameworkCore;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using PrivacyGuard.Repositories;
using static PrivacyGuard.DTOs.RolePermissionDto;

namespace PrivacyGuard.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<ModulePermissionDto>> GetGroupedPermissionsAsync()
        {
            var permissions = await _roleRepository.GetAllPermissionsAsync();

            // LINQ grouping ka saara logic ab yahan Service layer mein aayega
            var groupedPermissions = permissions
                .GroupBy(p => p.ModuleName)
                .Select(g => new ModulePermissionDto
                {
                    ModuleName = g.Key,
                    Actions = g.Select(p => new PermissionActionDto
                    {
                        PermissionId = p.Id,
                        ActionName = p.ActionName
                    }).ToList()
                })
                .ToList();

            return groupedPermissions;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> CreateCustomRoleAsync(CreateRoleDto request, Guid organizationId)
        {
            // 1. नया Role ऑब्जेक्ट बनाओ
            var newRole = new Role
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Name = request.RoleName,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            // 2. जो चेकबॉक्स टिक हुए हैं, उनकी मैपिंग बनाओ
            var rolePermissions = request.PermissionIds.Select(permissionId => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = newRole.Id,
                PermissionId = permissionId
            }).ToList();

            // 3. रिपॉजिटरी को दे दो सेव करने के लिए
            var result = await _roleRepository.CreateRoleWithPermissionsAsync(newRole, rolePermissions);

            if (!result)
            {
                return (false, "Something went wrong while saving the role and permissions to the database.");
            }

            return (true, string.Empty);
        }


       
        public async Task<List<ModulePermissionDto>> GetUserGroupedPermissionsAsync(Guid userId)
        {
            var permissions = await _roleRepository.GetUserPermissionDetailsAsync(userId);

            var groupedPermissions = permissions
                .GroupBy(p => p.ModuleName)
                .Select(g => new ModulePermissionDto
                {
                    ModuleName = g.Key,
                    Actions = g.Select(p => new PermissionActionDto
                    {
                        PermissionId = p.Id,
                        ActionName = p.ActionName
                    }).ToList()
                })
                .ToList();

            return groupedPermissions;
        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {

            return await _roleRepository.GetUserPermissionsAsync(userId);
        }

        public async Task<RolePermissionResponseDto?> GetRoleForEditAsync(Guid roleId, Guid organizationId)
        {
            return await _roleRepository.GetRoleForEditAsync(roleId, organizationId);
        }

        public async Task<bool> UpdateRoleAndPermissionsAsync(Guid roleId, Guid organizationId, UpdateRoleDto dto)
        {
            
            if (dto.Name.Trim().Equals("SystemAdmin", StringComparison.OrdinalIgnoreCase) ||
                dto.Name.Trim().Equals("DPO", StringComparison.OrdinalIgnoreCase))
            {
                // Tum chaho toh is check ko hata bhi sakte ho agar tumhe Admin edit karna allow karna hai
            }

            return await _roleRepository.UpdateRoleAndPermissionsAsync(
                roleId,
                organizationId,
                dto.Name,
                dto.Description,
                dto.PermissionIds
            );
        }
    }
}