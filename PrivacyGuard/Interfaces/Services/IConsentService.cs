using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IConsentService
    {
        Task<IEnumerable<ConsentPurpose>> GetPurposesAsync(Guid organizationId);
        Task<(bool IsSuccess, string Message, string ReceiptId)> RecordConsentAsync(CreateConsentRequestDto request, Guid organizationId);
        
        // Note: Withdraw mein ab sirf consentRecordId chahiye, kyunki DataInventory hata kar humne Purpose entity bana di hai.
        Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid organizationId);
        Task<object?> GetLatestConsentAsync(Guid dataPrincipalId, Guid organizationId);
        Task<IEnumerable<object>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId);

        Task<ConsentPurpose> CreatePurposeAsync(string code, string shortName, bool isMandatory, Guid organizationId);
    }
}