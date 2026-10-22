using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class PrivacyRequestRepository : IPrivacyRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public PrivacyRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PrivacyRequest> AddAsync(PrivacyRequest request)
        {
            await _context.PrivacyRequests.AddAsync(request);
            return request;
        }

        public async Task<PrivacyRequest?> GetByIdAsync(Guid requestId, Guid organizationId)
        {
            // RequestId aur OrganizationId dono match hone chahiye (Security)
            return await _context.PrivacyRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.OrganizationId == organizationId);
        }

        public async Task<IEnumerable<PrivacyRequest>> GetAllByUserAsync(string dataPrincipalId, Guid organizationId)
        {
            // Nayi request sabse upar dikhe isliye OrderByDescending lagaya hai
            return await _context.PrivacyRequests
                .Where(r => r.DataPrincipalId == dataPrincipalId && r.OrganizationId == organizationId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public void Update(PrivacyRequest request)
        {
            _context.PrivacyRequests.Update(request);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}