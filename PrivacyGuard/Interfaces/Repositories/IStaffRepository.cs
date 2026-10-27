using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IStaffRepository
    {
        Task<bool> StaffExistsAsync(string email);
        Task<User> AddStaffAsync(User user);
        Task<int> SaveChangesAsync();
        Task<List<User>> GetStaffByOrganizationIdAsync(Guid organizationId);
    }
}