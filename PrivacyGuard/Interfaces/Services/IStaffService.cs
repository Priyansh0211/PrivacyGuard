using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IStaffService
    {
        Task<(bool IsSuccess, StaffResponseDto? Staff, string ErrorMessage)> CreateStaffAsync(
            CreateStaffDto dto, Guid organizationId, Guid callerUserId, string? callerRole, Guid callerUnitId);

        Task<List<StaffResponseDto>> GetStaffListAsync(Guid organizationId, string? callerRole, Guid callerUnitId);
    }
}