using PrivacyGuard.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IOrgUnitService
    {
        Task<OrgUnitResponseDto> CreateOrgUnitAsync(CreateOrgUnitDto dto, Guid organizationId);
        Task<List<OrgUnitResponseDto>> GetHierarchyAsync(Guid organizationId, string role, Guid userOrgUnitId);

        Task<bool> DeactivateOrgUnitAsync(Guid id, Guid organizationId);
        Task<bool> ActivateOrgUnitAsync(Guid id, Guid organizationId); // Naya

        Task<OrgUnitResponseDto?> UpdateOrgUnitAsync(Guid id, Guid organizationId, UpdateOrgUnitDto dto); // Naya
        Task<bool> DeleteOrgUnitAsync(Guid id, Guid organizationId); // Naya
    }
}