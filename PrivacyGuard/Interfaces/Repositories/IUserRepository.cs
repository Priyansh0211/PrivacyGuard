using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<bool> CheckEmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);

        Task<List<User>> GetUsersByOrganizationIdAsync(Guid organizationId);

        Task<User?> GetUserByIdAndOrgAsync(Guid userId, Guid organizationId);
        Task UpdateUserAsync(User user);
    }
}
