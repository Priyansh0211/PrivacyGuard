using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IConsentService
    {
        // NAYA METHOD: UI Filters ke liye Controller ko support karne ke liye
        Task<IEnumerable<ConsentRecord>> GetFilteredRecordsAsync(Guid organizationId, string? reference, string? status, string? purpose, string? channel, DateTime? fromDate, DateTime? toDate);

        Task<IEnumerable<ConsentPurpose>> GetPurposesAsync(Guid organizationId);

        Task<(bool IsSuccess, string Message, string ReceiptId)> RecordConsentAsync(CreateConsentRequestDto request, Guid organizationId);

        Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid organizationId);
        Task<object?> GetLatestConsentAsync(Guid dataPrincipalId, Guid organizationId);
        Task<IEnumerable<object>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId);

        Task<ConsentPurpose> CreatePurposeAsync(CreatePurposeDto dto, Guid organizationId);

        Task<List<PurposeCheckboxItemDto>> GetPurposesForCheckboxesAsync(Guid orgId);
        // ==========================================
        // MASTER DATA (Categories & Processors)
        // ==========================================
        Task<IEnumerable<DataCategory>> GetCategoriesAsync(Guid organizationId);
        Task<DataCategory> AddCategoryAsync(CreateDataCategoryDto request, Guid organizationId);

        Task<IEnumerable<DataProcessor>> GetProcessorsAsync(Guid organizationId);
        Task<DataProcessor> AddProcessorAsync(CreateDataProcessorDto request, Guid organizationId);
    }
}