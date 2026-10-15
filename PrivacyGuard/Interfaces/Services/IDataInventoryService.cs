using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IDataInventoryService
    {
        Task<DataInventoryResponseDto?> GetByIdAsync(
            Guid id,
            Guid organizationId);

        Task<List<DataInventoryResponseDto>> GetAllAsync(
            Guid organizationId);

        Task<DataInventoryResponseDto> CreateAsync(
            CreateDataInventoryDto dto,
            Guid organizationId);

        Task<bool> UpdateAsync(
            Guid id,
            UpdateDataInventoryDto dto,
            Guid organizationId);

        Task<bool> DeleteAsync(
            Guid id,
            Guid organizationId);
    }
}