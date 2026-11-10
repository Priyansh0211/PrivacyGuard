using System;
using System.Threading.Tasks;
using PrivacyGuard.Data;
using PrivacyGuard.Models;
using PrivacyGuard.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PrivacyGuard.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ReportJob> CreateJobAsync(ReportJob job)
        {
            await _dbContext.ReportJobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();
            return job;
        }

        public async Task<ReportJob?> GetJobByIdAsync(Guid jobId)
        {
            return await _dbContext.ReportJobs.FindAsync(jobId);
        }

        public async Task<ReportJob> UpdateJobAsync(ReportJob job)
        {
            _dbContext.ReportJobs.Update(job);
            await _dbContext.SaveChangesAsync();
            return job;
        }

        public async Task SaveGeneratedReportAsync(GeneratedReport report)
        {
            await _dbContext.GeneratedReports.AddAsync(report);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReportJob>> GetJobsByOrganizationAsync(Guid organizationId)
        {
            return await _dbContext.ReportJobs
                .Where(j => j.OrganizationId == organizationId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<GeneratedReport?> GetReportByJobIdAsync(Guid jobId, Guid organizationId)
        {
            // Tenant isolation yahan bhi zaroori hai taaki koi dusri org ka report download na kar le
            return await _dbContext.GeneratedReports
                .FirstOrDefaultAsync(r => r.JobId == jobId && r.OrganizationId == organizationId);
        }


    }
}