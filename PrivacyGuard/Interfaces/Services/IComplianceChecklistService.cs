using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IComplianceChecklistService
    {
        Task<ComplianceChecklistDto> GetChecklistAsync(Guid organizationId);
    }
}
