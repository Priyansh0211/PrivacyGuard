using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class DpiaRepository : IDpiaRepository
    {
        private readonly ApplicationDbContext _context;

        public DpiaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DpiaListDto>> GetAllDpiaAsync(Guid orgId)
        {
            return await _context.DpiaRecords
                .Where(d => d.OrganizationId == orgId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DpiaListDto
                {
                    Id = d.Id,
                    ReferenceId = d.ReferenceId,
                    Title = d.Title,
                    PurposesCount = d.PurposesCount,
                    ResidualRisk = d.ResidualRisk,
                    Status = d.Status
                })
                .ToListAsync();
        }

        public async Task<DpiaRecord?> GetDpiaByIdAsync(Guid id, Guid orgId)
        {
            return await _context.DpiaRecords
                .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == orgId);
        }

        public async Task<DpiaRecord> CreateDpiaAsync(CreateDpiaDto dto, string createdByUserId, Guid orgId)
        {
            int count = await _context.DpiaRecords.Where(d => d.OrganizationId == orgId).CountAsync() + 1;
            string newReference = $"DPI-{DateTime.UtcNow.Year}-{count:D6}";

            var record = new DpiaRecord
            {
                OrganizationId = orgId,
                ReferenceId = newReference,
                Title = dto.Title,
                PurposesCount = dto.PurposesCount,
                ResidualRisk = dto.ResidualRisk,
                AssessmentDetails = dto.AssessmentDetails,
                Status = "In Review",
                CreatedByUserId = createdByUserId,
                NextReviewDate = DateTime.UtcNow.AddYears(1)
            };

            _context.DpiaRecords.Add(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<bool> ReviewDpiaAsync(Guid id, Guid orgId, ReviewDpiaDto dto, string reviewedByUserId)
        {
            var record = await _context.DpiaRecords
                .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == orgId);

            if (record == null) return false;

            record.ReviewNotes = dto.ReviewNotes;
            record.ReviewedByUserId = reviewedByUserId;
            record.ReviewedAt = DateTime.UtcNow;

            if (dto.Action == "Approve")
                record.Status = "Approved";
            else if (dto.Action == "SendBack")
                record.Status = "Needs Changes";

            await _context.SaveChangesAsync();
            return true;
        }
    }
}