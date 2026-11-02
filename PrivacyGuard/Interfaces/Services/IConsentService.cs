using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IConsentService
    {
        // UI Filters ke liye Controller ko support karne ke liye
        Task<IEnumerable<object>> GetFilteredRecordsAsync(Guid organizationId, string? reference, string? status, string? purpose, string? channel, DateTime? fromDate, DateTime? toDate);

        Task<object?> GetConsentRecordByIdAsync(Guid consentRecordId, Guid organizationId);
        Task<IEnumerable<ConsentPurpose>> GetPurposesAsync(Guid organizationId);

        Task<(bool IsSuccess, string Message, string ReceiptId)> RecordConsentAsync(CreateConsentRequestDto request, Guid organizationId);

        Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid organizationId);
        Task<object?> GetLatestConsentAsync(Guid dataPrincipalId, Guid organizationId);
        Task<IEnumerable<object>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId);

        Task<ConsentPurpose> CreatePurposeAsync(CreatePurposeDto dto, Guid organizationId);

        Task<List<PurposeCheckboxItemDto>> GetPurposesForCheckboxesAsync(Guid orgId);

        // ==========================================
        // NAYA: Purpose Edit & History
        // ==========================================
        Task<ConsentPurpose?> EditPurposeAsync(Guid purposeId, CreatePurposeDto dto, Guid organizationId);
        Task<IEnumerable<object>> GetPurposeHistoryAsync(Guid purposeId, Guid organizationId);

        // ==========================================
        // MASTER DATA (Categories & Processors)
        // ==========================================
        Task<IEnumerable<DataCategory>> GetCategoriesAsync(Guid organizationId);
        Task<DataCategory> AddCategoryAsync(CreateDataCategoryDto request, Guid organizationId);

        Task<DataCategory?> EditCategoryAsync(Guid id, CreateDataCategoryDto dto, Guid organizationId);

        Task<IEnumerable<DataProcessor>> GetProcessorsAsync(Guid organizationId);
        Task<DataProcessor> AddProcessorAsync(CreateDataProcessorDto request, Guid organizationId);

        Task<DataProcessor?> EditProcessorAsync(Guid id, CreateDataProcessorDto dto, Guid organizationId);
    }
}