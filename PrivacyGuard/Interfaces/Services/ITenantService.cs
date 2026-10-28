using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface ITenantService
    {
        Task<(bool IsSuccess, Guid? OrganizationId, Guid? AdminId, string ErrorMessage)> RegisterTenantAsync(RegisterTenantDto request);

        Task<(bool IsSuccess, string Token, string ErrorMessage)> LoginAsync(LoginDto request, string? loginIp);

        Task<List<RoleDropdownDto>> GetRolesForDropdownAsync(Guid organizationId);
    }

}
