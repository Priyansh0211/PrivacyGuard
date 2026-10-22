using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IPrivacyRequestService
    {
        // User jab nayi request raise karega
        Task<PrivacyRequest> RaiseRequestAsync(CreatePrivacyRequestDto requestDto, Guid organizationId);

        // Admin/DPO jab request resolve karega
        Task<bool> ResolveRequestAsync(Guid requestId, ResolvePrivacyRequestDto resolveDto, Guid organizationId);

        // User apni requests ki history dekhega
        Task<IEnumerable<PrivacyRequest>> GetMyRequestsAsync(string dataPrincipalId, Guid organizationId);
    }
}