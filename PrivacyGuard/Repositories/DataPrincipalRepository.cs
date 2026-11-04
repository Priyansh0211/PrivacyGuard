using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // .Include() ke liye ye bahut zaroori hai
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class DataPrincipalRepository : IDataPrincipalRepository
    {
        private readonly ApplicationDbContext _context;

        public DataPrincipalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataPrincipal> AddDataPrincipalAsync(DataPrincipal dataPrincipal)
        {
            await _context.DataPrincipals.AddAsync(dataPrincipal);
            return dataPrincipal;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // ==========================================
        // NAYA METHOD: EF Core Query for 360-View
        // ==========================================
        public async Task<DataPrincipal?> Get360ViewEntityAsync(Guid dataPrincipalId, Guid organizationId)
        {
            // Ek hi query mein DataPrincipal ke sath uski teeno related lists fetch kar li
            return await _context.DataPrincipals
                .Include(dp => dp.Identifiers)
                .Include(dp => dp.PrivacyRequests)
                .Include(dp => dp.ConsentRecords)
                .FirstOrDefaultAsync(dp => dp.DataPrincipalId == dataPrincipalId
                                        && dp.OrganizationId == organizationId);
        }
    }
}