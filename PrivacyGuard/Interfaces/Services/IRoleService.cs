using static PrivacyGuard.DTOs.RolePermissionDto;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IRoleService
    {
        Task<List<ModulePermissionDto>> GetGroupedPermissionsAsync();

        Task<(bool IsSuccess, string ErrorMessage)> CreateCustomRoleAsync(CreateRoleDto request, Guid organizationId);

        Task<List<ModulePermissionDto>> GetUserGroupedPermissionsAsync(Guid userId);

        Task<List<string>> GetUserPermissionsAsync(Guid userId);

        Task<RolePermissionResponseDto?> GetRoleForEditAsync(Guid roleId, Guid organizationId);

        Task<bool> UpdateRoleAndPermissionsAsync(Guid roleId, Guid organizationId, UpdateRoleDto dto);

    }
}
