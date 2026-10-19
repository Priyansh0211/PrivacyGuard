using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    // IConsentRepository interface ko implement kar rahe hain
    public class ConsentRepository : IConsentRepository
    {
        // Database context ka private variable banaya
        private readonly ApplicationDbContext _context;

        // Constructor mein DbContext inject kiya (Dependency Injection)
        public ConsentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Naya consent record table me insert karega
        public async Task<ConsentRecords> AddAsync(ConsentRecords consent)
        {
            // ConsentRecords ko table me add karne ke liye track me daala
            await _context.ConsentRecords.AddAsync(consent);
            return consent; // Wapas bhej diya
        }

        // ID ke basis par ek single record nikalega
        public async Task<ConsentRecords?> GetByIdAsync(Guid consentRecordId, Guid organizationId)
        {
            // FirstOrDefaultAsync se match karne wala pehla record layega
            return await _context.ConsentRecords
                .FirstOrDefaultAsync(c => c.ConsentRecordId == consentRecordId && c.OrganizationId == organizationId);
        }

        // Record ko update karega
        public void Update(ConsentRecords consent)
        {
            // Table mein record ko modify mode me set karega
            _context.ConsentRecords.Update(consent);
        }

        // User ki consent history database se uthayega
        public async Task<IEnumerable<ConsentRecords>> GetHistoryAsync(string dataPrincipalId, Guid organizationId)
        {
            // Where condition laga kar sirf us user aur uski organization ka data layega
            return await _context.ConsentRecords
                .Where(c => c.DataPrincipalId == dataPrincipalId && c.OrganizationId == organizationId)
                .OrderByDescending(c => c.GrantedAt) // Naya data sabse upar aayega
                .ToListAsync(); // List bana kar return karega
        }

        // Jo bhi Add ya Update kiya hai, usko actually Database me save karega
        public async Task<int> SaveChangesAsync()
        {
            // SaveChangesAsync EF Core ka in-built function hai SQL query run karne ke liye
            return await _context.SaveChangesAsync();
        }
    }
}