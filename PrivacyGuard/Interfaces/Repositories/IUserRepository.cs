using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<bool> CheckEmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);

        Task<List<User>> GetUsersByOrganizationIdAsync(Guid organizationId);
        Task<List<DataPrincipal>> GetDataPrincipalsByOrganizationIdAsync(Guid organizationId);

        Task<User?> GetUserByIdAndOrgAsync(Guid userId, Guid organizationId);
        Task UpdateUserAsync(User user);
        Task<Role?> GetRoleInOrganizationAsync(Guid roleId, Guid organizationId);
        Task<OrgUnit?> GetActiveOrgUnitInOrganizationAsync(Guid orgUnitId, Guid organizationId);
    }
}
