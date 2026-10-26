using PrivacyGuard.DTOs;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddEmployeeAsync(CreateUserDto request, Guid organizationId)
        {
            // 1. Validate karein ki Email pehle se use toh nahi hua
            bool emailExists = await _userRepository.CheckEmailExistsAsync(request.Email);
            if (emailExists)
            {
                return (false, "This email is already registered.");
            }

            // 2. Naya User model map karein
            var currentTime = DateTime.UtcNow;
            var newUser = new User
            {
                OrganizationId = organizationId,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = request.Password,
                ReferenceId = PrivacyHelper.GenerateUserReferenceId(),

                RoleId = request.RoleId,

                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = currentTime
            };

            // 3. Database mein save karein
            await _userRepository.CreateUserAsync(newUser);

            return (true, string.Empty);
        }

        public async Task<List<UserResponseDto>> GetOrganizationEmployeesAsync(Guid organizationId)
        {
            var users = await _userRepository.GetUsersByOrganizationIdAsync(organizationId);
            var userList = users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,

                // 🚨 UPDATE: String Role ki jagah Role table se Name nikalenge
                Role = u.UserRole != null ? u.UserRole.Name : "No Role Assigned",

                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();

            return userList;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ToggleUserStatusAsync(Guid userId, Guid organizationId)
        {
            var user = await _userRepository.GetUserByIdAndOrgAsync(userId, organizationId);

            if (user == null)
            {
                return (false, "User not found or you don't have permission to modify this user.");
            }

            // 🚨 UPDATE: Navigation property (UserRole) ka use karke Name check karenge
            if (user.UserRole != null && user.UserRole.Name == "Admin")
            {
                return (false, "You cannot deactivate an Admin account.");
            }

            user.IsActive = !user.IsActive; // True hai toh False, False hai toh True
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);

            return (true, string.Empty);
        }
    }
}