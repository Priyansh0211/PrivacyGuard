using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<List<Permission>> GetAllPermissionsAsync();

        Task<bool> CreateRoleWithPermissionsAsync(Role role, List<RolePermission> rolePermissions);

        
        Task<List<Permission>> GetUserPermissionDetailsAsync(Guid userId);

        Task<List<string>> GetUserPermissionsAsync(Guid userId);
    }

}
