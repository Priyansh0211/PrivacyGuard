using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

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
                OrganizationId = organizationId, // Controller se aayi hui ID save ho rahi hai
                Name = dto.Name,
                UnitType = dto.UnitType,
                UnitCode = dto.UnitCode, // <-- DATABASE MAPPING
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
                UnitCode = orgUnit.UnitCode, // <-- POST RESPONSE MAPPING
                ParentId = orgUnit.ParentId,
                IsActive = orgUnit.IsActive,
                PrincipalsCount = 0
            };
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

        public async Task<List<OrgUnitResponseDto>> GetHierarchyAsync(Guid organizationId, string role, Guid userOrgUnitId)
        {
            var allUnits = await _repository.GetAllByOrgIdAsync(organizationId);

            var dict = allUnits.Select(u => new OrgUnitResponseDto
            {
                Id = u.Id,
                OrganizationId = u.OrganizationId,
                Name = u.Name,
                UnitType = u.UnitType,
                UnitCode = u.UnitCode,      // <-- Yeh UI ke tags ke liye zaroori hai (e.g., "zone-amravati")
                PrincipalsCount = 0,        // <-- Abhi ke liye 0, baad mein Users table se link karenge
                ParentId = u.ParentId,
                IsActive = u.IsActive
            }).ToDictionary(u => u.Id);

            var tree = new List<OrgUnitResponseDto>();

            foreach (var item in dict.Values)
            {
                if (item.ParentId.HasValue && dict.ContainsKey(item.ParentId.Value))
                {
                    dict[item.ParentId.Value].Children.Add(item);
                }
                else
                {
                    tree.Add(item); // Sirf Zone (Top Level) yahan add honge
                }
            }

            // RBAC FILTER LOGIC:
            if (role == "Admin" || role == "SuperAdmin")
            {
                return tree; 
            }
            else if (role == "BranchAdmin" && dict.ContainsKey(userOrgUnitId))
            {
                // BranchAdmin ko sirf uska branch aur uske sub-branches (children) do
                return new List<OrgUnitResponseDto> { dict[userOrgUnitId] };
            }

            return new List<OrgUnitResponseDto>(); // Agar role match na ho toh empty
        }
    }
}