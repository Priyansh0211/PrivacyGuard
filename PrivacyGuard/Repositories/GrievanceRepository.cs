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
    public class GrievanceRepository : IGrievanceRepository
    {
        private readonly ApplicationDbContext _context;
        public GrievanceRepository(ApplicationDbContext context) { _context = context; }

        public async Task<Grievance> CreateGrievanceAsync(Grievance grievance)
        {
            _context.Grievances.Add(grievance);
            await _context.SaveChangesAsync();
            return grievance;
        }

        public async Task<List<Grievance>> GetAllGrievancesAsync(Guid organizationId)
        {
            return await _context.Grievances
                .Include(g => g.DataPrincipal)
                .Where(g => g.OrganizationId == organizationId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<Grievance?> GetGrievanceByIdAsync(Guid id, Guid organizationId)
        {
            return await _context.Grievances
                .Include(g => g.DataPrincipal)
                .Include(g => g.Messages)
                .FirstOrDefaultAsync(g => g.Id == id && g.OrganizationId == organizationId);
        }

        public async Task AddMessageAsync(GrievanceMessage message)
        {
            _context.GrievanceMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGrievanceAsync(Grievance grievance)
        {
            _context.Grievances.Update(grievance);
            await _context.SaveChangesAsync();
        }
    }
}

