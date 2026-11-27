using PrivacyGuard.DTOs;



namespace PrivacyGuard.Interfaces
{
    public interface IUserService
    {
        Task<(bool IsSuccess, string ErrorMessage)> AddEmployeeAsync(CreateUserDto request, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId);

        Task<List<UserResponseDto>> GetOrganizationEmployeesAsync(Guid organizationId, string? callerRole, Guid callerUnitId);

        Task<(bool IsSuccess, string ErrorMessage)> ToggleUserStatusAsync(Guid userId, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId);

        Task<(bool IsSuccess, string ErrorMessage)> UpdateEmployeeAsync(Guid userId, UpdateUserDto request, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId);
        Task<(bool IsSuccess, bool NotFound, string ErrorMessage)> AssignOrgUnitAsync(
    Guid userId, Guid? orgUnitId, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId);
    }
}