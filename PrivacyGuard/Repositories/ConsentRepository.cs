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

        public async Task<IEnumerable<ConsentPurpose>> GetPurposesByOrgAsync(Guid organizationId)
        {
            return await _context.ConsentPurposes
                .Where(p => p.OrganizationId == organizationId && p.IsMandatory == false || p.IsMandatory == true) // Sabhi purposes
                .ToListAsync();
        }

        public async Task<ConsentRecord> AddAsync(ConsentRecord consent)
        {
            await _context.ConsentRecords.AddAsync(consent);
            return consent;
        }

        public async Task<ConsentRecord?> GetByIdAsync(Guid consentRecordId, Guid organizationId)
        {
            // Update/Withdraw ke liye eager loading chahiye
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
            // PDF Fix: Missing Include in History Endpoint[cite: 4]
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
            // Purpose save karo
            await _context.ConsentPurposes.AddAsync(purpose);

            // Mappings save karo (agar hain toh)
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

        public async Task<List<ConsentPurpose>> GetAllPurposesByOrgAsync(Guid organizationId)
        {
            
            return await _context.ConsentPurposes
                .Where(p => p.OrganizationId == organizationId)
                .OrderBy(p => p.ShortName)
                .ToListAsync();
        }

    }
}