using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IBulkImportRepository
    {
        Task AddBatchAsync(BulkImportBatch batch);
        Task SaveChangesAsync();

        Task<IEnumerable<BulkImportBatch>> GetBatchesAsync(Guid organizationId, string? kind, string? status);
        Task<BulkImportBatch?> GetBatchByIdAsync(Guid batchId, Guid organizationId);
        Task<IEnumerable<BulkImportRow>> GetBatchRowsAsync(Guid batchId, string? status, int page, int pageSize);

        // EXECUTION METHODS
        Task<List<BulkImportRow>> GetValidRowsForExecutionAsync(Guid batchId);
        Task AddConsumersAsync(IEnumerable<DataPrincipal> consumers);
        Task UpdateBatchAndRowsAsync(BulkImportBatch batch, List<BulkImportRow> rows);
    }
}