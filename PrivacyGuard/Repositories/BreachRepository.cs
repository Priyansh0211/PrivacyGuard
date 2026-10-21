using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;
using Microsoft.EntityFrameworkCore;

namespace PrivacyGuard.Repositories
{
    public class BreachRepository : IBreachRepository
    {
        private readonly ApplicationDbContext _context;

        public BreachRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataBreachIncident> CreateIncidentAsync(DataBreachIncident incident)
        {
            _context.DataBreachIncidents.Add(incident);
            await _context.SaveChangesAsync();
            return incident;
        }

        public async Task<List<DataBreachIncident>> GetAllIncidentsAsync(Guid organizationId)
        {
            // Include lagaya hai taaki Reporter aur Assigned Admin ka naam bhi fetch ho jaye
            return await _context.DataBreachIncidents
                .Include(b => b.ReportedByUser)
                .Include(b => b.AssignedUser)
                .Where(b => b.OrganizationId == organizationId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<DataBreachIncident?> GetIncidentByIdAsync(Guid incidentId, Guid organizationId)
        {
            return await _context.DataBreachIncidents
                .FirstOrDefaultAsync(b => b.Id == incidentId && b.OrganizationId == organizationId);
        }

        public async Task UpdateIncidentAsync(DataBreachIncident incident)
        {
            _context.DataBreachIncidents.Update(incident);
            await _context.SaveChangesAsync();

        }
    }
}
