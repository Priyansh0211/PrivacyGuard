using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IConsentService
    {
        // Naya Granular Submit method (Task<ConsentRecord> return karega)
        Task<ConsentRecord> SubmitGranularConsentAsync(SubmitConsentDto request, Guid organizationId);

        // Naya Withdraw method jisme dataInventoryId bhi hai
        Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid dataInventoryId, Guid organizationId);

        // History wala method
        Task<IEnumerable<ConsentRecord>> GetHistoryAsync(string dataPrincipalId, Guid organizationId);
    }
}