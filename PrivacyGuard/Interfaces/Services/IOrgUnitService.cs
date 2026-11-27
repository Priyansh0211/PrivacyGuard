using PrivacyGuard.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IOrgUnitService
    {
        Task<OrgUnitResponseDto> CreateOrgUnitAsync(CreateOrgUnitDto dto, Guid organizationId, string? callerRole, Guid callerUnitId);
        Task<List<OrgUnitResponseDto>> GetHierarchyAsync(Guid organizationId, string role, Guid userOrgUnitId);
        Task<bool> DeactivateOrgUnitAsync(Guid id, Guid organizationId, string? callerRole, Guid callerUnitId);
        Task<bool> ActivateOrgUnitAsync(Guid id, Guid organizationId, string? callerRole, Guid callerUnitId);
        Task<OrgUnitResponseDto?> UpdateOrgUnitAsync(Guid id, Guid organizationId, UpdateOrgUnitDto dto, string? callerRole, Guid callerUnitId);
        Task<bool> DeleteOrgUnitAsync(Guid id, Guid organizationId, string? callerRole, Guid callerUnitId);
    }
}