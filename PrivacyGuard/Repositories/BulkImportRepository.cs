using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class BulkImportRepository : IBulkImportRepository
    {
        private readonly ApplicationDbContext _context;

        public BulkImportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddBatchAsync(BulkImportBatch batch)
        {
            await _context.BulkImportBatches.AddAsync(batch);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // DB se saare batches uthana
        public async Task<IEnumerable<BulkImportBatch>> GetBatchesAsync(Guid organizationId, string? kind, string? status)
        {
            var query = _context.BulkImportBatches.Where(b => b.OrganizationId == organizationId);

            // Kind (ImportType) ka filter add kiya, aur "Any" (case-insensitive) ko ignore kiya
            if (!string.IsNullOrEmpty(kind) && kind.ToLower() != "any")
            {
                query = query.Where(b => b.ImportType == kind);
            }

            // Status ka filter update kiya taaki UI ka "Any" bhejne par code phate nahi
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "any")
            {
                query = query.Where(b => b.Status == status);
            }

            return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        // DB se ek batch uthana
        public async Task<BulkImportBatch?> GetBatchByIdAsync(Guid batchId, Guid organizationId)
        {
            return await _context.BulkImportBatches
                .FirstOrDefaultAsync(b => b.BatchId == batchId && b.OrganizationId == organizationId);
        }

        // DB se paginated rows uthana
        public async Task<IEnumerable<BulkImportRow>> GetBatchRowsAsync(Guid batchId, string? status, int page, int pageSize)
        {
            var query = _context.BulkImportRows.Where(r => r.BatchId == batchId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            return await query.OrderBy(r => r.RowNumber)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
        }

        // ... (purane methods ke neeche yeh add karo)

        public async Task<List<BulkImportRow>> GetValidRowsForExecutionAsync(Guid batchId)
        {
            // Sirf valid rows utha rahe hain execution ke liye
            return await _context.BulkImportRows
                .Where(r => r.BatchId == batchId && r.Status == "Valid")
                .ToListAsync();
        }

        public async Task AddConsumersAsync(IEnumerable<DataPrincipal> consumers)
        {
            await _context.DataPrincipals.AddRangeAsync(consumers);
        }

        public async Task UpdateBatchAndRowsAsync(BulkImportBatch batch, List<BulkImportRow> rows)
        {
            _context.BulkImportBatches.Update(batch);
            _context.BulkImportRows.UpdateRange(rows);
            await _context.SaveChangesAsync();
        }
    }
}