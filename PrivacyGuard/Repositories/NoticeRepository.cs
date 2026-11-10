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
    public class NoticeRepository : INoticeRepository
    {
        private readonly ApplicationDbContext _context;

        public NoticeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notice>> GetAllNoticesAsync(Guid orgId)
        {
            return await _context.Notices
                .Include(n => n.Translations)
                .Where(n => n.OrganizationId == orgId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notice?> GetNoticeByIdAsync(Guid id, Guid orgId)
        {
            return await _context.Notices
                .Include(n => n.Translations)
                .FirstOrDefaultAsync(n => n.Id == id && n.OrganizationId == orgId);
        }

        public async Task<Notice> CreateNoticeAsync(Notice notice)
        {
            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();
            return notice;
        }

        public async Task UpdateNoticeAsync(Notice notice)
        {
            // सिर्फ SaveChangesAsync कॉल करना है, Update() नहीं। EF Core ट्रैक कर रहा है।
            await _context.SaveChangesAsync();
        }

        public async Task<NoticeTemplate?> GetTemplateByIdAsync(Guid templateId)
        {
            return await _context.NoticeTemplates.FindAsync(templateId);
        }

        public async Task UpdateNoticeWithTranslationsAsync(Notice notice, List<NoticeTranslation> newTranslations)
        {
            
            if (notice.Translations != null)
            {
                foreach (var oldTranslation in notice.Translations.ToList())
                {
                    _context.Entry(oldTranslation).State = EntityState.Detached;
                }
                notice.Translations.Clear(); 
            }

            await _context.NoticeTranslations
                          .Where(t => t.NoticeId == notice.Id)
                          .ExecuteDeleteAsync();

            
            foreach (var t in newTranslations)
            {
                t.NoticeId = notice.Id;
                _context.NoticeTranslations.Add(t);
            }

            
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notice>> GetPublishedNoticesByOrgAsync(Guid orgId)
        {
           
            return await _context.Notices
                .Include(n => n.Translations)
                .Where(n => n.OrganizationId == orgId && n.Status == "Published")
                .ToListAsync();
        }

        public async Task<Notice> GetLatestPublishedNoticeByCodeAsync(Guid orgId, string code)
        {
            return await _context.Notices
                .Include(n => n.Translations)
                .Where(n => n.OrganizationId == orgId && n.Code == code && n.Status == "Published")
                .OrderByDescending(n => n.Version)
                .FirstOrDefaultAsync();
        }
    }
}