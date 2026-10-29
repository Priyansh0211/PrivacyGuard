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
    public class NoticeTemplateRepository : INoticeTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public NoticeTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NoticeTemplate> CreateTemplateAsync(NoticeTemplate template)
        {
            _context.NoticeTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<List<NoticeTemplate>> GetAllTemplatesAsync(Guid organizationId)
        {
            return await _context.NoticeTemplates
                .Where(t => t.OrganizationId == organizationId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<NoticeTemplate?> GetTemplateByIdAsync(Guid id, Guid organizationId)
        {
            return await _context.NoticeTemplates
                .FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == organizationId);
        }

        public async Task<NoticeTemplate> UpdateTemplateAsync(NoticeTemplate template)
        {
            _context.NoticeTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

       
        public async Task<bool> DeleteTemplateAsync(Guid id, Guid organizationId)
        {
            var template = await _context.NoticeTemplates
                .FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == organizationId);

            if (template == null) return false;

            _context.NoticeTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}