using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IStaffService
    {
        Task<(bool IsSuccess, StaffResponseDto? Staff, string ErrorMessage)> CreateStaffAsync(CreateStaffDto dto, Guid organizationId);
        Task<List<StaffResponseDto>> GetStaffListAsync(Guid organizationId);
    }
}