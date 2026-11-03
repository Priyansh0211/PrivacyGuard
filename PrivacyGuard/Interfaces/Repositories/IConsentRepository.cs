using PrivacyGuard.DTOs;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IConsentRepository
    {
        Task<IEnumerable<ConsentRecord>> GetFilteredRecordsAsync(Guid organizationId, string? reference, string? status, string? purpose, string? channel, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<ConsentPurpose>> GetPurposesByOrgAsync(Guid organizationId);
        Task<ConsentRecord?> GetLatestConsentByPrincipalIdAsync(Guid dataPrincipalId, Guid organizationId);
        Task<ConsentRecord> AddAsync(ConsentRecord consent);
        Task<ConsentRecord?> GetByIdAsync(Guid consentRecordId, Guid organizationId);
        void Update(ConsentRecord consent);
        Task<IEnumerable<ConsentRecord>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId);
        Task<int> SaveChangesAsync();

        Task AddPurposeAsync(ConsentPurpose purpose);
        Task<List<ConsentPurpose>> GetAllPurposesByOrgAsync(Guid organizationId);

        // ==========================================
        // NAYE METHODS (CS1061 Errors fix karne ke liye)
        // ==========================================
        Task<ConsentPurpose?> GetPurposeByIdAsync(Guid purposeId, Guid organizationId);
        Task<IEnumerable<ConsentPurpose>> GetPurposeHistoryAsync(string code, Guid organizationId);

        // ==========================================
        // MASTER DATA (Categories & Processors)
        // ==========================================
        Task<IEnumerable<DataCategory>> GetCategoriesAsync(Guid organizationId);
        Task<DataCategory> AddCategoryAsync(DataCategory category);
        Task<DataCategory?> GetCategoryByIdAsync(Guid id, Guid organizationId);

        Task<IEnumerable<DataProcessor>> GetProcessorsAsync(Guid organizationId);
        Task<DataProcessor> AddProcessorAsync(DataProcessor processor);
        Task<DataProcessor?> GetProcessorByIdAsync(Guid id, Guid organizationId);
    }
}