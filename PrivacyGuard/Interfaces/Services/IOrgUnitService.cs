using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IOrgUnitService
    {
        Task<OrgUnitResponseDto> CreateOrgUnitAsync(CreateOrgUnitDto dto, Guid organizationId);
        Task<bool> DeactivateOrgUnitAsync(Guid id, Guid organizationId);
        Task<List<OrgUnitResponseDto>> GetHierarchyAsync(Guid organizationId, string role, Guid userOrgUnitId);
    }
}