using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IOrgUnitRepository _repository;
        private readonly IAccessScopeService _scope;
        public OrgUnitService(IOrgUnitRepository repository, IAccessScopeService scope)
        {
            _repository = repository;
            _scope = scope;
        }

        public async Task<OrgUnitResponseDto> CreateOrgUnitAsync(CreateOrgUnitDto dto, Guid organizationId, string? callerRole, Guid callerUnitId)
        {
            var name = dto.Name?.Trim();
            var unitType = dto.UnitType?.Trim();
            var unitCode = dto.UnitCode?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(unitType) || string.IsNullOrWhiteSpace(unitCode))
                throw new InvalidOperationException("Name, unit type and unit code are required.");

            // Parent (agar diya hai) isi organization ki honi chahiye
            var parentId = dto.ParentId == Guid.Empty ? null : dto.ParentId;
            var allowedUnits = await _scope.GetAllowedUnitIdsAsync(organizationId, callerRole, callerUnitId);
            if (allowedUnits != null && (!parentId.HasValue || !allowedUnits.Contains(parentId.Value)))
                throw new OrgUnitForbiddenException("You can create units only under your own branch.");
            if (parentId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(parentId.Value, organizationId);
                if (parent == null)
                    throw new InvalidOperationException("Parent org unit not found in this organization.");
            }

            if (!await _repository.IsUnitCodeUniqueAsync(unitCode, organizationId))
                throw new OrgUnitConflictException($"Unit code '{unitCode}' already exists in this organization.");

            var orgUnit = new OrgUnit
            {
                OrganizationId = organizationId,
                Name = name,
                UnitType = unitType,
                UnitCode = unitCode,
                ParentId = parentId,
                IsActive = true
            };

            await _repository.AddAsync(orgUnit);
            await SaveUnitAsync();

            return new OrgUnitResponseDto
            {
                Id = orgUnit.Id,
                OrganizationId = orgUnit.OrganizationId,
                Name = orgUnit.Name,
                UnitType = orgUnit.UnitType,
                UnitCode = orgUnit.UnitCode,
                ParentId = orgUnit.ParentId,
                IsActive = orgUnit.IsActive,
                PrincipalsCount = 0,
                StaffCount = 0,
                DataPrincipalCount = 0
            };
        }
        public async Task<List<OrgUnitResponseDto>> GetHierarchyAsync(Guid organizationId, string role, Guid userOrgUnitId)
        {
            var allUnits = (await _repository.GetAllByOrgIdAsync(organizationId)).ToList();

            // 3 queries total (N+1 nahi), fir memory me roll-up
            var staffDirect = await _repository.GetStaffCountsByOrgUnitAsync(organizationId);
            var principalsDirect = await _repository.GetDataPrincipalCountsByOrgUnitAsync(organizationId);
            var rollup = BuildRollup(allUnits, staffDirect, principalsDirect);

            var dict = new Dictionary<Guid, OrgUnitResponseDto>();

            foreach (var u in allUnits)
            {
                rollup.TryGetValue(u.Id, out var counts);

                dict.Add(u.Id, new OrgUnitResponseDto
                {
                    Id = u.Id,
                    OrganizationId = u.OrganizationId,
                    Name = u.Name,
                    UnitType = u.UnitType,
                    UnitCode = u.UnitCode,
                    StaffCount = counts.Staff,
                    DataPrincipalCount = counts.Principals,
                    PrincipalsCount = counts.Staff, // backward compatibility
                    ParentId = u.ParentId,
                    IsActive = u.IsActive,
                    Children = new List<OrgUnitResponseDto>()
                });
            }

            var tree = new List<OrgUnitResponseDto>();

            foreach (var item in dict.Values)
            {
                if (item.ParentId.HasValue && dict.ContainsKey(item.ParentId.Value))
                {
                    dict[item.ParentId.Value].Children.Add(item);
                }
                else
                {
                    tree.Add(item);
                }
            }

            // Admin-level ya unit-less users ko poora tree, unit wale (branch) users ko sirf apna subtree
            if (!RoleGuard.IsBranchScoped(role, userOrgUnitId))
                return tree;

            if (dict.TryGetValue(userOrgUnitId, out var branchRoot))
                return new List<OrgUnitResponseDto> { branchRoot };

            return new List<OrgUnitResponseDto>();
        }

        public async Task<bool> DeactivateOrgUnitAsync(Guid id, Guid organizationId, string? callerRole, Guid callerUnitId)
        {
            var unit = await _repository.GetByIdAsync(id, organizationId);
            if (unit == null) return false;
            await EnsureUnitInScopeAsync(organizationId, id, callerRole, callerUnitId);
            if (RoleGuard.IsBranchScoped(callerRole, callerUnitId) && id == callerUnitId)
                throw new OrgUnitForbiddenException("You cannot deactivate or delete your own branch.");
            if (!unit.IsActive) return true; // pehle se inactive hai

            if (await _repository.HasActiveChildrenAsync(id))
                throw new OrgUnitConflictException("This unit has active child units. Deactivate them first.");

            if (await _repository.HasActiveStaffAsync(id))
                throw new OrgUnitConflictException("Active staff are assigned to this unit. Move or deactivate them first.");

            unit.IsActive = false;
            _repository.Update(unit);
            await SaveUnitAsync();

            return true;
        }

        public async Task<bool> ActivateOrgUnitAsync(Guid id, Guid organizationId, string? callerRole, Guid callerUnitId)
        {
            var unit = await _repository.GetByIdAsync(id, organizationId);
            if (unit == null) return false;
            await EnsureUnitInScopeAsync(organizationId, id, callerRole, callerUnitId);
            if (unit.IsActive) return true; // pehle se active hai

            if (unit.ParentId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(unit.ParentId.Value, organizationId);
                if (parent != null && !parent.IsActive)
                    throw new OrgUnitConflictException("The parent unit is inactive. Activate it first.");
            }

            unit.IsActive = true;
            _repository.Update(unit);
            await SaveUnitAsync();

            return true;
        }

        public async Task<OrgUnitResponseDto?> UpdateOrgUnitAsync(Guid id, Guid organizationId, UpdateOrgUnitDto dto, string? callerRole, Guid callerUnitId)
        {
            var unit = await _repository.GetByIdAsync(id, organizationId);
            if (unit == null) return null;
            await EnsureUnitInScopeAsync(organizationId, id, callerRole, callerUnitId);

            var name = dto.Name?.Trim();
            var unitCode = dto.UnitCode?.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(unitCode))
                throw new InvalidOperationException("Name and unit code are required.");

            // Apna hi code ho to chalega (excludeId = id)
            if (!await _repository.IsUnitCodeUniqueAsync(unitCode, organizationId, id))
                throw new OrgUnitConflictException($"Unit code '{unitCode}' already exists in this organization.");

            unit.Name = name;
            unit.UnitCode = unitCode;

            _repository.Update(unit);
            await SaveUnitAsync();

            var allUnits = (await _repository.GetAllByOrgIdAsync(organizationId)).ToList();
            var rollup = BuildRollup(
                allUnits,
                await _repository.GetStaffCountsByOrgUnitAsync(organizationId),
                await _repository.GetDataPrincipalCountsByOrgUnitAsync(organizationId));
            rollup.TryGetValue(unit.Id, out var counts);

            return new OrgUnitResponseDto
            {
                Id = unit.Id,
                OrganizationId = unit.OrganizationId,
                Name = unit.Name,
                UnitType = unit.UnitType,
                UnitCode = unit.UnitCode,
                ParentId = unit.ParentId,
                IsActive = unit.IsActive,
                StaffCount = counts.Staff,
                DataPrincipalCount = counts.Principals,
                PrincipalsCount = counts.Staff
            };
        }

        public async Task<bool> DeleteOrgUnitAsync(Guid id, Guid organizationId, string? callerRole, Guid callerUnitId)
        {
            var unit = await _repository.GetByIdAsync(id, organizationId);
            if (unit == null) return false;
            await EnsureUnitInScopeAsync(organizationId, id, callerRole, callerUnitId);
            if (RoleGuard.IsBranchScoped(callerRole, callerUnitId) && id == callerUnitId)
                throw new OrgUnitForbiddenException("You cannot deactivate or delete your own branch.");

            if (await _repository.HasChildrenAsync(id))
                throw new OrgUnitConflictException("This unit has child units. Delete them first.");

            if (await _repository.HasAssignedUsersAsync(id))
                throw new OrgUnitConflictException("Staff are assigned to this unit. Move them to another unit first.");

            if (await _repository.HasAssignedDataPrincipalsAsync(id))
                throw new OrgUnitConflictException("Data principals are assigned to this unit and it cannot be deleted.");

            _repository.Delete(unit);
            await SaveUnitAsync();

            return true;
        }
        // Har unit ke liye: apna + saari child units ka count (roll-up)
        private static Dictionary<Guid, (int Staff, int Principals)> BuildRollup(
            List<OrgUnit> units,
            Dictionary<Guid, int> staffDirect,
            Dictionary<Guid, int> principalsDirect)
        {
            var childrenMap = units
                .Where(u => u.ParentId.HasValue)
                .GroupBy(u => u.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            var result = new Dictionary<Guid, (int Staff, int Principals)>();

            (int Staff, int Principals) Sum(Guid id, HashSet<Guid> path)
            {
                if (result.TryGetValue(id, out var cached)) return cached;
                if (!path.Add(id)) return (0, 0); // cycle guard

                staffDirect.TryGetValue(id, out var staff);
                principalsDirect.TryGetValue(id, out var principals);

                if (childrenMap.TryGetValue(id, out var kids))
                {
                    foreach (var kid in kids)
                    {
                        var c = Sum(kid, path);
                        staff += c.Staff;
                        principals += c.Principals;
                    }
                }

                path.Remove(id);
                result[id] = (staff, principals);
                return (staff, principals);
            }

            foreach (var u in units) Sum(u.Id, new HashSet<Guid>());
            return result;
        }
        private async Task SaveUnitAsync()
        {
            try
            {
                await _repository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Race condition: DB ke unique index (OrganizationId, UnitCode) ya kisi FK ne rok diya
                throw new OrgUnitConflictException("Could not save the org unit. The unit code may already exist or the unit is in use.");
            }
        }
        private async Task EnsureUnitInScopeAsync(Guid organizationId, Guid targetUnitId, string? callerRole, Guid callerUnitId)
        {
            var allowed = await _scope.GetAllowedUnitIdsAsync(organizationId, callerRole, callerUnitId);
            if (allowed == null) return; // org-wide

            if (!allowed.Contains(targetUnitId))
                throw new OrgUnitForbiddenException("You can manage units only within your own branch.");
        }
    }
}