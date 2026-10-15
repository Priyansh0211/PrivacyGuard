using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        { 
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetUsersByOrganizationIdAsync(Guid organizationId)
        {
            
            return await _context.Users
                .Where(u => u.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAndOrgAsync(Guid userId, Guid organizationId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId && u.OrganizationId == organizationId);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}