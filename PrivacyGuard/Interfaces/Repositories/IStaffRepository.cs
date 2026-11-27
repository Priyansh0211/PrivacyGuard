using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IStaffRepository
    {
        Task<bool> StaffExistsAsync(string email);
        Task<User> AddStaffAsync(User user);
        Task<int> SaveChangesAsync();
        Task<List<User>> GetStaffByOrganizationIdAsync(Guid organizationId);
        Task<Role?> GetRoleInOrganizationAsync(Guid roleId, Guid organizationId);
        Task<OrgUnit?> GetActiveOrgUnitInOrganizationAsync(Guid orgUnitId, Guid organizationId);
    }
}