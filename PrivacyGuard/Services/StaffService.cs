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
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;

        public StaffService(IStaffRepository repository)
        {
            _staffRepository = repository;
        }

        public async Task<(bool IsSuccess, StaffResponseDto? Staff, string ErrorMessage)> CreateStaffAsync(CreateStaffDto dto, Guid organizationId)
        {
            if (await _staffRepository.StaffExistsAsync(dto.Email))
            {
                return (false, null, "User with this email already exists.");
            }

            var newStaff = new User
            {
                FullName = dto.Name,
                Email = dto.Email,
                PasswordHash = dto.Password,

                // FIX 1: Ab yahan direct string nahi, balki RoleId (Guid) save hoga
                RoleId = dto.RoleId, // (Note: Agar CreateStaffDto mein purana 'Role' string tha, toh usko update karke Guid RoleId kar dena)

                OrganizationId = organizationId,
                OrgUnitId = dto.OrgUnitId,
                IsActive = true
            };

            await _staffRepository.AddStaffAsync(newStaff);
            await _staffRepository.SaveChangesAsync();

            var response = new StaffResponseDto
            {
                UserId = newStaff.UserId,
                Name = newStaff.FullName,
                Email = newStaff.Email,

                // FIX 2: Response DTO mein string chahiye, toh relation se Name nikalenge
                Role = newStaff.UserRole != null ? newStaff.UserRole.Name : "Unassigned",

                OrganizationId = newStaff.OrganizationId,
                OrgUnitId = newStaff.OrgUnitId,
                IsActive = newStaff.IsActive
            };

            return (true, response, string.Empty);
        }

        public async Task<List<StaffResponseDto>> GetStaffListAsync(Guid organizationId)
        {
            var staffUsers = await _staffRepository.GetStaffByOrganizationIdAsync(organizationId);

            return staffUsers.Select(u => new StaffResponseDto
            {
                UserId = u.UserId,
                Name = u.FullName,
                Email = u.Email,

                // FIX 3: Same yahan bhi UserRole.Name use hoga
                Role = u.UserRole != null ? u.UserRole.Name : "Unassigned",

                OrganizationId = u.OrganizationId,
                OrgUnitId = u.OrgUnitId,
                OrgUnitName = u.OrgUnit != null ? u.OrgUnit.Name : "Unassigned",
                IsActive = u.IsActive
            }).ToList();
        }
    }
}