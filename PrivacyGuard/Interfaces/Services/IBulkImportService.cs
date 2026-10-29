using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IBulkImportService
    {
        Task<(bool IsSuccess, Guid? BatchId, string ErrorMessage)> StageImportBatchAsync(BulkImportRequestDto request, Guid organizationId, Guid userId);

        // NAYE GET METHODS (Business format ke liye)
        Task<IEnumerable<BulkImportBatch>> GetBatchesAsync(Guid organizationId, string? status);

        // Return type object hai kyunki hum anomymous formatted object bhejenge UI ke liye
        Task<object?> GetBatchDetailsAsync(Guid batchId, Guid organizationId, string? rowStatus, int page, int pageSize);

        // EXECUTION METHOD
        Task<(bool IsSuccess, string Message)> ExecuteImportBatchAsync(Guid batchId, Guid organizationId);
    }
}