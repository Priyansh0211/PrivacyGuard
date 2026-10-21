using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    // Business logic ke method define kiye gaye hain
    public interface IConsentService
    {
        // Consent grant karne ka method
        Task<ConsentRecords> GrantConsentAsync(GrantConsentDto request, Guid organizationId);

        // Consent wapas lene (withdraw) ka method, true/false return karega
        Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid organizationId);

        // History nikalne ka method
        Task<IEnumerable<ConsentRecords>> GetHistoryAsync(string dataPrincipalId, Guid organizationId);
    }
}