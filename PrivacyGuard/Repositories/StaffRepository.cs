using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly ApplicationDbContext _context;

        public StaffRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> StaffExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> AddStaffAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return user;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<List<User>> GetStaffByOrganizationIdAsync(Guid organizationId)
        {
            return await _context.Users // (Agar DbSet ka naam alag hai toh update kar lena)
                .Include(u => u.OrgUnit) // Yeh SQL JOIN ka kaam karega Branch Name laane ke liye
                .Where(u => u.OrganizationId == organizationId)
                .ToListAsync();
        }
    }
}