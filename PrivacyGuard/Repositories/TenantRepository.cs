using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _context;

        public TenantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(Organization org, User admin)> CreateTenantWithAdminAsync(Organization org, User admin)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Organization ko add karo
                _context.Organizations.Add(org);

                // 2. Nayi OrganizationId ko Admin user ke sath link karo
                admin.OrganizationId = org.OrganizationId;
                _context.Users.Add(admin);

                // 3. Database me save karo aur transaction commit karo
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (org, admin);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Login ke baad LastLoginDate save karna
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
