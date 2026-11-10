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
                .Where(r => r.DataPrincipalId == Guid.Parse(dataPrincipalId) && r.OrganizationId == organizationId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        public async Task<(List<PrivacyRequest> Requests, int TotalCount)> GetFilteredRequestsAsync(
    Guid organizationId, string? search, PrivacyRequestStatus? status, PrivacyRequestType? type, bool overdueOnly, int page, int pageSize)
        {
            // Include DataPrincipal so we can search by their name or show it in the grid
            var query = _context.PrivacyRequests
                .Include(r => r.DataPrincipal)
                .Where(r => r.OrganizationId == organizationId)
                .AsQueryable();

            // 1. Search Filter (Reference ID or Principal Name/Email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.ReferenceId.ToLower().Contains(search) ||
                    (r.DataPrincipal != null && r.DataPrincipal.FullName != null && r.DataPrincipal.FullName.ToLower().Contains(search)) ||
                    (r.DataPrincipal != null && r.DataPrincipal.Email != null && r.DataPrincipal.Email.ToLower().Contains(search))
                );
            }

            // 2. Status Filter
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            // 3. Type Filter
            if (type.HasValue)
            {
                query = query.Where(r => r.RequestType == type.Value);
            }

            // 4. Overdue Only Filter (If target date is passed and it's not resolved)
            if (overdueOnly)
            {
                query = query.Where(r =>
                    r.TargetResolutionDate < DateTime.UtcNow &&
                    r.Status != PrivacyRequestStatus.Fulfilled &&
                    r.Status != PrivacyRequestStatus.Rejected &&
                    r.Status != PrivacyRequestStatus.Withdrawn);
            }

            int totalCount = await query.CountAsync();

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (requests, totalCount);
        }
        public async Task<PrivacyRequest?> GetRequestDetailsAsync(Guid requestId, Guid organizationId)
        {
            // Include DataPrincipal so admin can see who raised it
            return await _context.PrivacyRequests
                .Include(r => r.DataPrincipal)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.OrganizationId == organizationId);
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