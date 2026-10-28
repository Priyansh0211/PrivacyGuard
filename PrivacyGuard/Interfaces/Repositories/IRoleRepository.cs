using PrivacyGuard.Models;
using static PrivacyGuard.DTOs.RolePermissionDto;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<List<Permission>> GetAllPermissionsAsync();

        Task<bool> CreateRoleWithPermissionsAsync(Role role, List<RolePermission> rolePermissions);

        
        Task<List<Permission>> GetUserPermissionDetailsAsync(Guid userId);

        Task<List<string>> GetUserPermissionsAsync(Guid userId);

        Task<RolePermissionResponseDto?> GetRoleForEditAsync(Guid roleId, Guid organizationId);

        Task<bool> UpdateRoleAndPermissionsAsync(Guid roleId, Guid organizationId, string name, string description, List<int> permissionIds);
    }

}
