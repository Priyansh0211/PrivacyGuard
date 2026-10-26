using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface ITenantRepository
    {
        
        Task<(Organization org, User admin)> CreateTenantWithAdminAsync(Organization org, User admin);

        Task<User?> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);

        Task<List<string>> GetPermissionsByRoleIdAsync(Guid? roleId);

    }
}
