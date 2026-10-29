using PrivacyGuard.DTOs;



namespace PrivacyGuard.Interfaces
{
    public interface IUserService
    {
        Task<(bool IsSuccess, string ErrorMessage)> AddEmployeeAsync(CreateUserDto request, Guid organizationId);

        
        Task<List<UserResponseDto>> GetOrganizationEmployeesAsync(Guid organizationId);

        
        Task<(bool IsSuccess, string ErrorMessage)> ToggleUserStatusAsync(Guid userId, Guid organizationId);

        Task<(bool IsSuccess, string ErrorMessage)> UpdateEmployeeAsync(Guid userId, UpdateUserDto request, Guid organizationId);
    }
}