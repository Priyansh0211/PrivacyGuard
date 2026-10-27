using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using PrivacyGuard.Repositories;

namespace PrivacyGuard.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _repository;
        private readonly IStaffRepository _staffRepository; 
        public StaffService(IStaffRepository repository)
        {
            _repository = repository;
            _staffRepository = repository;
        }

        public async Task<(bool IsSuccess, StaffResponseDto? Staff, string ErrorMessage)> CreateStaffAsync(CreateStaffDto dto, Guid organizationId)
        {
            if (await _repository.StaffExistsAsync(dto.Email))
            {
                return (false, null, "User with this email already exists.");
            }

            var newStaff = new User
            {
                FullName = dto.Name, // <-- YEH MISSING HAI
                Email = dto.Email,
                PasswordHash = dto.Password,
                Role = dto.Role,
                OrganizationId = organizationId,
                OrgUnitId = dto.OrgUnitId,
                IsActive = true
            };

            await _repository.AddStaffAsync(newStaff);
            await _repository.SaveChangesAsync();

            var response = new StaffResponseDto
            {
                UserId = newStaff.UserId,
                Name = newStaff.FullName, 
                Email = newStaff.Email,
                Role = newStaff.Role,
                OrganizationId = newStaff.OrganizationId,
                OrgUnitId = newStaff.OrgUnitId,
                IsActive = newStaff.IsActive
            };

            return (true, response, string.Empty);
        }
        public async Task<List<StaffResponseDto>> GetStaffListAsync(Guid organizationId)
        {
            // Database se data repository ke through la rahe hain
            var staffUsers = await _staffRepository.GetStaffByOrganizationIdAsync(organizationId);

            // Usko DTO mein map kar rahe hain
            return staffUsers.Select(u => new StaffResponseDto
            {
                UserId = u.UserId, // Agar Id string hai toh Guid.Parse(u.Id) kar lena
                Name = u.FullName,
                Email = u.Email,
                Role = u.Role,
                OrganizationId = u.OrganizationId,
                OrgUnitId = u.OrgUnitId,
                OrgUnitName = u.OrgUnit != null ? u.OrgUnit.Name : "Unassigned",
                IsActive = true // ya u.IsActive agar DB mein hai
            }).ToList();
        }
    }
}