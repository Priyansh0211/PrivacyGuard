using PrivacyGuard.DTOs;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccessScopeService _scope;
        public UserService(IUserRepository userRepository, IAccessScopeService scope)
        {
            _userRepository = userRepository;
            _scope = scope;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddEmployeeAsync(CreateUserDto request, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId)
        {
            bool emailExists = await _userRepository.CheckEmailExistsAsync(request.Email);
            if (emailExists)
            {
                return (false, "This email is already registered.");
            }

            var role = await _userRepository.GetRoleInOrganizationAsync(request.RoleId, organizationId);
            if (role == null)
            {
                return (false, "Selected role is invalid for this organization.");
            }

            if (RoleGuard.IsPrivilegedRole(role.Name) && !RoleGuard.IsSuperRole(callerRole))
            {
                return (false, "You are not allowed to assign this role.");
            }

            // Role ki saari permissions caller ke paas bhi honi chahiye
            if (!await _scope.CanAssignRoleAsync(callerUserId, callerRole, role.Id))
            {
                return (false, "You cannot assign a role that has permissions you do not have.");
            }

            var requestedUnitId = request.OrgUnitId == Guid.Empty ? null : request.OrgUnitId;

            // Branch scoping: scoped caller sirf apni branch me (aur unit dena zaroori)
            var allowedUnits = await _scope.GetAllowedUnitIdsAsync(organizationId, callerRole, callerUnitId);
            if (allowedUnits != null)
            {
                if (!requestedUnitId.HasValue)
                    return (false, "Please select an org unit within your branch.");

                if (!allowedUnits.Contains(requestedUnitId.Value))
                    return (false, "You can add users only within your own branch.");
            }

            Guid? orgUnitId = null;
            if (requestedUnitId.HasValue)
            {
                var unit = await _userRepository.GetActiveOrgUnitInOrganizationAsync(requestedUnitId.Value, organizationId);
                if (unit == null)
                {
                    return (false, "Selected org unit is invalid or inactive.");
                }
                orgUnitId = unit.Id;
            }

            var currentTime = DateTime.UtcNow;
            var newUser = new User
            {
                OrganizationId = organizationId,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = PasswordHelper.Hash(request.Password),
                ReferenceId = PrivacyHelper.GenerateUserReferenceId(),
                RoleId = role.Id,
                OrgUnitId = orgUnitId,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = currentTime
            };

            await _userRepository.CreateUserAsync(newUser);

            return (true, string.Empty);
        }

        public async Task<List<UserResponseDto>> GetOrganizationEmployeesAsync(Guid organizationId, string? callerRole, Guid callerUnitId)
        {
            var users = await _userRepository.GetUsersByOrganizationIdAsync(organizationId);
            var dataPrincipals = await _userRepository.GetDataPrincipalsByOrganizationIdAsync(organizationId);

            // Branch scoping: scoped caller ko sirf apni branch ke log
            var allowedUnits = await _scope.GetAllowedUnitIdsAsync(organizationId, callerRole, callerUnitId);
            if (allowedUnits != null)
            {
                users = users
                    .Where(u => u.OrgUnitId.HasValue && allowedUnits.Contains(u.OrgUnitId.Value))
                    .ToList();
                dataPrincipals = dataPrincipals
                    .Where(dp => dp.OrgUnitId.HasValue && allowedUnits.Contains(dp.OrgUnitId.Value))
                    .ToList();
            }
            var userList = users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                ReferenceId = u.ReferenceId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,

                // 🚨 UPDATE: String Role ki jagah Role table se Name nikalenge
                Role = u.UserRole != null ? u.UserRole.Name : "No Role Assigned",

                IsActive = u.IsActive,
                OrgUnitId = u.OrgUnitId,
                OrgUnitName = u.OrgUnit?.Name,
                CreatedAt = u.CreatedAt
            }).ToList();

            userList.AddRange(dataPrincipals.Select(dp => new UserResponseDto
            {
                UserId = dp.DataPrincipalId,
                ReferenceId = dp.ReferenceId,
                FullName = dp.FullName ?? dp.DisplayName ?? dp.ReferenceId,
                Email = dp.Email ?? string.Empty,
                PhoneNumber = dp.Mobile ?? string.Empty,
                Role = "DataPrincipal",
                IsActive = dp.IsActive,
                OrgUnitId = dp.OrgUnitId,
                CreatedAt = dp.CreatedAt
            }));

            return userList;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ToggleUserStatusAsync(Guid userId, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId)
        {
            var user = await _userRepository.GetUserByIdAndOrgAsync(userId, organizationId);

            if (user == null)
            {
                return (false, "User not found or you don't have permission to modify this user.");
            }

            // Branch scoping
            var allowedUnits = await _scope.GetAllowedUnitIdsAsync(organizationId, callerRole, callerUnitId);
            if (allowedUnits != null && (!user.OrgUnitId.HasValue || !allowedUnits.Contains(user.OrgUnitId.Value)))
            {
                return (false, "You can manage users only within your own branch.");
            }

            // Khud ka account deactivate/activate nahi kar sakte
            if (user.UserId == callerUserId)
            {
                return (false, "You cannot change the status of your own account.");
            }

            // Jis user ke paas tumse zyada access hai usse modify nahi kar sakte
            if (user.RoleId.HasValue && !await _scope.CanAssignRoleAsync(callerUserId, callerRole, user.RoleId.Value))
            {
                return (false, "You cannot modify a user who has more access than you.");
            }

            // Admin level accounts deactivate nahi ho sakte
            if (user.IsActive && RoleGuard.IsPrivilegedRole(user.UserRole?.Name))
            {
                return (false, "Admin accounts cannot be deactivated.");
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);

            return (true, string.Empty);
        }
        public async Task<(bool IsSuccess, string ErrorMessage)> UpdateEmployeeAsync(Guid userId, UpdateUserDto request, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId)
        {
            var user = await _userRepository.GetUserByIdAndOrgAsync(userId, organizationId);
            if (user == null)
            {
                return (false, "User not found or you don't have permission to modify this user.");
            }

            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            {
                return (false, "Full name and email are required.");
            }

            bool callerIsSuper = RoleGuard.IsSuperRole(callerRole);

            // Branch scoping: target user apni branch me hona chahiye
            var allowedUnits = await _scope.GetAllowedUnitIdsAsync(organizationId, callerRole, callerUnitId);
            if (allowedUnits != null && (!user.OrgUnitId.HasValue || !allowedUnits.Contains(user.OrgUnitId.Value)))
            {
                return (false, "You can manage users only within your own branch.");
            }

            if (RoleGuard.IsPrivilegedRole(user.UserRole?.Name) && !callerIsSuper)
            {
                return (false, "You are not allowed to modify an admin account.");
            }

            // Jis user ke paas tumse zyada access hai usse edit nahi kar sakte
            if (user.RoleId.HasValue && !await _scope.CanAssignRoleAsync(callerUserId, callerRole, user.RoleId.Value))
            {
                return (false, "You cannot modify a user who has more access than you.");
            }

            var newRole = await _userRepository.GetRoleInOrganizationAsync(request.RoleId, organizationId);
            if (newRole == null)
            {
                return (false, "Selected role is invalid for this organization.");
            }

            if (RoleGuard.IsPrivilegedRole(newRole.Name) && !callerIsSuper)
            {
                return (false, "You are not allowed to assign this role.");
            }

            bool roleChanged = user.RoleId != request.RoleId;

            if (user.UserId == callerUserId && roleChanged)
            {
                return (false, "You cannot change your own role.");
            }

            // Role badal raha hai to nayi role ki permissions caller ke paas honi chahiye
            if (roleChanged && !await _scope.CanAssignRoleAsync(callerUserId, callerRole, newRole.Id))
            {
                return (false, "You cannot assign a role that has permissions you do not have.");
            }

            // OrgUnit: null = no change, Guid.Empty = unit se hata do, value = nayi unit
            Guid? newOrgUnitId = user.OrgUnitId;
            if (request.OrgUnitId.HasValue)
            {
                if (request.OrgUnitId.Value == Guid.Empty)
                {
                    // Scoped caller user ko branch se bahar nahi nikal sakta
                    if (allowedUnits != null)
                        return (false, "You cannot remove a user from your branch.");

                    newOrgUnitId = null;
                }
                else if (request.OrgUnitId.Value != user.OrgUnitId)
                {
                    if (allowedUnits != null && !allowedUnits.Contains(request.OrgUnitId.Value))
                        return (false, "You can move users only within your own branch.");

                    var unit = await _userRepository.GetActiveOrgUnitInOrganizationAsync(request.OrgUnitId.Value, organizationId);
                    if (unit == null)
                    {
                        return (false, "Selected org unit is invalid or inactive.");
                    }
                    newOrgUnitId = unit.Id;
                }
            }

            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                bool emailExists = await _userRepository.CheckEmailExistsAsync(request.Email);
                if (emailExists)
                {
                    return (false, "This email is already in use by another account.");
                }
            }

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.RoleId = newRole.Id;
            user.OrgUnitId = newOrgUnitId;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);

            return (true, string.Empty);
        }
    }
}