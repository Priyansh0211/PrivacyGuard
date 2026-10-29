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

        // Yeh line galti se do baar thi, ab maine ek hi baar rakhi hai
        Task<(bool IsSuccess, string Message, string ReceiptId)> RecordConsentAsync(CreateConsentRequestDto request, Guid organizationId);

        Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid organizationId);
        Task<object?> GetLatestConsentAsync(Guid dataPrincipalId, Guid organizationId);
        Task<IEnumerable<object>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId);

        Task<ConsentPurpose> CreatePurposeAsync(CreatePurposeDto dto, Guid organizationId);

        Task<List<PurposeCheckboxItemDto>> GetPurposesForCheckboxesAsync(Guid orgId);
    }
}