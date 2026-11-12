using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IOrgUnitRepository _repository;

        public OrgUnitService(IOrgUnitRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrgUnitResponseDto> CreateOrgUnitAsync(CreateOrgUnitDto dto, Guid organizationId)
        {
            var orgUnit = new OrgUnit
            {
                OrganizationId = organizationId,
                Name = dto.Name,
                UnitType = dto.UnitType,
                UnitCode = dto.UnitCode,
                ParentId = dto.ParentId,
                IsActive = true
            };

            await _repository.AddAsync(orgUnit);
            await _repository.SaveChangesAsync();

            return new OrgUnitResponseDto
            {
                Id = orgUnit.Id,
                OrganizationId = orgUnit.OrganizationId,
                Name = orgUnit.Name,
                UnitType = orgUnit.UnitType,
                UnitCode = orgUnit.UnitCode,
                ParentId = orgUnit.ParentId,
                IsActive = orgUnit.IsActive,
                PrincipalsCount = 0 // Naya banaya hai toh count 0 hi rahega
            };
        }

        public async Task<List<OrgUnitResponseDto>> GetHierarchyAsync(Guid organizationId, string role, Guid userOrgUnitId)
        {
            var allUnits = await _repository.GetAllByOrgIdAsync(organizationId);

            var dict = new Dictionary<Guid, OrgUnitResponseDto>();

            // FIX 2: Hardcoded 0 ki jagah Repository se actual DB count mangwa rahe hain
            foreach (var u in allUnits)
            {
                var principalsCount = await _repository.GetPrincipalsCountAsync(u.Id);

                dict.Add(u.Id, new OrgUnitResponseDto
                {
                    Id = u.Id,
                    OrganizationId = u.OrganizationId,
                    Name = u.Name,
                    UnitType = u.UnitType,
                    UnitCode = u.UnitCode,
                    PrincipalsCount = principalsCount, // Actual count mapped here
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

            // FIX 1: "SystemAdmin" ko add kar diya aur ek fallback (string.IsNullOrEmpty) de diya
            // Kyunki controller par [HasPermission] pehle hi security handle kar raha hai
            if (role == "Admin" || role == "SuperAdmin" || role == "SystemAdmin" || string.IsNullOrEmpty(role))
            {
                return tree;
            }
            else if (role == "BranchAdmin" && dict.ContainsKey(userOrgUnitId))
            {
                return new List<OrgUnitResponseDto> { dict[userOrgUnitId] };
            }

            return new List<OrgUnitResponseDto>();
        }

        public async Task<bool> DeactivateOrgUnitAsync(Guid id, Guid organizationId)
        {
            var unit = await _repository.GetByIdAsync(id, organizationId);
            if (unit == null) return false;

            unit.IsActive = false;
            _repository.Update(unit);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateOrgUnitAsync(Guid id, Guid organizationId)
        {
            var unit = await _repository.GetByIdAsync(id, organizationId);
            if (unit == null) return false;

            unit.IsActive = true;
            _repository.Update(unit);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<OrgUnitResponseDto?> UpdateOrgUnitAsync(Guid id, Guid organizationId, UpdateOrgUnitDto dto)
        {
            var unit = await _repository.GetByIdAsync(id, organizationId);
            if (unit == null) return null;

            unit.Name = dto.Name;
            unit.UnitCode = dto.UnitCode;

            _repository.Update(unit);
            await _repository.SaveChangesAsync();

            return new OrgUnitResponseDto
            {
                Id = unit.Id,
                OrganizationId = unit.OrganizationId,
                Name = unit.Name,
                UnitType = unit.UnitType,
                UnitCode = unit.UnitCode,
                ParentId = unit.ParentId,
                IsActive = unit.IsActive,
                PrincipalsCount = await _repository.GetPrincipalsCountAsync(unit.Id)
            };
        }

        public async Task<bool> DeleteOrgUnitAsync(Guid id, Guid organizationId)
        {
            var unit = await _repository.GetByIdAsync(id, organizationId);
            if (unit == null) return false;

            bool hasChildren = await _repository.HasChildrenAsync(id);
            if (hasChildren)
            {
                return false;
            }

            _repository.Delete(unit);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}