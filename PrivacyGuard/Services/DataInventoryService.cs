using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class DataInventoryService : IDataInventoryService
    {
        private readonly IDataInventoryRepository _repository;

        public DataInventoryService(
            IDataInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<DataInventoryResponseDto?> GetByIdAsync(
            Guid id,
            Guid organizationId)
        {
            var inventory = await _repository.GetByIdAsync(
                id, organizationId);

            if (inventory == null)
                return null;

            return MapToResponseDto(inventory);
        }

        public async Task<List<DataInventoryResponseDto>> GetAllAsync(
            Guid organizationId)
        {
            var inventories = await _repository.GetAllAsync(
                organizationId);

            return inventories
                .Select(MapToResponseDto)
                .ToList();
        }

        public async Task<DataInventoryResponseDto> CreateAsync(
            CreateDataInventoryDto dto,
            Guid organizationId)
        {
            var inventory = new DataInventory
            {
                OrganizationId = organizationId,
                Name = dto.Name.Trim(),
                DataCategory = dto.DataCategory.Trim(),
                Purpose = dto.Purpose.Trim(),
                DataSource = dto.DataSource?.Trim(),
                StorageLocation = dto.StorageLocation?.Trim(),
                RetentionPeriodDays = dto.RetentionPeriodDays,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(inventory);

            return MapToResponseDto(inventory);
        }

        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateDataInventoryDto dto,
            Guid organizationId)
        {
            var inventory = await _repository.GetByIdAsync(
                id, organizationId);

            if (inventory == null)
                return false;

            inventory.Name = dto.Name.Trim();
            inventory.DataCategory = dto.DataCategory.Trim();
            inventory.Purpose = dto.Purpose.Trim();
            inventory.DataSource = dto.DataSource?.Trim();
            inventory.StorageLocation = dto.StorageLocation?.Trim();
            inventory.RetentionPeriodDays = dto.RetentionPeriodDays;
            inventory.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(inventory);

            return true;
        }

        public async Task<bool> DeleteAsync(
            Guid id,
            Guid organizationId)
        {
            var inventory = await _repository.GetByIdAsync(
                id, organizationId);

            if (inventory == null)
                return false;

            await _repository.DeleteAsync(inventory);

            return true;
        }

        private static DataInventoryResponseDto MapToResponseDto(
            DataInventory inventory)
        {
            return new DataInventoryResponseDto
            {
                DataInventoryId = inventory.DataInventoryId,
                Name = inventory.Name,
                DataCategory = inventory.DataCategory,
                Purpose = inventory.Purpose,
                DataSource = inventory.DataSource,
                StorageLocation = inventory.StorageLocation,
                RetentionPeriodDays = inventory.RetentionPeriodDays,
                Status = inventory.Status,
                CreatedAt = inventory.CreatedAt,
                UpdatedAt = inventory.UpdatedAt
            };
        }
    }
}