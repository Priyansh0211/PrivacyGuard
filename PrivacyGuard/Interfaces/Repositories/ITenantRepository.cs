using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface ITenantRepository
    {
        Task<(Organization org, User admin)> CreateTenantWithAdminAsync(Organization org, User admin);
        Task<User?> GetUserByEmailAsync(string email);
        Task<DataPrincipal?> GetDataPrincipalByEmailAsync(string email);
        Task UpdateUserAsync(User user);
        Task UpdateDataPrincipalAsync(DataPrincipal dataPrincipal);
        Task<bool> IsOrganizationActiveAsync(Guid organizationId);
        Task<List<string>> GetPermissionsByRoleIdAsync(Guid? roleId);
        Task<List<RoleDropdownDto>> GetRolesForDropdownAsync(Guid organizationId);

    }
}
