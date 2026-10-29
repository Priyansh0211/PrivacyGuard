using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Repositories
{
    public class ConsentRepository : IConsentRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // NAYA METHOD: UI Filters apply karne ke liye
        // NAYA METHOD: UI Filters apply karne ke liye (Permanent Fix with Enums)
        public async Task<IEnumerable<ConsentRecord>> GetFilteredRecordsAsync(
            Guid organizationId, string? reference, string? status, string? purpose, string? channel, DateTime? fromDate, DateTime? toDate)
        {
            // DataPrincipal ka Include yahan se hata diya hai taaki purana error wapas na aaye
            var query = _context.ConsentRecords
                .Include(c => c.ConsentDetails)
                .ThenInclude(d => d.ConsentPurpose)
                .Where(c => c.OrganizationId == organizationId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(reference))
            {
                // Seedha ConsentRecord ki property use kar rahe hain
                query = query.Where(c => c.ReferenceNumber.Contains(reference) || c.ReceiptId.Contains(reference));
            }

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "any status")
            {
                // String ko Enum me safe tarike se convert karke compare kar rahe hain
                if (Enum.TryParse<ConsentStatus>(status, true, out var parsedStatus))
                {
                    query = query.Where(c => c.OverallStatus == parsedStatus);
                }
            }

            if (!string.IsNullOrEmpty(channel) && channel.ToLower() != "any channel")
            {
                // String ko Enum me safe tarike se convert karke compare kar rahe hain
                if (Enum.TryParse<ConsentChannel>(channel, true, out var parsedChannel))
                {
                    query = query.Where(c => c.Channel == parsedChannel);
                }
            }

            if (!string.IsNullOrEmpty(purpose) && purpose.ToLower() != "any purpose")
            {
                // 'Name' ki jagah tumhara correct DB column 'ShortName' lagaya hai
                query = query.Where(c => c.ConsentDetails.Any(d => d.ConsentPurpose.ShortName == purpose));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= toDate.Value.AddDays(1).AddTicks(-1));
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }
        public async Task<IEnumerable<ConsentPurpose>> GetPurposesByOrgAsync(Guid organizationId)
        {
            return await _context.ConsentPurposes
                .Where(p => p.OrganizationId == organizationId && p.IsMandatory == false || p.IsMandatory == true)
                .ToListAsync();
        }

        public async Task<ConsentRecord> AddAsync(ConsentRecord consent)
        {
            await _context.ConsentRecords.AddAsync(consent);
            return consent;
        }

        public async Task<ConsentRecord?> GetByIdAsync(Guid consentRecordId, Guid organizationId)
        {
            return await _context.ConsentRecords
                .Include(c => c.ConsentDetails)
                .FirstOrDefaultAsync(c => c.ConsentRecordId == consentRecordId && c.OrganizationId == organizationId);
        }

        public void Update(ConsentRecord consent)
        {
            _context.ConsentRecords.Update(consent);
        }

        public async Task<IEnumerable<ConsentRecord>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId)
        {
            return await _context.ConsentRecords
                .Include(c => c.ConsentDetails)
                .ThenInclude(d => d.ConsentPurpose)
                .Where(c => c.DataPrincipalId == dataPrincipalId && c.OrganizationId == organizationId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<ConsentRecord?> GetLatestConsentByPrincipalIdAsync(Guid dataPrincipalId, Guid organizationId)
        {
            return await _context.ConsentRecords
                .Include(c => c.ConsentDetails)
                .ThenInclude(d => d.ConsentPurpose)
                .Where(c => c.DataPrincipalId == dataPrincipalId && c.OrganizationId == organizationId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task AddPurposeAsync(ConsentPurpose purpose)
        {
            await _context.ConsentPurposes.AddAsync(purpose);

            if (purpose.Categories != null && purpose.Categories.Any())
            {
                await _context.ConsentPurposeDataCategories.AddRangeAsync(purpose.Categories);
            }
            if (purpose.Processors != null && purpose.Processors.Any())
            {
                await _context.ConsentPurposeDataProcessors.AddRangeAsync(purpose.Processors);
            }

            await _context.SaveChangesAsync();
        }
<<<<<<< Updated upstream

        public async Task<List<ConsentPurpose>> GetAllPurposesByOrgAsync(Guid organizationId)
        {
            
            return await _context.ConsentPurposes
                .Where(p => p.OrganizationId == organizationId)
                .OrderBy(p => p.ShortName)
                .ToListAsync();
        }

=======
>>>>>>> Stashed changes
    }
}