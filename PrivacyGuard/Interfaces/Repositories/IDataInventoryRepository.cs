using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IDataInventoryRepository
    {
        Task<DataInventory?> GetByIdAsync(
            Guid id,
            Guid organizationId);

        Task<List<DataInventory>> GetAllAsync(
            Guid organizationId);

        Task AddAsync(DataInventory inventory);

        Task UpdateAsync(DataInventory inventory);

        Task DeleteAsync(DataInventory inventory);
        Task<bool> ExistsAsync(
            Guid id,
            Guid organizationId);
    }
}