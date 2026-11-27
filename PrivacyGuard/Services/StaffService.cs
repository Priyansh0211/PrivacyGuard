using PrivacyGuard.DTOs;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IAccessScopeService _scope;
        public StaffService(IStaffRepository repository, IAccessScopeService scope)
        {
            _staffRepository = repository;
            _scope = scope;
        }

        public async Task<(bool IsSuccess, StaffResponseDto? Staff, string ErrorMessage)> CreateStaffAsync(
    CreateStaffDto dto, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId)
        {
            if (await _staffRepository.StaffExistsAsync(dto.Email))
            {
                return (false, null, "User with this email already exists.");
            }

            var role = await _staffRepository.GetRoleInOrganizationAsync(dto.RoleId, organizationId);
            if (role == null)
            {
                return (false, null, "Selected role is invalid for this organization.");
            }

            if (RoleGuard.IsPrivilegedRole(role.Name) && !RoleGuard.IsSuperRole(callerRole))
            {
                return (false, null, "You are not allowed to assign this role.");
            }

            // Role ki saari permissions caller ke paas bhi honi chahiye
            if (!await _scope.CanAssignRoleAsync(callerUserId, callerRole, role.Id))
            {
                return (false, null, "You cannot assign a role that has permissions you do not have.");
            }

            var requestedUnitId = dto.OrgUnitId == Guid.Empty ? null : dto.OrgUnitId;

            // Branch scoping: scoped caller sirf apni branch me (aur unit dena zaroori)
            var allowedUnits = await _scope.GetAllowedUnitIdsAsync(organizationId, callerRole, callerUnitId);
            if (allowedUnits != null)
            {
                if (!requestedUnitId.HasValue)
                    return (false, null, "Please select an org unit within your branch.");

                if (!allowedUnits.Contains(requestedUnitId.Value))
                    return (false, null, "You can add staff only within your own branch.");
            }

            OrgUnit? orgUnit = null;
            if (requestedUnitId.HasValue)
            {
                orgUnit = await _staffRepository.GetActiveOrgUnitInOrganizationAsync(requestedUnitId.Value, organizationId);
                if (orgUnit == null)
                {
                    return (false, null, "Selected org unit is invalid or inactive.");
                }
            }

            var now = DateTime.UtcNow;
            var newStaff = new User
            {
                FullName = dto.Name,
                Email = dto.Email,
                PasswordHash = PasswordHelper.Hash(dto.Password),
                ReferenceId = PrivacyHelper.GenerateUserReferenceId(),
                RoleId = role.Id,
                OrganizationId = organizationId,
                OrgUnitId = orgUnit?.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _staffRepository.AddStaffAsync(newStaff);
            await _staffRepository.SaveChangesAsync();

            var response = new StaffResponseDto
            {
                UserId = newStaff.UserId,
                Name = newStaff.FullName,
                Email = newStaff.Email,
                Role = role.Name,
                OrganizationId = newStaff.OrganizationId,
                OrgUnitId = newStaff.OrgUnitId,
                OrgUnitName = orgUnit?.Name ?? "Unassigned",
                IsActive = newStaff.IsActive
            };

            return (true, response, string.Empty);
        }
        public async Task<List<StaffResponseDto>> GetStaffListAsync(Guid organizationId, string? callerRole, Guid callerUnitId)
        {
            var staffUsers = await _staffRepository.GetStaffByOrganizationIdAsync(organizationId);

            // Branch scoping: scoped caller ko sirf apni branch ka staff
            var allowedUnits = await _scope.GetAllowedUnitIdsAsync(organizationId, callerRole, callerUnitId);
            if (allowedUnits != null)
            {
                staffUsers = staffUsers
                    .Where(u => u.OrgUnitId.HasValue && allowedUnits.Contains(u.OrgUnitId.Value))
                    .ToList();
            }

            return staffUsers.Select(u => new StaffResponseDto
            {
                UserId = u.UserId,
                Name = u.FullName,
                Email = u.Email,
                Role = u.UserRole != null ? u.UserRole.Name : "Unassigned",
                OrganizationId = u.OrganizationId,
                OrgUnitId = u.OrgUnitId,
                OrgUnitName = u.OrgUnit != null ? u.OrgUnit.Name : "Unassigned",
                IsActive = u.IsActive
            }).ToList();
        }
    }
}