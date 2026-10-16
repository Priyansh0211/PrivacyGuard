using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class DataInventoryRepository : IDataInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public DataInventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataInventory?> GetByIdAsync(
            Guid id,
            Guid organizationId)
        {
            return await _context.DataInventories
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.DataInventoryId == id &&
                    x.OrganizationId == organizationId);
        }

        public async Task<List<DataInventory>> GetAllAsync(
            Guid organizationId)
        {
            return await _context.DataInventories
                .AsNoTracking()
                .Where(x => x.OrganizationId == organizationId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(DataInventory inventory)
        {
            await _context.DataInventories.AddAsync(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DataInventory inventory)
        {
            _context.DataInventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(DataInventory inventory)
        {
            _context.DataInventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(
            Guid id,
            Guid organizationId)
        {
            return await _context.DataInventories
                .AnyAsync(x =>
                    x.DataInventoryId == id &&
                    x.OrganizationId == organizationId);
        }
    }
}