using Microsoft.EntityFrameworkCore;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using PrivacyGuard.Repositories;
using PrivacyGuard.Helpers;
using PrivacyGuard.DTOs;
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

        public async Task<List<RoleSummaryDto>> GetRoleSummariesAsync(Guid organizationId)
        {
            return await _roleRepository.GetRoleSummariesAsync(organizationId);
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
        public async Task<(bool IsSuccess, string ErrorMessage)> CreateCustomRoleAsync(CreateRoleDto request, Guid organizationId, Guid callerUserId, string? callerRole)
        {
            bool callerIsSuper = RoleGuard.IsSuperRole(callerRole);

            var name = request.RoleName?.Trim();
            var nameCheck = await ValidateRoleNameAsync(name, organizationId, null, callerIsSuper);
            if (!nameCheck.IsValid)
                return (false, nameCheck.Error);

            var permCheck = await ValidatePermissionIdsAsync(request.PermissionIds, callerUserId, callerIsSuper);
            if (!permCheck.IsValid)
                return (false, permCheck.Error);

            var newRole = new Role
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Name = name!,
                Description = request.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var rolePermissions = permCheck.PermissionIds.Select(permissionId => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = newRole.Id,
                PermissionId = permissionId
            }).ToList();

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

        public async Task<(bool IsSuccess, bool NotFound, string ErrorMessage)> UpdateRoleAndPermissionsAsync(
            Guid roleId, Guid organizationId, UpdateRoleDto dto, Guid callerUserId, string? callerRole)
        {
            var existing = await _roleRepository.GetRoleAsync(roleId, organizationId);
            if (existing == null)
                return (false, true, "Role not found.");

            bool callerIsSuper = RoleGuard.IsSuperRole(callerRole);

            // Admin level role ko sirf SuperAdmin/SystemAdmin edit kar sakta hai
            if (RoleGuard.IsPrivilegedRole(existing.Name) && !callerIsSuper)
                return (false, false, "You are not allowed to modify this role.");

            var newName = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(newName))
                return (false, false, "Role name is required.");

            bool nameChanged = !string.Equals(existing.Name, newName, StringComparison.OrdinalIgnoreCase);
            if (nameChanged)
            {
                // System roles ka naam kabhi nahi badalna (RoleGuard naam par chalta hai)
                if (RoleGuard.IsPrivilegedRole(existing.Name) || RoleGuard.IsDataPrincipalRole(existing.Name))
                    return (false, false, "System roles cannot be renamed.");

                var nameCheck = await ValidateRoleNameAsync(newName, organizationId, roleId, callerIsSuper);
                if (!nameCheck.IsValid)
                    return (false, false, nameCheck.Error);
            }

            var permCheck = await ValidatePermissionIdsAsync(dto.PermissionIds, callerUserId, callerIsSuper);
            if (!permCheck.IsValid)
                return (false, false, permCheck.Error);

            var finalPermissionIds = permCheck.PermissionIds;

            if (!callerIsSuper)
            {
                // Jo permissions caller ke paas nahi hain, wo role me jaise hain waise hi rahengi
                var callerIds = (await _roleRepository.GetUserPermissionDetailsAsync(callerUserId))
                    .Select(p => p.Id).ToHashSet();

                var current = await _roleRepository.GetRoleForEditAsync(roleId, organizationId);
                var preserved = current?.Permissions
                    .Where(p => p.IsAssigned && !callerIds.Contains(p.PermissionId))
                    .Select(p => p.PermissionId)
                    ?? Enumerable.Empty<int>();

                finalPermissionIds = finalPermissionIds.Union(preserved).ToList();
            }

            var updated = await _roleRepository.UpdateRoleAndPermissionsAsync(
                roleId,
                organizationId,
                newName,
                dto.Description?.Trim() ?? string.Empty,
                finalPermissionIds);

            return updated ? (true, false, string.Empty) : (false, true, "Role not found.");
        }
        public async Task<List<ModulePermissionDto>?> GetUserGroupedPermissionsForOrgAsync(Guid userId, Guid organizationId)
        {
            // Doosri organization ke user ki permissions nahi dikhani
            if (!await _roleRepository.UserBelongsToOrganizationAsync(userId, organizationId))
                return null;

            return await GetUserGroupedPermissionsAsync(userId);
        }

        // ---------- private helpers ----------

        private async Task<(bool IsValid, string Error)> ValidateRoleNameAsync(
            string? name, Guid organizationId, Guid? excludeRoleId, bool callerIsSuper)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Role name is required.");

            if (RoleGuard.IsDataPrincipalRole(name))
                return (false, "This role name is reserved.");

            if (RoleGuard.IsPrivilegedRole(name) && !callerIsSuper)
                return (false, "This role name is reserved for administrators.");

            if (await _roleRepository.RoleNameExistsAsync(organizationId, name, excludeRoleId))
                return (false, "A role with this name already exists.");

            return (true, string.Empty);
        }

        private async Task<(bool IsValid, string Error, List<int> PermissionIds)> ValidatePermissionIdsAsync(
            List<int>? requested, Guid callerUserId, bool callerIsSuper)
        {
            var ids = (requested ?? new List<int>()).Distinct().ToList();

            // Saari permission IDs system me exist karni chahiye
            var allIds = (await _roleRepository.GetAllPermissionsAsync()).Select(p => p.Id).ToHashSet();
            if (ids.Any(id => !allIds.Contains(id)))
                return (false, "One or more selected permissions are invalid.", ids);

            // Non-super caller sirf wahi permissions de sakta hai jo uske paas khud hain
            if (!callerIsSuper)
            {
                var callerIds = (await _roleRepository.GetUserPermissionDetailsAsync(callerUserId))
                    .Select(p => p.Id).ToHashSet();

                if (ids.Any(id => !callerIds.Contains(id)))
                    return (false, "You cannot grant permissions that you do not have.", ids);
            }

            return (true, string.Empty, ids);
        }
    }
}